
namespace MiniSqlParser
{
  public class SubQueryPredicate : Predicate
  {
    public SubQueryPredicate(Expr operand
                            , PredicateOperator operater
                            , QueryQuantifier quantifier
                            , IQuery query) {
      this.Comments = new Comments(4);
      this.Operand = operand;
      this.Operator = operater;
      this.Quantifier = quantifier;
      this.Query = query;
    }

    internal SubQueryPredicate(Expr operand
                              , PredicateOperator operater
                              , QueryQuantifier quantifier
                              , IQuery query
                              , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.Operator = operater;
      this.Quantifier = quantifier;
      this.Query = query;
    }

    private Expr _operand;
    public Expr Operand {
      get {
        return _operand;
      }
      set {
        _operand = value;
        this.SetParent(value);
      }
    }

    public PredicateOperator Operator { get; set; }
    public QueryQuantifier Quantifier { get; set; }

    private IQuery _query;
    public IQuery Query {
      get {
        return _query;
      }
      set {
        _query = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.Visit(this, 0);
      this.Query.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
