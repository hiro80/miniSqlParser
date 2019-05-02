using System.Collections.Generic;
using System.ComponentModel;
using Antlr4.Runtime;

namespace MiniSqlParser
{
  internal partial class MakeASTListener: MiniSqlParserBaseListener
  {
    private BufferedTokenStream _tokens;
    private Stack<INode> _stack = new Stack<INode>();
    // SqlAccessorの文法制限例外
    private SqlSyntaxErrorsException _saSyntaxErrorsException;

    public MakeASTListener(BufferedTokenStream tokens
                         , DBMSType dbmsType = DBMSType.Unknown
                         , bool forSqlAccessor = false) {
      _tokens = tokens;
      this.ForSqlAccessor = forSqlAccessor;
      this.DBMSType = dbmsType;
    }

    public DBMSType DBMSType { get; private set; }
    public bool ForSqlAccessor { get; private set; }

    public INode GetAST() {
      if(_stack.Count > 1) {
        throw new CannotBuildASTException("Node Stack is under flow");
      }
      return _stack.Peek();
    }

    public override void ExitStmts(MiniSqlParserParser.StmtsContext context) {
      var stmts = new List<Stmt>();

      // SQL文リストの先頭以外のSQL文をstmtsオブジェクトに格納する
      for(int i = context.stmt_sub().Length - 1; i >= 0;  --i) {
        var stmt = (Stmt)_stack.Pop();
        var semicolonCount2 = context._c2[i].ChildCount;
        var comments2 = this.GetComments(context._c2[i]);
        // 次のSQL文の先頭SCOLを、末尾のセミコロンと見做す
        if(context.SCOL().Length > i + 1) {
          ++semicolonCount2;
          comments2.AddRange(this.GetComments(context.SCOL()[i+1]));
        }
        stmt.AppendSeparators(semicolonCount2, comments2);
        stmts.Insert(0, stmt);
      }

      // SQL文リストの先頭のSQL文をstmtsオブジェクトに格納する
      var firstStmt = (Stmt)_stack.Pop();
      var semicolonCount1 = context.c1.ChildCount;
      var comments1 = this.GetComments(context.c1);
      if(context.SCOL().Length > 0) {
        ++semicolonCount1;
        comments1.AddRange(this.GetComments(context.SCOL()[0]));
      }
      firstStmt.AppendSeparators(semicolonCount1, comments1);
      stmts.Insert(0, firstStmt);

      var node = new Stmts(stmts, null);

      //SQL文の先頭のコメントを次のSQL文のHeaderCommentに移動する
      this.ShiftLastCommentToNextStmt(node);

      _stack.Push(node);
    }

    public override void ExitSelect_stmt(MiniSqlParserParser.Select_stmtContext context) {
      ForUpdateClause forUpdate = null;
      if(context.for_update_clause() != null) {
        forUpdate = (ForUpdateClause)_stack.Pop();
      }
      IQuery query = (IQuery)_stack.Pop();
      //query.IsSubQuery = false;
      WithClause withClause = null;
      if(context.with_clause() != null) {
        withClause = (WithClause)_stack.Pop();
      }
      var node = new SelectStmt(withClause, query, forUpdate);

      // SELECT文の先頭コメント、プレースホルダ初期値及びAutoWhere値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
      node.AutoWhere = this.GetHeaderAutoWhere(context.Start.TokenIndex);

      _stack.Push(node);
    }

    public override void ExitFor_update_clause(MiniSqlParserParser.For_update_clauseContext context) {
      var comments = this.GetComments(context);
      WaitType waitType = WaitType.None;
      int waitTime = -1;
      if(context.K_WAIT() != null) {
        waitType = WaitType.Wait;
        if(context.UINTEGER_LITERAL() != null) {
          waitTime = int.Parse(context.UINTEGER_LITERAL().ToString());
        }
      } else if(context.K_NOWAIT() != null) {
        waitType = WaitType.NoWait;
      } else if(context.K_SKIP() != null) {
        waitType = WaitType.SkipLocked;
      }
      ForUpdateOfClause ofClause = null;
      if(context.for_update_of_clause() != null) {
        ofClause = (ForUpdateOfClause)_stack.Pop();
      }
      var node = new ForUpdateClause(ofClause, waitType, waitTime, comments);
      _stack.Push(node);
    }

    public override void ExitFor_update_of_clause(MiniSqlParserParser.For_update_of_clauseContext context) {
      var comments = this.GetComments(context);
      var columns = new List<Column>();
      for(var i = context.column_name().Length - 1; i >= 0; --i) {
        var columnName = (Column)_stack.Pop();
        columns.Insert(0, columnName);
      }
      var node = new ForUpdateOfClause(columns, comments);
      _stack.Push(node);
    }

    public override void ExitInsert_stmt(MiniSqlParserParser.Insert_stmtContext context) {
      var comments = this.GetComments(context);
      var hasIntoKeyword = context.K_INTO() != null;
      var hasColumnNames = context.unqualified_column_names() != null;

      if(this.ForSqlAccessor && !hasIntoKeyword) {
        this.AddSqlAccessorSyntaxError("SqlPodではINSERT文のINTOキーワードを省略できません", context);
      }

      if(this.ForSqlAccessor && !hasColumnNames) {
        this.AddSqlAccessorSyntaxError("SqlPodではINSERT文のテーブル列名の指定を省略できません", context);
      }

      ValuesList valuesList = null;
      IQuery query = null;
      if(context.K_VALUES() != null) {
        valuesList = (ValuesList)_stack.Pop();
      } else if(context.query() != null) {
        query = (IQuery)_stack.Pop();
      } else {
        throw new CannotBuildASTException("Either values clause or select clause are not used");
      }

      UnqualifiedColumnNames columns = null;
      if(hasColumnNames) {
        columns = (UnqualifiedColumnNames)_stack.Pop();
      }

      var tableNode = (Table)_stack.Pop();
      // コメントでテーブル別名の指定があれば取得する
      var implicitAliasName = this.GetTableAliasNameFromDocComment(context.table_name());
      tableNode.ImplicitAliasName = implicitAliasName;

      ConflictType onConflict = ConflictType.None;
      if(context.K_OR() != null) {
        if(context.K_ROLLBACK() != null) {
          onConflict = ConflictType.Rollback;
        } else if(context.K_ABORT() != null) {
          onConflict = ConflictType.Abort;
        } else if(context.K_REPLACE() != null) {
          onConflict = ConflictType.Replace;
        } else if(context.K_FAIL() != null) {
          onConflict = ConflictType.Fail;
        } else if(context.K_IGNORE() != null) {
          onConflict = ConflictType.Ignore;
        } else {
          throw new InvalidEnumArgumentException("Undefined ConflictType is used");
        }
      }

      WithClause withNode = null;
      if(context.with_clause() != null) {
        withNode = (WithClause)_stack.Pop();
      }

      if(context.K_VALUES() != null) {
        var node = new InsertValuesStmt(withNode
                                      , false
                                      , onConflict
                                      , hasIntoKeyword
                                      , tableNode
                                      , columns
                                      , valuesList
                                      , comments);
        // INSERT-VALUES文の先頭コメント、プレースホルダ初期値を設定する
        node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
        node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
        _stack.Push(node);
      } else {
        var node = new InsertSelectStmt(withNode
                                      , false
                                      , onConflict
                                      , hasIntoKeyword
                                      , tableNode
                                      , columns
                                      , query
                                      , comments);
        // INSERT-SELECT文の先頭コメント、プレースホルダ初期値及びAutoWhere値を設定する
        node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
        node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
        node.AutoWhere = this.GetHeaderAutoWhere(context.Start.TokenIndex);
        _stack.Push(node);
      }
    }

    public override void ExitValues_clauses(MiniSqlParserParser.Values_clausesContext context) {
      var comments = this.GetComments(context);

      var valuesList = new List<Values>();
      for(var i = context.value_columns().Length - 1; i >= 0; --i) {
        var values = (Values)_stack.Pop();
        valuesList.Insert(0, values);
      }
      var node = new ValuesList(valuesList, comments);
      _stack.Push(node);
    }

    public override void ExitValue_columns(MiniSqlParserParser.Value_columnsContext context) {
      var comments = this.GetComments(context);

      var values = new List<IValue>();
      for(var i = context.value_column().Length - 1; i >= 0; --i) {
        var value = (IValue)_stack.Pop();
        values.Insert(0, value);
      }
      var node = new Values(values, comments);
      _stack.Push(node);
    }

    public override void ExitReplace_stmt(MiniSqlParserParser.Replace_stmtContext context) {
      var comments = this.GetComments(context);
      var hasIntoKeyword = context.K_INTO() != null;
      var hasColumnNames = context.unqualified_column_names() != null;

      if(this.ForSqlAccessor && !hasIntoKeyword) {
        this.AddSqlAccessorSyntaxError("SqlPodではREPLACE文のINTOキーワードを省略できません", context);
      }

      if(this.ForSqlAccessor && !hasColumnNames) {
        this.AddSqlAccessorSyntaxError("SqlPodではREPLACE文のテーブル列名の指定を省略できません", context);
      }

      ValuesList valuesList = null;
      IQuery query = null;
      if(context.K_VALUES() != null) {
        valuesList = (ValuesList)_stack.Pop();
      } else if(context.query() != null) {
        query = (IQuery)_stack.Pop();
      } else {
        throw new CannotBuildASTException("Either values clause or select clause are not used");
      }

      UnqualifiedColumnNames columns = null;
      if(hasColumnNames) {
        columns = (UnqualifiedColumnNames)_stack.Pop();
      }

      var tableNode = (Table)_stack.Pop();
      // コメントでテーブル別名の指定があれば取得する
      var implicitAliasName = this.GetTableAliasNameFromDocComment(context.table_name());
      tableNode.ImplicitAliasName = implicitAliasName;

      ConflictType onConflict = ConflictType.None;

      WithClause withNode = null;
      if(context.with_clause() != null) {
        withNode = (WithClause)_stack.Pop();
      }

      if(context.K_VALUES() != null) {
        var node = new InsertValuesStmt(withNode
                                      , true
                                      , onConflict
                                      , hasIntoKeyword
                                      , tableNode
                                      , columns
                                      , valuesList
                                      , comments);
        // REPLACE-VALUES文の先頭コメント、プレースホルダ初期値を設定する
        node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
        node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
        _stack.Push(node);
      } else {
        var node = new InsertSelectStmt(withNode
                                      , true
                                      , onConflict
                                      , hasIntoKeyword
                                      , tableNode
                                      , columns
                                      , query
                                      , comments);
        // REPLACE-SELECT文の先頭コメント、プレースホルダ初期値及びAutoWhere値を設定する
        node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
        node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
        node.AutoWhere = this.GetHeaderAutoWhere(context.Start.TokenIndex);
        _stack.Push(node);
      }
    }

    public override void ExitUpdate_stmt(MiniSqlParserParser.Update_stmtContext context) {
      var comments = this.GetComments(context);

      Predicate whereNode = null;
      if(context.K_WHERE() != null) {
        whereNode = (Predicate)_stack.Pop();
      }

      Table tableNode2 = null;
      if(context.K_FROM() != null) {
        tableNode2 = (Table)_stack.Pop();
      }

      var assignments = (Assignments)_stack.Pop();

      var tableNode = (Table)_stack.Pop();

      ConflictType onConflict = ConflictType.None;
      if(context.K_OR() != null) {
        if(context.K_ROLLBACK() != null) {
          onConflict = ConflictType.Rollback;
        }else if(context.K_ABORT() != null){
          onConflict = ConflictType.Abort;
        }else if(context.K_REPLACE() != null){
          onConflict = ConflictType.Replace;
        }else if(context.K_FAIL() != null){
          onConflict = ConflictType.Fail;
        } else if(context.K_IGNORE() != null) {
          onConflict = ConflictType.Ignore;
        } else {
          throw new InvalidEnumArgumentException("Undefined ConflictType is used");
        }
      }

      WithClause withNode = null;
      if(context.with_clause() != null) {
        withNode = (WithClause)_stack.Pop();
      }

      var node = new UpdateStmt(withNode
                              , onConflict
                              , tableNode
                              , assignments
                              , tableNode2
                              , whereNode
                              , comments);
      // UPDATE文の先頭コメント、プレースホルダ初期値及びAutoWhere値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
      node.AutoWhere = this.GetHeaderAutoWhere(context.Start.TokenIndex);

      this._stack.Push(node);
    }

    public override void ExitAssignments(MiniSqlParserParser.AssignmentsContext context) {
      var assignments = new List<Assignment>();
      var comments = new Comments();

      for(var i = context.column_name().Length - 1; i >= 0; --i) {
        if(_stack.Peek().GetType() == typeof(Default)) {
          var valueNode = (Default)_stack.Pop();
          var columnNode = (Column)_stack.Pop();
          assignments.Insert(0, new Assignment(columnNode
                                              , valueNode
                                              , this.GetComments(context.ASSIGN(i))));
        } else {
          var valueNode = (Expr)_stack.Pop();
          var columnNode = (Column)_stack.Pop();
          assignments.Insert(0, new Assignment(columnNode
                                              , valueNode
                                              , this.GetComments(context.ASSIGN(i))));
        }
        if(i != 0) {
          comments.Add(this.GetComments(context.COMMA(i-1))[0]);
        }
      }

      var node = new Assignments(assignments, comments);
      _stack.Push(node);
    }

    public override void ExitDelete_stmt(MiniSqlParserParser.Delete_stmtContext context) {
      var comments = this.GetComments(context);

      Predicate whereNode = null;
      if(context.K_WHERE() != null) {
        whereNode = (Predicate)_stack.Pop();
      }

      Table tableNode2 = null;
      if(context.f2 != null) {
        tableNode2 = (Table)_stack.Pop();
      }

      var tableNode = (Table)_stack.Pop();

      var hasFromKeyword = context.f1 != null;

      if(ForSqlAccessor && !hasFromKeyword) {
        this.AddSqlAccessorSyntaxError("SqlPodではDELETE文のFROMキーワードを省略できません", context);
      }

      WithClause withNode = null;
      if(context.with_clause() != null) {
        withNode = (WithClause)_stack.Pop();
      }

      var node = new DeleteStmt(withNode, hasFromKeyword, tableNode, tableNode2, whereNode, comments);

      // DELETE文の先頭コメント、プレースホルダ初期値及びAutoWhere値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
      node.AutoWhere = this.GetHeaderAutoWhere(context.Start.TokenIndex);

      _stack.Push(node);
    }

    public override void ExitMerge_stmt(MiniSqlParserParser.Merge_stmtContext context) {
      var comments = this.GetComments(context);

      MergeUpdateClause updateClause = null;
      MergeInsertClause insertClause = null;
      bool updateBeforeInsert = false;
      if(context.primary != null) {
        if(context.merge_insert_clause() != null) {
          insertClause = (MergeInsertClause)_stack.Pop();
        }
        updateClause = (MergeUpdateClause)_stack.Pop();
        updateBeforeInsert = true;
      } else if(context.secondary != null){
        if(context.merge_update_clause() != null) {
          updateClause = (MergeUpdateClause)_stack.Pop();
        }
        insertClause = (MergeInsertClause)_stack.Pop();
        updateBeforeInsert = false;
      }

      Predicate constraint = null;
      if(context.p != null) {
        // MS SQL Server
        constraint = (Predicate)_stack.Pop();
      } else {
        // Oracle

        // ON句の括弧はBracketedPredicateで表現する
        // MergeStmtのコメントをBracketedPredicateに移す
        var constraintComments = new Comments();
        constraintComments.Add(comments[4]);
        constraintComments.Add(comments[5]);
        comments.RemoveAt(4);
        comments.RemoveAt(4);
        constraint = new BracketedPredicate((Predicate)_stack.Pop(), constraintComments);
      }

      Table usingTable = null;
      AliasedQuery usingQuery = null;
      if(context.aliased_query() == null) {
        usingTable = (Table)_stack.Pop();
      } else {
        usingQuery = (AliasedQuery)_stack.Pop();
      }

      Table table = (Table)_stack.Pop();

      WithClause withNode = null;
      if(context.with_clause() != null) {
        withNode = (WithClause)_stack.Pop();
      }

      var node = new MergeStmt(withNode
                              , table
                              , usingTable
                              , usingQuery
                              , constraint
                              , updateClause
                              , insertClause
                              , updateBeforeInsert
                              , comments);
      // MERGE文の先頭コメント、プレースホルダ初期値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);

      _stack.Push(node);
    }

    public override void ExitMerge_update_clause(MiniSqlParserParser.Merge_update_clauseContext context) {
      var comments = this.GetComments(context);
      var assignments = (Assignments)_stack.Pop();
      _stack.Push(new MergeUpdateClause(assignments, comments));
    }

    public override void ExitMerge_insert_clause(MiniSqlParserParser.Merge_insert_clauseContext context) {
      var comments = this.GetComments(context);
      var values = (Values)_stack.Pop();
      ColumnNames columns = null;
      if(context.column_names() != null) {
        columns = (ColumnNames)_stack.Pop();
      }
      _stack.Push(new MergeInsertClause(columns, values, comments));
    }
    
    public override void ExitIf_stmt(MiniSqlParserParser.If_stmtContext context) {
      var comments = this.GetComments(context);

      Stmts elseStmts = null;
      if(context.K_ELSE() != null) {
        elseStmts = (Stmts)_stack.Pop();
      }

      var conditions = new List<Predicate>();
      var statementsList = new List<Stmts>();
      for(var i = context.predicate().Length - 1; i >= 0; --i) {
        statementsList.Insert(0, (Stmts)_stack.Pop());
        conditions.Insert(0, (Predicate)_stack.Pop());
      }

      var node = new IfStmt(conditions, statementsList, elseStmts, comments);
      // IF文の先頭コメント、プレースホルダ初期値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);

      // THENの直後とStatement.HeadderCommentが同じ位置のコメントを表している
      // 重複しないようTHENの直後のコメントを削除する
      node.Comments[1] = null;
      int commentIndex;
      for(commentIndex = 3; commentIndex <= node.CountElsIfStatements*2 + 1 ; commentIndex+=2) {
        node.Comments[commentIndex] = null;
      }
      if(node.HasElseStatements) {
        node.Comments[node.CountElsIfStatements*2 + 2] = null;
      }

      _stack.Push(node);
    }

    public override void ExitTruncate_stmt(MiniSqlParserParser.Truncate_stmtContext context) {
      var comments = this.GetComments(context);

      var tableNode = (Table)_stack.Pop();
      // コメントでテーブル別名の指定があれば取得する
      var implicitAliasName = this.GetTableAliasNameFromDocComment(context.table_name());
      tableNode.ImplicitAliasName = implicitAliasName;

      var node = new TruncateStmt(tableNode, comments);
      // TRUNCATE文の先頭コメント、プレースホルダ初期値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);

      _stack.Push(node);
    }

    public override void ExitCall_stmt(MiniSqlParserParser.Call_stmtContext context) {
      var commentsAfterName = this.GetComments(context.function_name().qualified_schema_name());
      commentsAfterName.AddRange(this.GetComments(context.function_name()));
      var comments = this.GetComments(context);
      comments.InsertRange(1, commentsAfterName);
      
      Identifier serverName = null;
      Identifier databaseName = null;
      Identifier schemaName = null;
      if(context.function_name().qualified_schema_name() != null) {
        serverName = this.GetIdentifier(context.function_name().qualified_schema_name().s);
        databaseName = this.GetIdentifier(context.function_name().qualified_schema_name().d);
        schemaName = this.GetIdentifier(context.function_name().qualified_schema_name().n);
      }
      var name = this.GetIdentifier(context.function_name().identifier());
      Exprs arguments = null;
      if(context.exprs() != null) {
        arguments = (Exprs)_stack.Pop();
      }

      var node = new CallStmt(serverName, databaseName, schemaName, name, arguments, comments);
      // CALL文の先頭コメント、プレースホルダ初期値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);

      _stack.Push(node);
    }

    public override void ExitSqlite_pragma_stmt(MiniSqlParserParser.Sqlite_pragma_stmtContext context) {
      var placeHolderNode =
          context.PLACEHOLDER1() != null ? context.PLACEHOLDER1() : context.PLACEHOLDER2();
      var comments = this.GetComments(context);
      if(placeHolderNode != null) {
        var tableName = new PlaceHolderExpr(placeHolderNode.GetText()
                                          , this.GetComments(placeHolderNode));
        comments.RemoveAt(3);
        var node = new SqlitePragmaStmt(tableName, comments);
        // PRAGMA文の先頭コメント、プレースホルダ初期値を設定する
        node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
        node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);

        _stack.Push(node);
      } else {
        var table = (Table)_stack.Pop();
        var node = new SqlitePragmaStmt(table, comments);
        // PRAGMA文の先頭コメント、プレースホルダ初期値を設定する
        node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
        node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);

        _stack.Push(node);
      }
    }

    public override void ExitNull_stmt(MiniSqlParserParser.Null_stmtContext context) {
      var comments = this.GetComments(context);
      var node = new NullStmt(0, comments);
      // NULL文の先頭コメント、プレースホルダ初期値を設定する
      node.HeaderComment = this.GetHeaderComment(context.Start.TokenIndex);
      node.PlaceHolderAssignComments = this.GetDefaultValuePlaceHolders(context.Start.TokenIndex);
      _stack.Push(node);
    }

    public override void ExitWith_clause(MiniSqlParserParser.With_clauseContext context) {
      var comments = this.GetComments(context);
      var hasRecursiveKeyword = context.K_RECURSIVE() != null;

      var withDefinitions = new List<WithDefinition>();
      for(int i = context.with_definition().Length - 1; i >= 0; --i) {
        var withDefinition = (WithDefinition)_stack.Pop();
        withDefinitions.Insert(0, withDefinition);
      }

      var node = new WithClause(hasRecursiveKeyword, withDefinitions, comments);
      _stack.Push(node);
    }

    public override void ExitWith_definition(MiniSqlParserParser.With_definitionContext context) {
      var comments = this.GetComments(context);
      var query = (IQuery)_stack.Pop();
      UnqualifiedColumnNames columns = null;
      if(context.unqualified_column_names() != null) {
        columns = (UnqualifiedColumnNames)_stack.Pop();
      }
      var table = (Table)_stack.Pop();
      var node = new WithDefinition(table, columns, query, comments);
      _stack.Push(node);
    }

    public override void ExitQuery(MiniSqlParserParser.QueryContext context) {
      ILimitClause limitClause = null;
      if(context.limit_clause() != null) {
        limitClause = (ILimitClause)_stack.Pop();
      }

      OrderBy orderBy;
      if(context.orderBy_clause() != null) {
        orderBy = (OrderBy)_stack.Pop();
      } else {
        orderBy = new OrderBy();
      }

      var queryClause = (IQueryClause)_stack.Pop();
      if(queryClause.Type == QueryType.Single) {
        var singleQueryClause = (SingleQueryClause)queryClause;
        var node = new SingleQuery(singleQueryClause.Quantifier
                                  , singleQueryClause.HasTop
                                  , singleQueryClause.Top
                                  , singleQueryClause.HasWildcard
                                  , singleQueryClause.Results
                                  , singleQueryClause.From
                                  , singleQueryClause.Where
                                  , singleQueryClause.GroupBy
                                  , singleQueryClause.Having
                                  , orderBy
                                  , limitClause
                                  , singleQueryClause.Comments);
        node.Comments.AddRange(this.GetComments(context));
        _stack.Push(node);
      } else if(queryClause.Type == QueryType.Compound) {
        var compoundQueryClause = (CompoundQueryClause)queryClause;
        var node = new CompoundQuery(compoundQueryClause.Left
                                    , compoundQueryClause.Operator
                                    , compoundQueryClause.Right
                                    , orderBy
                                    , limitClause
                                    , compoundQueryClause.Comments);
        node.Comments.AddRange(this.GetComments(context));
        _stack.Push(node);
      } else if(queryClause.Type == QueryType.Bracketed) {
        var bracketedQueryClause = (BracketedQueryClause)queryClause;
        var node = new BracketedQuery(bracketedQueryClause.Operand
                                    , orderBy
                                    , limitClause
                                    , bracketedQueryClause.Comments);
        node.Comments.AddRange(this.GetComments(context));
        _stack.Push(node);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used"
                                              , (int)queryClause.Type
                                              , typeof(QueryType));
      }
    }

    public override void ExitSingleQueryClause(MiniSqlParserParser.SingleQueryClauseContext context) {
      var comments = this.GetComments(context);

      QuantifierType quantifier;
      if(context.K_DISTINCT() != null) {
        quantifier = QuantifierType.Distinct;
      } else if(context.K_ALL() != null) {
        quantifier = QuantifierType.All;
      } else {
        quantifier = QuantifierType.None;
      }

      var hasTop = context.K_TOP() != null;
      int top = 0;
      if(hasTop) {
        top = int.Parse(context.UINTEGER_LITERAL().GetText());
      }
      var hasWildcard = context.STAR() != null;

      Predicate having = null;
      if(context.K_HAVING() != null) {
        having = (Predicate)_stack.Pop();
      }

      GroupBy groupBy;
      if(context.groupBy_clause() != null) {
        groupBy = (GroupBy)_stack.Pop();
      } else {
        groupBy = new GroupBy();
      }

      Predicate where = null;
      if(context.K_WHERE() != null) {
        where = (Predicate)_stack.Pop();
      }

      IFromSource from = null;
      if(context.K_FROM() != null) {
        from = (IFromSource)_stack.Pop();
      } else if(hasWildcard) {
        throw new CannotBuildASTException("FROM句が無いためWildcard(*)は使えません");
      }

      ResultColumns results;
      if(!hasWildcard) {
        results = (ResultColumns)_stack.Pop();
      }else{
        results = new ResultColumns();
      }

      INode
      node = new SingleQueryClause(quantifier
                                  , hasTop
                                  , top
                                  , hasWildcard
                                  , results
                                  , from
                                  , where
                                  , groupBy
                                  , having
                                  , comments);
      this._stack.Push(node);
    }

    public override void ExitCompoundQueryClause(MiniSqlParserParser.CompoundQueryClauseContext context) {
      var comments = this.GetComments(context);

      CompoundType operater;
      if(context.K_UNION() != null) {
        if(context.K_ALL() != null) {
          operater = CompoundType.UnionAll;
        } else {
          operater = CompoundType.Union;
        }
      } else if(context.K_INTERSECT() != null) {
        operater = CompoundType.Intersect;
      } else if(context.K_EXCEPT() != null) {
        operater = CompoundType.Except;
      } else if(context.K_MINUS() != null){
        operater = CompoundType.Minus;
      } else {
        throw new InvalidEnumArgumentException("Undefined compound operator type is used");
      }

      var right = (IQueryClause)_stack.Pop();
      var left = (IQueryClause)_stack.Pop();

      INode
      node = new CompoundQueryClause(left
                                    , operater
                                    , right
                                    , comments);
      _stack.Push(node);
    }

    public override void ExitBracketedQueryClause(MiniSqlParserParser.BracketedQueryClauseContext context) {
      var comments = this.GetComments(context);
      var operand = (IQueryClause)_stack.Pop();
      INode node = new BracketedQueryClause(operand, comments);
      _stack.Push(node);
    }

    public override void ExitAliased_query(MiniSqlParserParser.Aliased_queryContext context) {
      var comments = this.GetComments(context);
      var hasAs = context.K_AS() != null;
      var aliasName = this.GetIdentifier(context.table_alias());
      var subQuery = (IQuery)_stack.Pop();

      if(this.ForSqlAccessor) {
        if(string.IsNullOrEmpty(aliasName)) {
          this.AddSqlAccessorSyntaxError("SqlPodではFrom句サブクエリに別名の指定が必要です", context);
        } else if(hasAs) {
          this.AddSqlAccessorSyntaxError("SqlPodではFrom句サブクエリの別名指定にAS句は不要です", context);
        }
      }

      this._stack.Push(new AliasedQuery(subQuery, hasAs, aliasName, comments));
    }

    public override void ExitResult_columns(MiniSqlParserParser.Result_columnsContext context) {
      var comments = this.GetComments(context);
      var resultColumns = new List<ResultColumn>();
      for(var i = context.result_column().Length - 1; i >= 0; --i) {
        var recultColumn = (ResultColumn)_stack.Pop();
        resultColumns.Insert(0, recultColumn);
      }
      var node = new ResultColumns(resultColumns, comments);
      _stack.Push(node);
    }
    
    public override void ExitResult_column(MiniSqlParserParser.Result_columnContext context) {
      var comments = this.GetComments(context);
      var contextIsTablewildcard = context.STAR() != null;
      if(contextIsTablewildcard){
        var tableNode = (Table)_stack.Pop();
        var tableNodeComments = tableNode.Comments;
        tableNodeComments.AddRange(comments);
        _stack.Push(new TableWildcard(tableNode.ServerName
                                    , tableNode.DataBaseName
                                    , tableNode.SchemaName
                                    , tableNode.Name
                                    , tableNodeComments));
      } else {
        var aliasName = this.GetIdentifier(context.column_alias());
        var hasAs = context.K_AS() != null;
        var expr = (Expr)_stack.Pop();
        _stack.Push(new ResultExpr(expr, hasAs, aliasName, comments));
      }
    }

    public override void ExitValue_column(MiniSqlParserParser.Value_columnContext context) {
      if(context.K_DEFAULT() != null) {
        var comments = this.GetComments(context.K_DEFAULT());
        _stack.Push(new Default(comments));
      }
    }

    public override void ExitColumn_names(MiniSqlParserParser.Column_namesContext context) {
      var comments = this.GetComments(context);
      var columns = new List<Column>();
      for(var i = context.column_name().Length - 1; i >= 0; --i) {
        var columnName = (Column)_stack.Pop();
        columns.Insert(0, columnName);
      }
      var node = new ColumnNames(columns, comments);
      _stack.Push(node);
    }

    public override void ExitUnqualified_column_names(MiniSqlParserParser.Unqualified_column_namesContext context) {
      var comments = this.GetComments(context);
      var columns = new List<UnqualifiedColumnName>();
      for(var i = context.unqualified_column_name().Length - 1; i >= 0; --i) {
        var columnName = (UnqualifiedColumnName)_stack.Pop();
        columns.Insert(0, columnName);
        // columns.Insert(0, context.IDENTIFIER(i).GetText());
      }
      var node = new UnqualifiedColumnNames(columns, comments);
      _stack.Push(node);
    }

    public override void ExitUnqualified_column_name(MiniSqlParserParser.Unqualified_column_nameContext context) {
      var comments = this.GetComments(context);
      var node = new UnqualifiedColumnName(this.GetIdentifier(context.identifier()), comments);
      _stack.Push(node);
    }

    public override void ExitExprs(MiniSqlParserParser.ExprsContext context) {
      var comments = this.GetComments(context);
      var exprs = new List<Expr>();
      for(var i = context.expr().Length - 1; i >= 0; --i) {
        var expr = (Expr)_stack.Pop();
        exprs.Insert(0, expr);
      }
      var node = new Exprs(exprs, comments);
      _stack.Push(node);
    }

    //public override void ExitSubQuerySource(MiniSqlParserParser.SubQuerySourceContext context) {

    //}

    public override void ExitJoinSource(MiniSqlParserParser.JoinSourceContext context) {
      var comments = this.GetComments(context.join_constraint());
      var constraintContext = context.join_constraint();

      Predicate constraint = null;
      UnqualifiedColumnNames usingConstraint = null;
      if(constraintContext != null) {
        if(constraintContext.K_ON() != null) {
          constraint = (Predicate)_stack.Pop();
        } else if(constraintContext.K_USING() != null) {
          if(this.ForSqlAccessor) {
            this.AddSqlAccessorSyntaxError("SqlPodではUSING句は使えません", context);
          }
          usingConstraint = (UnqualifiedColumnNames)_stack.Pop();
        }
      }
      var right = (IFromSource)_stack.Pop();
      var op = (JoinOperator)_stack.Pop();
      var left = (IFromSource)_stack.Pop();
      
      _stack.Push(new JoinSource(left, op, right, constraint, usingConstraint, comments));
    }

    public override void ExitCommaJoinSource(MiniSqlParserParser.CommaJoinSourceContext context) {
      var comments = this.GetComments(context.COMMA());

      if(this.ForSqlAccessor) {
        this.AddSqlAccessorSyntaxError("SqlPodではFROM句でカンマ(,)による結合はできません", context);
      }

      var right = (IFromSource)_stack.Pop();
      var left = (IFromSource)_stack.Pop();

      _stack.Push(new CommaJoinSource(left, right, comments));
    }

    public override void ExitBracketedSource(MiniSqlParserParser.BracketedSourceContext context) {
      var comments = this.GetComments(context);
      var hasAs = context.K_AS() != null;
      var aliasName = this.GetIdentifier(context.table_alias());
      var operand = (IFromSource)_stack.Pop();
      this._stack.Push(new BracketedSource(operand, hasAs, aliasName, comments));
    }

    public override void ExitJoin_operator(MiniSqlParserParser.Join_operatorContext context) {
      var comments = this.GetComments(context);
      var hasNatural = context.K_NATURAL() != null;
      var hasOuter = context.K_OUTER() != null;

      if(this.ForSqlAccessor && hasNatural) {
        this.AddSqlAccessorSyntaxError("SqlPodではNATURALキーワードを使えません", context);
      }

      JoinType joinType = JoinType.None;
      if(context.K_LEFT() != null) {
        joinType = JoinType.Left;
      } else if(context.K_RIGHT() != null) {
        joinType = JoinType.Right;
      } else if(context.K_FULL()  != null) {
        joinType = JoinType.Full;
      } else if(context.K_INNER() != null) {
        joinType = JoinType.Inner;
      } else if(context.K_CROSS() != null) {
        joinType = JoinType.Cross;
      }

      _stack.Push(new JoinOperator(joinType, hasNatural, hasOuter, comments));
    }

    public override void ExitGroupBy_clause(MiniSqlParserParser.GroupBy_clauseContext context) {
      var comments = this.GetComments(context);
      var terms = (Exprs)_stack.Pop();
      comments.AddRange(terms.Comments);
      _stack.Push(new GroupBy(terms, comments));
    }

    public override void ExitOrderBy_clause(MiniSqlParserParser.OrderBy_clauseContext context) {
      var comments = this.GetComments(context);
      var terms = new List<OrderingTerm>();
      for(var i = context.ordering_term().Length - 1; i >= 0; --i) {
        var term = (OrderingTerm)_stack.Pop();
        terms.Insert(0, term);
      }
      _stack.Push(new OrderBy(terms, comments));
    }

    public override void ExitOrdering_term(MiniSqlParserParser.Ordering_termContext context) {
      var comments = this.GetComments(context);

      Identifier collation = null;
      if(context.K_COLLATE() != null) {
        collation = this.GetIdentifier(context.collation_name());
      }

      OrderSpec orderSpec = OrderSpec.None;
      if(context.K_DESC() != null) {
        orderSpec = OrderSpec.Desc;
      } else if(context.K_ASC() != null) {
        orderSpec = OrderSpec.Asc;
      }

      NullOrder nullOrder = NullOrder.None;
      if(context.K_NULLS() != null) {
        if(context.K_FIRST() != null) {
          nullOrder = NullOrder.First;
        } else if(context.K_LAST() != null) {
          nullOrder = NullOrder.Last;
        }
      }

      var term = (Expr)_stack.Pop();

      _stack.Push(new OrderingTerm(term, collation, orderSpec, nullOrder, comments));
    }

    public override void ExitPartitionBy_clause(MiniSqlParserParser.PartitionBy_clauseContext context) {
      var comments = this.GetComments(context);
      var terms = new List<PartitioningTerm>();
      for(var i = context.partitioning_term().Length - 1; i >= 0; --i) {
        var term = (PartitioningTerm)_stack.Pop();
        terms.Insert(0, term);
      }
      _stack.Push(new PartitionBy(terms, comments));
    }

    public override void ExitPartitioning_term(MiniSqlParserParser.Partitioning_termContext context) {
      var comments = this.GetComments(context);

      Identifier collation = null;
      if(context.K_COLLATE() != null) {
        collation = this.GetIdentifier(context.collation_name());
      }

      var term = (Expr)_stack.Pop();

      _stack.Push(new PartitioningTerm(term, collation, comments));
    }

    public override void ExitLimit_clause(MiniSqlParserParser.Limit_clauseContext context) {
      var comments = this.GetComments(context);

      if(context.K_LIMIT() != null) {
        var offsetSeparatorIsComma = context.COMMA() != null;

        // カンマで区切られた場合はcountとoffsetが逆になる
        // LIMIT [count]
        // LIMIT [count] OFFSET [offset]
        // LIMIT [offset] , [count]
        Expr limitExpr = null;
        Expr offsetExpr = null;
        if(offsetSeparatorIsComma) {
          limitExpr = (Expr)_stack.Pop();
          offsetExpr = (Expr)_stack.Pop();
        } else {
          if(context.K_OFFSET() != null) {
            // OFFSET句がある場合
            offsetExpr = (Expr)_stack.Pop();
          }
          limitExpr = (Expr)_stack.Pop();
        }

        _stack.Push(new LimitClause(offsetExpr, limitExpr, offsetSeparatorIsComma, comments));

      } else {
        var hasOffset = context.K_OFFSET() != null;
        int offset = 0;
        var hasFetch = context.K_FETCH() != null;
        int fetch = 0;
        var fetchFromFirst = context.K_FIRST() != null;
        var fetchWithTies = context.K_TIES() != null;
        var fetchRowCountType = FetchRowCountType.None;

        if(hasOffset) {
          offset = int.Parse(context.uint0.Text);
        }

        if(hasFetch){
          fetch = int.Parse(context.uint1.Text); 
          if(context.K_PERCENT() != null){
            fetchRowCountType = FetchRowCountType.Percentile;
          }else{
            fetchRowCountType = FetchRowCountType.Integer;
          }
        }

        _stack.Push(new OffsetFetchClause(hasOffset
                                        , offset
                                        , hasFetch
                                        , fetch
                                        , fetchFromFirst
                                        , fetchRowCountType
                                        , fetchWithTies
                                        , comments));
      }
    }

    public override void ExitTable_name(MiniSqlParserParser.Table_nameContext context) {
      Identifier serverName = null;
      Identifier databaseName = null;
      Identifier schemaName = null;
      if(context.qualified_schema_name() != null) {
        serverName = this.GetIdentifier(context.qualified_schema_name().s);
        databaseName = this.GetIdentifier(context.qualified_schema_name().d);
        schemaName = this.GetIdentifier(context.qualified_schema_name().n);
      }
      var tableName = this.GetIdentifier(context.identifier());
      var comments = this.GetComments(context.qualified_schema_name());
      comments.AddRange(this.GetComments(context));
      _stack.Push(new Table(serverName, databaseName, schemaName, tableName, comments));
    }

    public override void ExitAliased_table_name(MiniSqlParserParser.Aliased_table_nameContext context) {
      var tableNode = (Table)_stack.Pop();
      var comments = tableNode.Comments;
      comments.AddRange(this.GetComments(context));

      var hasAs = context.K_AS() != null;
      var aliasName = this.GetIdentifier(context.table_alias());
      // コメントでテーブル別名の指定があれば取得する
      var implicitAliasName = this.GetTableAliasNameFromDocComment(context);

      _stack.Push(new Table(tableNode.ServerName
                          , tableNode.DataBaseName
                          , tableNode.SchemaName
                          , tableNode.Name
                          , hasAs
                          , aliasName
                          , implicitAliasName
                          , comments));
    }

    public override void ExitHinted_table_name(MiniSqlParserParser.Hinted_table_nameContext context) {
      if(context.table_hint() == null) {
        // table_hintが存在しない場合はなにもしない
        return;
      }

      var tableNode = (Table)_stack.Pop();
      var comments = tableNode.Comments;
      comments.AddRange(this.GetComments(context));
      comments.AddRange(this.GetComments(context.table_hint()));

      // コメントでテーブル別名の指定があれば取得する
      var implicitAliasName = this.GetTableAliasNameFromDocComment(context);

      var hinted_table = this.CreateHintedTable(context.table_hint()
                                              , tableNode
                                              , implicitAliasName
                                              , comments);
      _stack.Push(hinted_table);
    }

    public override void ExitHinted_aliased_table_name(MiniSqlParserParser.Hinted_aliased_table_nameContext context) {
      if(context.table_hint() == null) {
        // table_hintが存在しない場合はなにもしない
        return;
      }
 
      var tableNode = (Table)_stack.Pop();
      var comments = tableNode.Comments;
      comments.AddRange(this.GetComments(context));
      comments.AddRange(this.GetComments(context.table_hint()));

      var hinted_table = this.CreateHintedTable(context.table_hint()
                                              , tableNode
                                              , tableNode.ImplicitAliasName
                                              , comments);
      _stack.Push(hinted_table);
    }

    private Table CreateHintedTable(MiniSqlParserParser.Table_hintContext table_hintContext
                                  , Table tableNode
                                  , string implicitAliasName
                                  , Comments comments) {
      Identifier indexServerName = null;
      Identifier indexDatabaseName = null;
      Identifier indexSchemaName = null;
      Identifier indexName = null;
      bool hasNotIndexed = false;

      if(table_hintContext.K_NOT() != null) {
        hasNotIndexed = true;
      } else if(table_hintContext.K_INDEXED() != null) {
        comments.AddRange(this.GetComments(table_hintContext.index_name().qualified_schema_name()));
        comments.AddRange(this.GetComments(table_hintContext.index_name()));
        if(table_hintContext.index_name().qualified_schema_name() != null) {
          indexServerName = this.GetIdentifier(table_hintContext.index_name().qualified_schema_name().s);
          indexDatabaseName = this.GetIdentifier(table_hintContext.index_name().qualified_schema_name().d);
          indexSchemaName = this.GetIdentifier(table_hintContext.index_name().qualified_schema_name().n);
        }
        indexName = this.GetIdentifier(table_hintContext.index_name().identifier());
      }
      var msSqlHint = this.ConvToMsSqlHint(table_hintContext.h);

      return new Table(tableNode.ServerName
                     , tableNode.DataBaseName
                     , tableNode.SchemaName
                     , tableNode.Name
                     , tableNode.HasAs
                     , tableNode.AliasName
                     , implicitAliasName
                     , indexServerName
                     , indexDatabaseName
                     , indexSchemaName
                     , indexName
                     , hasNotIndexed
                     , msSqlHint
                     , comments);
    }

    private MsSqlHint ConvToMsSqlHint(IToken msSqlHint) {
      if(msSqlHint == null) {
        return MsSqlHint.None;
      }
      var hintType = msSqlHint.Type;
      MsSqlHint hint = MsSqlHint.None;
      if(hintType == MiniSqlParserLexer.K_NOLOCK) {
        hint = MsSqlHint.NoLock;
      } else if(hintType == MiniSqlParserLexer.K_READCOMMITTED) {
        hint = MsSqlHint.ReadCommitted;
      } else if(hintType == MiniSqlParserLexer.K_REPEATABLEREAD) {
        hint = MsSqlHint.RepeatableRead;
      } else if(hintType == MiniSqlParserLexer.K_SERIALIZABLE) {
        hint = MsSqlHint.Serializable;
      } else {
        throw new CannotBuildASTException("Undifined Ms SQL Hint is used");
      }
      return hint;
    }
  }

}
