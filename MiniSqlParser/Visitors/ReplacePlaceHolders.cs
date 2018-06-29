using System;
using System.Collections.Generic;

namespace MiniSqlParser
{
  /// <summary>
  /// プレースホルダNodeを指定するNodeに置き換える
  /// </summary>
  public class ReplacePlaceHolders : Visitor
  {
    // プレースホルダ値にプレースホルダがある場合、プレースホルダの置き換え処理が
    // 停止しない可能性があるため、プレースホルダ値内のプレースホルダは置き換えない
    // そのため、置き換え処理はVisitAfterで行う必要がある.

    private readonly Dictionary<string, Node> _placeHolders;

    private readonly Dictionary<string, Node> _placedHolders;

    public ReplacePlaceHolders(Dictionary<string, string> placeHolders) {
      _placeHolders = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);
      _placedHolders = new Dictionary<string, Node>();
    }

    public ReplacePlaceHolders(Dictionary<string, Node> placeHolders) {
      _placeHolders = placeHolders;
      _placedHolders = new Dictionary<string, Node>();
    }

    public Dictionary<string, Node> PlacedHolders {
      get {
        return _placedHolders;
      }
    }

    public override void VisitAfter(Assignment assignment) {
      if(!assignment.Value.IsDefault && IsPlaceHolderExpr((Expr)assignment.Value)) {
        assignment.Value = PlaceValue((PlaceHolderExpr)assignment.Value);
      }
    }

    public override void VisitAfter(Values values) {
      for(int i = 0; i < values.Count; ++i) {
        if(!values[i].IsDefault && IsPlaceHolderExpr((Expr)values[i])) {
          values[i] = PlaceValue((PlaceHolderExpr)values[i]);
        }
      }
    }

    public override void VisitAfter(ResultExpr resultExpr) {
      if(IsPlaceHolderExpr(resultExpr.Value)) {
        resultExpr.Value = Place((PlaceHolderExpr)resultExpr.Value);
      }
    }

    public override void VisitAfter(Exprs exprs) {
      for(int i = 0; i < exprs.Count; ++i) {
        if(IsPlaceHolderExpr(exprs[i])) {
          exprs[i] = Place((PlaceHolderExpr)exprs[i]);
        }
      }
    }

    public override void VisitAfter(BinaryOpExpr expr) {
      if(IsPlaceHolderExpr(expr.Left)) {
        expr.Left = PlaceInBinaryOp((PlaceHolderExpr)expr.Left);
      }
      if(IsPlaceHolderExpr(expr.Right)) {
        expr.Right = PlaceInBinaryOp((PlaceHolderExpr)expr.Right);
      }
    }

    public override void VisitAfter(BracketedExpr expr) {
      if(IsPlaceHolderExpr(expr.Operand)) {
        expr.Operand = Place((PlaceHolderExpr)expr.Operand);
      }
    }

    public override void VisitAfter(BitwiseNotExpr expr) {
      if(IsPlaceHolderExpr(expr.Operand)) {
        expr.Operand = PlaceInBitwiseNot((PlaceHolderExpr)expr.Operand);
      }
    }

    public override void VisitAfter(CastExpr expr) {
      if(IsPlaceHolderExpr(expr.Operand)) {
        expr.Operand = Place((PlaceHolderExpr)expr.Operand);
      }
    }

    public override void VisitAfter(CaseExpr expr) {
      if(expr.IsSimpleCase) {
        if(IsPlaceHolderExpr(expr.SearchExpr)) {
          expr.SearchExpr = Place((PlaceHolderExpr)expr.SearchExpr);
        }
        for(int i = 0; i < expr.Comparisons.Count; ++i) {
          if(IsPlaceHolderExpr((Expr)expr.Comparisons[i]) ||
             IsPlaceHolderExpr(expr.Results[i])) {
               expr.SetBranch(i
                           , Place((PlaceHolderExpr)expr.Comparisons[i])
                           , Place((PlaceHolderExpr)expr.Results[i]));
          }
        }

      } else {
        for(int i = 0; i < expr.Conditions.Count; ++i) {
          if(IsPlaceHolderPredicate((Predicate)expr.Conditions[i]) ||
             IsPlaceHolderExpr(expr.Results[i])) {
               expr.SetBranch(i
                            , Place((PlaceHolderPredicate)expr.Conditions[i])
                            , Place((PlaceHolderExpr)expr.Results[i]));
          }
        }
      }

      if(IsPlaceHolderExpr(expr.ElseResult)) {
        expr.ElseResult = Place((PlaceHolderExpr)expr.ElseResult);
      }
    }

    public override void VisitAfter(BinaryOpPredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Left)) {
        predicate.Left = Place((PlaceHolderExpr)predicate.Left);
      }
      if(IsPlaceHolderExpr(predicate.Right)) {
        predicate.Right = Place((PlaceHolderExpr)predicate.Right);
      }
    }

    public override void VisitAfter(LikePredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Operand)) {
        predicate.Operand = Place((PlaceHolderExpr)predicate.Operand);
      }
      if(IsPlaceHolderExpr(predicate.Pattern)) {
        predicate.Pattern = Place((PlaceHolderExpr)predicate.Pattern);
      }
      if(IsPlaceHolderExpr(predicate.Escape)) {
        predicate.Escape = Place((PlaceHolderExpr)predicate.Escape);
      }
    }

    public override void VisitAfter(IsNullPredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Operand)) {
        predicate.Operand = Place((PlaceHolderExpr)predicate.Operand);
      }
    }

    public override void VisitAfter(IsPredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Left)) {
        predicate.Left = Place((PlaceHolderExpr)predicate.Left);
      }
      if(IsPlaceHolderExpr(predicate.Right)) {
        predicate.Right = Place((PlaceHolderExpr)predicate.Right);
      }
    }

    public override void VisitAfter(BetweenPredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Operand)) {
        predicate.Operand = Place((PlaceHolderExpr)predicate.Operand);
      }
      if(IsPlaceHolderExpr(predicate.From)) {
        predicate.From = Place((PlaceHolderExpr)predicate.From);
      }
      if(IsPlaceHolderExpr(predicate.To)) {
        predicate.To = Place((PlaceHolderExpr)predicate.To);
      }
    }

    public override void VisitAfter(InPredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Operand)) {
        predicate.Operand = Place((PlaceHolderExpr)predicate.Operand);
      }
    }

    public override void VisitAfter(SubQueryPredicate predicate) {
      if(IsPlaceHolderExpr(predicate.Operand)) {
        predicate.Operand = Place((PlaceHolderExpr)predicate.Operand);
      }
    }


    public override void VisitAfter(AggregateFuncExpr expr) {
      if(IsPlaceHolderExpr(expr.Argument1)) {
        expr.Argument1 = Place((PlaceHolderExpr)expr.Argument1);
      }
      if(IsPlaceHolderExpr(expr.Argument2)) {
        expr.Argument2 = Place((PlaceHolderExpr)expr.Argument2);
      }
    }

    public override void VisitAfter(SubstringFunc expr) {
      if(IsPlaceHolderExpr(expr.Argument1)) {
        expr.Argument1 = Place((PlaceHolderExpr)expr.Argument1);
      }
      if(IsPlaceHolderExpr(expr.Argument2)) {
        expr.Argument2 = Place((PlaceHolderExpr)expr.Argument2);
      }
      if(IsPlaceHolderExpr(expr.Argument3)) {
        expr.Argument3 = Place((PlaceHolderExpr)expr.Argument3);
      }
    }

    public override void VisitAfter(ExtractFuncExpr expr) {
      if(IsPlaceHolderExpr(expr.Argument)) {
        expr.Argument = Place((PlaceHolderExpr)expr.Argument);
      }
    }

    public override void VisitAfter(GroupBy groupBy) {
      for(int i = 0; i < groupBy.Count; ++i) {
        if(IsPlaceHolderExpr(groupBy[i])) {
          groupBy[i] = Place((PlaceHolderExpr)groupBy[i]);
        }
      }
    }

    public override void VisitAfter(OrderingTerm orderingTerm) {
      if(IsPlaceHolderExpr(orderingTerm.Term)) {
        orderingTerm.Term = Place((PlaceHolderExpr)orderingTerm.Term);
      }
    }

    public override void VisitAfter(PartitioningTerm partitioningTerm) {
      if(IsPlaceHolderExpr(partitioningTerm.Term)) {
        partitioningTerm.Term = Place((PlaceHolderExpr)partitioningTerm.Term);
      }
    }

    public override void VisitAfter(ILimitClause iLimitClause) {
      if(iLimitClause.Type == LimitClauseType.Limit) {
        var limitClause = (LimitClause)iLimitClause;
        if(IsPlaceHolderExpr(limitClause.Offset)) {
          limitClause.Offset = Place((PlaceHolderExpr)limitClause.Offset);
        }
        if(IsPlaceHolderExpr(limitClause.Limit)) {
          limitClause.Limit = Place((PlaceHolderExpr)limitClause.Limit);
        }
      }
    }


    public override void VisitAfter(AndPredicate andPredicate) {
      if(IsPlaceHolderPredicate(andPredicate.Left)) {
        andPredicate.Left = PlaceInAnd((PlaceHolderPredicate)andPredicate.Left);
      }
      if(IsPlaceHolderPredicate(andPredicate.Right)) {
        andPredicate.Right = PlaceInAnd((PlaceHolderPredicate)andPredicate.Right);
      }
    }

    public override void VisitAfter(OrPredicate orPredicate) {
      if(IsPlaceHolderPredicate(orPredicate.Left)) {
        orPredicate.Left = Place((PlaceHolderPredicate)orPredicate.Left);
      }
      if(IsPlaceHolderPredicate(orPredicate.Right)) {
        orPredicate.Right = Place((PlaceHolderPredicate)orPredicate.Right);
      }
    }

    public override void VisitAfter(NotPredicate notPredicate) {
      if(IsPlaceHolderPredicate(notPredicate.Operand)) {
        notPredicate.Operand = PlaceInNot((PlaceHolderPredicate)notPredicate.Operand);
      }
    }

    public override void VisitAfter(BracketedPredicate predicate) {
      if(IsPlaceHolderPredicate(predicate.Operand)) {
        predicate.Operand = Place((PlaceHolderPredicate)predicate.Operand);
      }
    }

    public override void VisitAfter(CollatePredicate predicate) {
      if(IsPlaceHolderPredicate(predicate.Operand)) {
        predicate.Operand = PlaceInCollate((PlaceHolderPredicate)predicate.Operand);
      }
    }

    public override void VisitAfter(JoinSource joinSource) {
      if(IsPlaceHolderPredicate(joinSource.Constraint)) {
        joinSource.Constraint = Place((PlaceHolderPredicate)joinSource.Constraint);
      }
    }

    public override void VisitAfter(SingleQueryClause query) {
      if(IsPlaceHolderPredicate(query.Where)) {
        query.Where = Place((PlaceHolderPredicate)query.Where);
      }
      if(IsPlaceHolderPredicate(query.Having)) {
        query.Having = Place((PlaceHolderPredicate)query.Having);
      }
    }

    public override void VisitAfter(UpdateStmt updateStmt) {
      if(IsPlaceHolderPredicate(updateStmt.Where)) {
        updateStmt.Where = Place((PlaceHolderPredicate)updateStmt.Where);
      }
    }

    public override void VisitAfter(DeleteStmt deleteStmt) {
      if(IsPlaceHolderPredicate(deleteStmt.Where)) {
        deleteStmt.Where = Place((PlaceHolderPredicate)deleteStmt.Where);
      }
    }

    public override void VisitAfter(MergeStmt mergeStmt) {
      if(IsPlaceHolderPredicate(mergeStmt.Constraint)) {
        mergeStmt.Constraint = Place((PlaceHolderPredicate)mergeStmt.Constraint);
      }
    }

    public override void VisitAfter(IfStmt ifStmt) {
      for(int i = 0; i < ifStmt.Conditions.Count; ++i) {
        if(IsPlaceHolderPredicate(ifStmt.Conditions[i])) {
          ifStmt.SetBranch(i
                         , Place((PlaceHolderPredicate)ifStmt.Conditions[i])
                         , ifStmt.StatementsList[i]);
        }
      }
    }

    public override void VisitAfter(SqlitePragmaStmt pragmaStmt) {
      if(pragmaStmt.HasPlaceHolder) {
        pragmaStmt.Table = PlaceInSqlitePragma(pragmaStmt.TableName);
      }
    }

    protected virtual IValue PlaceValue(PlaceHolderExpr ph) {
      var placeHolderName = ph.LabelName;
      if(!_placeHolders.ContainsKey(placeHolderName)) {
        return ph;
      }
      var placeHolderValue = _placeHolders[placeHolderName];
      if(placeHolderValue is Predicate) {
        throw new CannotBuildASTException("Type of placeholder value is mismatched.");
      }
      // 適用したプレースホルダを記録する
      if(!_placedHolders.ContainsKey(placeHolderName)) {
        _placedHolders.Add(placeHolderName, placeHolderValue);
      }
      return (IValue)placeHolderValue;
    }

    private bool IsPlaceHolderExpr(Expr expr) {
      return expr != null && expr.GetType() == typeof(PlaceHolderExpr);
    }

    protected virtual Expr Place(PlaceHolderExpr ph) {
      var placeHolderName = ph.LabelName;
      if(!_placeHolders.ContainsKey(placeHolderName)) {
        return ph;
      }
      var placeHolderValue = _placeHolders[placeHolderName];
      if(placeHolderValue is Predicate) {
        throw new CannotBuildASTException("Type of placeholder value is mismatched.");
      }
      // 適用したプレースホルダを記録する
      if(!_placedHolders.ContainsKey(placeHolderName)) {
        _placedHolders.Add(placeHolderName, placeHolderValue);
      }
      return (Expr)placeHolderValue;
    }

    private bool IsPlaceHolderPredicate(Predicate predicate) {
      return predicate != null && predicate.GetType() == typeof(PlaceHolderPredicate);
    }

    protected virtual Predicate Place(PlaceHolderPredicate ph) {
      var placeHolderName = ph.LabelName;
      if(!_placeHolders.ContainsKey(placeHolderName)) {
        return ph;
      }
      var placeHolderValue = _placeHolders[placeHolderName];
      if(placeHolderValue is Expr) {
        throw new CannotBuildASTException("Type of placeholder value is mismatched.");
      }
      // 適用したプレースホルダを記録する
      if(!_placedHolders.ContainsKey(placeHolderName)) {
        _placedHolders.Add(placeHolderName, placeHolderValue);
      }
      return (Predicate)placeHolderValue;
    }

    private Predicate PlaceInAnd(PlaceHolderPredicate ph) {
      var predicate = this.Place(ph);
      if(predicate.GetType() == typeof(OrPredicate)) {
        // OR演算式の場合は式全体を括弧で囲んでAND節をつなげる
        predicate = new BracketedPredicate(predicate);
      }
      return predicate;
    }

    private Predicate PlaceInNot(PlaceHolderPredicate ph) {
      var predicate = this.Place(ph);
      if(predicate.GetType() == typeof(AndPredicate) ||
         predicate.GetType() == typeof(OrPredicate)) {
        predicate = new BracketedPredicate(predicate);
      }
      return predicate;
    }

    private Predicate PlaceInCollate(PlaceHolderPredicate ph) {
      var predicate = this.Place(ph);
      if(predicate.GetType() == typeof(AndPredicate) ||
         predicate.GetType() == typeof(OrPredicate)) {
        predicate = new BracketedPredicate(predicate);
      }
      return predicate;
    }

    // Exprの演算子の種別は多く場合分けが煩雑になるので
    // 二項演算子内に二項演算子を適用する場合は全て括弧で囲む
    private Expr PlaceInBinaryOp(PlaceHolderExpr ph) {
      var expr = this.Place(ph);
      if(expr.GetType() == typeof(BinaryOpExpr)) {
        expr = new BracketedExpr(expr);
      }
      return expr;
    }

    private Expr PlaceInBitwiseNot(PlaceHolderExpr ph) {
      var expr = this.Place(ph);
      if(expr.GetType() == typeof(BinaryOpExpr)) {
        expr = new BracketedExpr(expr);
      }
      return expr;
    }

    private Table PlaceInSqlitePragma(PlaceHolderExpr ph) {
      // TableではなくColumnとして認識される
      var dummyExpr = Place(ph);
      if(dummyExpr.GetType() != typeof(Column)) {
        throw new InvalidASTStructureError(
          "PRAGMA TABLE_INFOにテーブル名以外の値の格納が試みられました");
      }

      // Columnオブジェクトからテーブル名を作成する
      // (SQLiteではテーブル名の名前空間は1階層のみである)
      Table table;
      var dummyColumn = (Column)dummyExpr;
      if(string.IsNullOrEmpty(dummyColumn.TableAliasName)) {
        table = new Table(dummyColumn.Name);
      } else {
        table = new Table(dummyColumn.TableAliasName, dummyColumn.Name);
      }
      return table;
    }
  
  }
}
