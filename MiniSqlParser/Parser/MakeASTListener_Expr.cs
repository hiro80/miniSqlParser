using System.Collections.Generic;
using System.ComponentModel;
using Antlr4.Runtime.Tree;

namespace MiniSqlParser
{
  internal partial class MakeASTListener: MiniSqlParserBaseListener
  {

    public override void ExitColumn_name(MiniSqlParserParser.Column_nameContext context) {
      Identifier serverName;
      Identifier databaseName;
      Identifier schemaName;
      Identifier tableName;
      Comments comments;

      if(context.table_name() != null) {
        var tableNode = (Table)_stack.Pop();
        serverName = tableNode.ServerName;
        databaseName = tableNode.DataBaseName;
        schemaName = tableNode.SchemaName;
        tableName = tableNode.Name;
        comments = tableNode.Comments;
      } else {
        serverName = null;
        databaseName = null;
        schemaName = null;
        tableName = null;
        comments = new Comments();
      }

      var columnName = this.GetIdentifier(context.identifier());
      comments.AddRange(this.GetComments(context));
      var node = new Column(serverName, databaseName, schemaName, tableName, columnName, false, comments);
      _stack.Push(node);
    }

    public override void ExitColumnExpr(MiniSqlParserParser.ColumnExprContext context) {
      if(context.OUTER_JOIN() == null) {
        return;
      }
      if(this.ForSqlAccessor) {
        this.AddSqlAccessorSyntaxError("SqlPodでは外部結合演算子(+)は使えません", context);
      }
      var comments = this.GetComments(context.OUTER_JOIN());
      var column = (Column)_stack.Peek();
      column.HasOuterJoinKeyword = true;
      column.Comments.AddRange(comments);
    }

    public override void ExitLiteral_value(MiniSqlParserParser.Literal_valueContext context) {
      ITerminalNode node;
      if((node = context.STRING_LITERAL()) != null) {
        _stack.Push(new StringLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.UINTEGER_LITERAL()) != null) {
        _stack.Push(new UNumericLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.K_NULL()) != null) {
        _stack.Push(new NullLiteral(this.GetComments(node)));
      } else if((node = context.DATE_LITERAL()) != null) {
        _stack.Push(new DateLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.TIME_LITERAL()) != null) {
        _stack.Push(new TimeLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.TIMESTAMP_LITERAL()) != null) {
        _stack.Push(new TimeStampLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.INTERVAL_LITERAL()) != null) {
        _stack.Push(new IntervalLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.UNUMERIC_LITERAL()) != null) {
        _stack.Push(new UNumericLiteral(node.Symbol.Text, this.GetComments(node)));
      } else if((node = context.BLOB_LITERAL()) != null) {
        _stack.Push(new BlobLiteral(node.Symbol.Text, this.GetComments(node)));
      } else {
        throw new CannotBuildASTException("Undefined literal node is used");
      }
    }

    public override void ExitAggregate_function1(MiniSqlParserParser.Aggregate_function1Context context) {
      var comments = this.GetComments(context);
      var name = this.Coalesce(context.K_COUNT()
                              , context.K_SUM()
                              , context.K_AVG()
                              , context.K_TOTAL()
                              , context.K_COUNT_BIG());

      var quantifier = QuantifierType.None;
      if(context.K_DISTINCT() != null) {
        quantifier = QuantifierType.Distinct;
      } else if(context.K_ALL() != null) {
        quantifier = QuantifierType.All;
      }

      var hasWildCard = context.STAR() != null;
      if(hasWildCard) {
        _stack.Push(new AggregateFuncExpr(name, quantifier, true, null, null, comments));
      } else {
        var argument1 = (Expr)_stack.Pop();
        _stack.Push(new AggregateFuncExpr(name, quantifier, false, argument1, null, comments));
      }
    }

    public override void ExitAggregate_function2(MiniSqlParserParser.Aggregate_function2Context context) {
      var comments = this.GetComments(context);
      var name = this.Coalesce(context.K_MAX(), context.K_MIN()
                              , context.K_CORR()
                              , context.K_GROUP_CONCAT()
                              , context.K_STDDEV_POP(), context.K_VAR_POP()
                              , context.K_STDEVP(), context.K_VAR(), context.K_VARP()
                              , context.K_STDEV()
                              , context.K_VARIANCE()
                              , context.K_STDDEV()
                              , context.K_MEDIAN()
                              );
      Expr argument1 = null;
      Expr argument2 = null;
      var countArgs = context.expr().Length;
      if(countArgs == 1) {
        argument1 = (Expr)_stack.Pop();
      } else if(countArgs == 2) {
        argument1 = (Expr)_stack.Pop();
        argument2 = (Expr)_stack.Pop();
      } else {
        throw new CannotBuildASTException("This aggregate function has more than 2 parameters");
      }

      _stack.Push(new AggregateFuncExpr(name, QuantifierType.None, false, argument1, argument2, comments));
    }

    public override void ExitWindow_function(MiniSqlParserParser.Window_functionContext context) {
      var comments = this.GetComments(context.function_name().qualified_schema_name());
      comments.AddRange(this.GetComments(context.function_name()));
      comments.AddRange(this.GetComments(context));
      Identifier serverName = null;
      Identifier databaseName = null;
      Identifier schemaName = null;
      if(context.function_name().qualified_schema_name() != null) {
        serverName = this.GetIdentifier(context.function_name().qualified_schema_name().s);
        databaseName = this.GetIdentifier(context.function_name().qualified_schema_name().d);
        schemaName = this.GetIdentifier(context.function_name().qualified_schema_name().n);
      }
      var name = this.GetIdentifier(context.function_name().identifier());

      QuantifierType quantifier;
      if(context.K_DISTINCT() != null) {
        quantifier = QuantifierType.Distinct;
      } else if(context.K_ALL() != null) {
        quantifier = QuantifierType.All;
      } else {
        quantifier = QuantifierType.None;
      }

      var hasWildcard = context.STAR() != null;
      var orderBy = (OrderBy)_stack.Pop();
      PartitionBy partitionBy = null;
      if(context.partitionBy_clause() != null) {
        partitionBy = (PartitionBy)_stack.Pop();
      }

      Exprs arguments = null;
      if(context.exprs() != null) {
        arguments = (Exprs)_stack.Pop();
      }

      _stack.Push(new WindowFuncExpr(serverName
                                    , databaseName
                                    , schemaName
                                    , name
                                    , quantifier
                                    , hasWildcard
                                    , arguments
                                    , partitionBy
                                    , orderBy
                                    , comments));
    }

    public override void ExitGeneric_function(MiniSqlParserParser.Generic_functionContext context) {
      var comments = this.GetComments(context.function_name().qualified_schema_name());
      comments.AddRange(this.GetComments(context.function_name()));
      comments.AddRange(this.GetComments(context));
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
      _stack.Push(new FuncExpr(serverName, databaseName, schemaName, name, arguments, comments));
    }

    public override void ExitBinaryOpExpr(MiniSqlParserParser.BinaryOpExprContext context) {
      var right = (Expr)_stack.Pop();
      var comments = this.GetComments(context.op);
      var left = (Expr)_stack.Pop();

      var opType = context.op.Type;
      ExpOperator op = ExpOperator.StringConcat;
      if(opType == MiniSqlParserLexer.PIPE2) {
        op = ExpOperator.StringConcat;
      }else if(opType == MiniSqlParserLexer.STAR){
        op = ExpOperator.Mult;
      }else if(opType == MiniSqlParserLexer.DIV){
        op = ExpOperator.Div;
      }else if(opType == MiniSqlParserLexer.MOD){
        op = ExpOperator.Mod;
      }else if(opType == MiniSqlParserLexer.PLUS){
        op = ExpOperator.Add;
      }else if(opType == MiniSqlParserLexer.MINUS){
        op = ExpOperator.Sub;
      }else if(opType == MiniSqlParserLexer.LT2){
        op = ExpOperator.LeftBitShift;
      } else if(opType == MiniSqlParserLexer.GT2) {
        op = ExpOperator.RightBitShift;
      } else if(opType == MiniSqlParserLexer.AMP) {
        op = ExpOperator.BitAnd;
      } else if(opType == MiniSqlParserLexer.PIPE) {
        op = ExpOperator.BitOr;
      } else {
        throw new InvalidEnumArgumentException("Undefined ExpOperator is used"
                                              , (int)opType
                                              , typeof(MiniSqlParserLexer));
      }
      
      var node = new BinaryOpExpr(left, op, right, comments);
      _stack.Push(node);
    }

    public override void ExitCase1Expr(MiniSqlParserParser.Case1ExprContext context) {
      Expr elseResult = null;
      var results = new List<Expr>();
      var comparisons = new List<Expr>();
      var branchCount = context.K_WHEN().Length;
      var comments = this.GetComments(context);
      if(context.K_ELSE() != null) {
        elseResult = (Expr)_stack.Pop();
      }
      for(int i = 0; i < branchCount; ++i) {
        results.Insert(0, (Expr)_stack.Pop());
        comparisons.Insert(0, (Expr)_stack.Pop());
      }
      var searchExpr = (Expr)_stack.Pop();
      var node = new CaseExpr(searchExpr, comparisons, results, elseResult, comments);
      _stack.Push(node);
    }

    public override void ExitCase2Expr(MiniSqlParserParser.Case2ExprContext context) {
      Expr elseResult = null;
      var results = new List<Expr>();
      var conditions = new List<Predicate>();
      var branchCount = context.K_WHEN().Length;
      var comments = this.GetComments(context);
      if(context.K_ELSE() != null) {
        elseResult = (Expr)_stack.Pop();
      }
      for(int i = 0; i < branchCount; ++i) {
        results.Insert(0, (Expr)_stack.Pop());
        conditions.Insert(0, (Predicate)_stack.Pop());
      }
      var node = new CaseExpr(conditions, results, elseResult, comments);
      _stack.Push(node);
    }

    public override void ExitPhExpr(MiniSqlParserParser.PhExprContext context) {
      var placeHolderNode = 
          context.PLACEHOLDER1() != null ? context.PLACEHOLDER1() : context.PLACEHOLDER2();
      var comments = this.GetComments(placeHolderNode);
      var name = placeHolderNode.GetText();
      if(this.ForSqlAccessor) {
        if(name == "?") {
          this.AddSqlAccessorSyntaxError("SqlPodではプレースホルダに'?'を使えません", context);
        } else if(name.StartsWith(":")) {
          this.AddSqlAccessorSyntaxError("SqlPodではプレースホルダに':'を使えません", context);
        }
      }
      var node = new PlaceHolderExpr(name, comments);
      _stack.Push(node);
    }

    public override void ExitSignedNumberExpr(MiniSqlParserParser.SignedNumberExprContext context) {
      Sign sign = Sign.Plus;
      if(context.op.Type == MiniSqlParserLexer.MINUS) {
        sign = Sign.Minus;
      } else {
        sign = Sign.Plus;
      }

      INode node = null;
      if(context.UINTEGER_LITERAL() != null) {
        var comments = this.GetComments(context.UINTEGER_LITERAL());
        var value = new UNumericLiteral(context.UINTEGER_LITERAL().GetText(), comments);
        node = new SignedNumberExpr(sign, value, this.GetComments(context.op));
      } else {
        var comments = this.GetComments(context.UNUMERIC_LITERAL());
        var value = new UNumericLiteral(context.UNUMERIC_LITERAL().GetText(), comments);
        node = new SignedNumberExpr(sign, value, this.GetComments(context.op));
      }

      _stack.Push(node);
    }

    public override void ExitSubQueryExpr(MiniSqlParserParser.SubQueryExprContext context) {
      var comments = this.GetComments(context);
      var query = (IQuery)_stack.Pop();
      var node = new SubQueryExp(query, comments);
      _stack.Push(node);
    }

    public override void ExitBitwiseNotExpr(MiniSqlParserParser.BitwiseNotExprContext context) {
      var comments = this.GetComments(context);
      var operand = (Expr)_stack.Pop();
      var node = new BitwiseNotExpr(operand, comments);
      _stack.Push(node);
    }

    public override void ExitSubstring_function(MiniSqlParserParser.Substring_functionContext context) {
      var comments = this.GetComments(context);

      Expr arg3 = null;
      bool isComma2 = false;
      if(context.expr().Length == 3) {
        arg3 = (Expr)_stack.Pop();
        isComma2 = context.K_FOR() == null;
      }
      Expr arg2 = (Expr)_stack.Pop();
      bool isComma1 = context.K_FROM() == null;
      Expr arg1 = (Expr)_stack.Pop();
      var name = this.Coalesce(context.K_SUBSTRING(), context.K_SUBSTR());

      var node = new SubstringFunc(name, arg1, arg2, arg3, isComma1, isComma2, comments);
      _stack.Push(node);
    }

    public override void ExitExtract_function(MiniSqlParserParser.Extract_functionContext context) {
      var comments = this.GetComments(context);
      var datetimeComments = this.GetComments(context.datetimeField());
      comments.InsertRange(2, datetimeComments);

      Expr arg = null;
      bool isComma = false;
      if(context.expr() != null) {
        arg = (Expr)_stack.Pop();
        isComma = context.K_FROM() == null;
      }
      DateTimeField dt = DateTimeField.Day;
      var datetimeContext = context.datetimeField();
      if(datetimeContext.K_DAY() != null) {
        dt = DateTimeField.Day;
      } else if(datetimeContext.K_YEAR() != null) {
        dt = DateTimeField.Year;
       }else if(datetimeContext.K_MONTH() != null) {
        dt = DateTimeField.Month;
      } else if(datetimeContext.K_HOUR() != null) {
        dt = DateTimeField.Hour;
      } else if(datetimeContext.K_MINUTE() != null) {
        dt = DateTimeField.Minute;
      } else if(datetimeContext.K_SECOND() != null) {
        dt = DateTimeField.Second;
      } else {
        throw new CannotBuildASTException("Undefined DateTimeField is used in EXTRACT function");
      }
      var name = context.K_EXTRACT().GetText();

      var node = new ExtractFuncExpr(name, dt, isComma, arg, comments);
      _stack.Push(node);
    }

    public override void ExitBracketedExpr(MiniSqlParserParser.BracketedExprContext context) {
      var comments = this.GetComments(context);
      var operand = (Expr)_stack.Pop();
      _stack.Push(new BracketedExpr(operand, comments));
    }

    public override void ExitCastExpr(MiniSqlParserParser.CastExprContext context) {
      var comments = this.GetComments(context);
      var typeNameComment = this.GetComments(context.type_name()).Last;
      comments.Insert(3, typeNameComment);
      var typeName = context.type_name().GetText();
      var operand = (Expr)_stack.Pop();
      var name = context.K_CAST().GetText();
      var node = new CastExpr(operand, typeName, comments);
      _stack.Push(node);
    }

  }
}