using System.Collections.Generic;

namespace MiniSqlParser
{
  /// <summary>
  /// Expressionが集約関数を含むか否か判定する
  /// </summary>
  public class FindAggregateExprVisitor: SetPlaceHoldersVisitor
  {
    public FindAggregateExprVisitor(Dictionary<string, string> placeHolders = null)
      : base(placeHolders) {
    }

    public bool ContainsAggregativeExpr { get; private set; }

    public override void VisitBefore(AggregateFuncExpr expr) {
      // Expressionがサブクエリの場合、その中に集約関数が含まれていても
      // Expressionは集約式ではない
      this.ContainsAggregativeExpr = this.ContainsAggregativeExpr || _inSubQueryExp == 0;
    }

    private int _inSubQueryExp;

    public override void VisitBefore(SubQueryExp expr) {
      ++_inSubQueryExp;
    }
    public override void VisitAfter(SubQueryExp expr) {
      --_inSubQueryExp;
    }

    //
    // Exists, In, SubQueryPredicateの被演算子に集約関数が含まれていても
    // これらのPredicateは集約式ではない
    //
    public override void VisitBefore(ExistsPredicate predicate) {
      ++_inSubQueryExp;
    }
    public override void VisitAfter(ExistsPredicate predicate) {
      --_inSubQueryExp;
    }

    public override void VisitBefore(InPredicate predicate) {
      ++_inSubQueryExp;
    }
    public override void VisitAfter(InPredicate predicate) {
      --_inSubQueryExp;
    }

    public override void VisitBefore(SubQueryPredicate predicate) {
      ++_inSubQueryExp;
    }
    public override void VisitAfter(SubQueryPredicate predicate) {
      --_inSubQueryExp;
    }

  }
}
