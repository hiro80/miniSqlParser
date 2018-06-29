using System;
using System.Collections.Generic;

namespace MiniSqlParser
{
  /// <summary>
  /// SELECT句を追加する、
  /// UNION文であればUNIONの被演算子のSELECT文にSELECT句を追加する
  /// </summary>
  public class InsertResultVisitor : Visitor
  {
    private readonly int _index;
    private readonly ResultColumn _result;
    private bool _added;

    public InsertResultVisitor(int index, ResultColumn result) {
      _index = index;
      _result = result;
    }

    public InsertResultVisitor(int index
                              , string exprStr
                              , Identifier aliasName
                              , DBMSType dbmsType = DBMSType.Unknown
                              , bool forSqlAccessor = false){
      _index = index;
      var expr = MiniSqlParserAST.CreateExpr(exprStr, dbmsType, forSqlAccessor);
      _result = new ResultExpr(expr, true, aliasName);
    }

    // 抽出元の別名を取得する
    // 別名のないサブクエリには別名を付加する
    private List<Table> GetAliasNamesFromSource(IFromSource fromSource) {
      var ret = new List<Table>();
      if(fromSource.Type == FromSourceType.Table) {
        ret.Add((Table)fromSource);
      } else if(fromSource.Type == FromSourceType.AliasedQuery) {
        var aliasedQuery = (AliasedQuery)fromSource;
        if(string.IsNullOrEmpty(aliasedQuery.AliasName)) {
          // サブクエリの別名を変更すると、そのサブクエリのSELECT句を参照する
          // 式も変更する必要がある. SqlAccessorではサブクエリに別名を付加する
          // ことを強制しているので、実装を保留する.
          throw new NotImplementedException("Can't deal with sub query that has no alias name");
        }
        ret.Add(new Table(aliasedQuery.AliasName));
      } else if(fromSource.Type == FromSourceType.Join) {
        ret.AddRange(this.GetAliasNamesFromSource(((JoinSource)fromSource).Left));
        ret.AddRange(this.GetAliasNamesFromSource(((JoinSource)fromSource).Right));
      } else if(fromSource.Type == FromSourceType.CommaJoin) {
        ret.AddRange(this.GetAliasNamesFromSource(((CommaJoinSource)fromSource).Left));
        ret.AddRange(this.GetAliasNamesFromSource(((CommaJoinSource)fromSource).Right));
      } else if(fromSource.Type == FromSourceType.Bracketed) {
        var bracketedSource = (BracketedSource)fromSource;
        if(!string.IsNullOrEmpty(bracketedSource.AliasName)) {
          ret.Add(new Table(bracketedSource.AliasName));
        } else {
          ret.AddRange(this.GetAliasNamesFromSource(((BracketedSource)fromSource).Operand));
        }
      }
      return ret;
    }

    private void InsertResult(SingleQueryClause query, int index, ResultColumn result) {
      // SELECT句が'*'かつFrom句が存在する場合、Table.*に置き換える
      if(query.HasWildcard) {
        if(!query.HasFrom) {
          throw new InvalidASTStructureError(
            "'*'が参照するFROM句が存在しないため、SELECT句を追加できません");
        }
        query.HasWildcard = false;
        // From句で参照する抽出元の別名を取得する
        var tableWildcards = new List<TableWildcard>();
        foreach(var table in this.GetAliasNamesFromSource(query.From)) {
          if(!string.IsNullOrEmpty(table.AliasName)) {
            tableWildcards.Add(new TableWildcard(table.AliasName));
          } else {
            tableWildcards.Add(new TableWildcard(table.ServerName
                                               , table.DataBaseName
                                               , table.SchemaName
                                               , table.Name));
          }
        }
        query.Results.Clear();
        query.Results.AddRange(tableWildcards);
      }
      // 指定されたSelect句を追加する
      query.Results.Insert(index, result);
    }

    //public override void VisitBefore(SingleQuery query) {
    //  this.VisitBefore((SingleQueryClause)query);
    //}

    //public override void VisitBefore(CompoundQuery compoundQuery) {
    //  this.VisitBefore((CompoundQueryClause)compoundQuery);
    //}

    //public override void VisitBefore(BracketedQuery bracketedQuery) {
    //  this.VisitBefore((BracketedQueryClause)bracketedQuery);
    //}

    public override void VisitBefore(SingleQueryClause query) {
      if(_added) {
        return;
      }
      this.InsertResult(query, _index, _result);
      _added = true;
    }

    public override void VisitBefore(CompoundQueryClause compoundQuery) {
      if(_added) {
        return;
      }
      compoundQuery.Left.Accept(this);
      compoundQuery.Right.Accept(new InsertResultVisitor(_index, _result.Clone()));
    }

    public override void VisitBefore(BracketedQueryClause bracketedQuery) {
      if(_added) {
        return;
      }
      bracketedQuery.Operand.Accept(this);
    }
  }
}
