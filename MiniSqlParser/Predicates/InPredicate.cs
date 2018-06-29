
namespace MiniSqlParser
{
  public class InPredicate : Predicate
  {
    public InPredicate(Expr operand
                      , bool not
                      , Exprs operands) {
      this.Comments = new Comments(not ? 1 : 0 + 3);
      this.Operand = operand;
      this.Not = not;
      this.Operands = operands;
    }

    public InPredicate(Expr operand
                      , bool not
                      , IQuery query) {
      this.Comments = new Comments(not ? 1 : 0 + 3);
      this.Operand = operand;
      this.Not = not;
      this.Query = query;
    }

    internal InPredicate(Expr operand
                        , bool not
                        , Exprs operands
                        , IQuery query
                        , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.Not = not;
      this.Operands = operands;
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

    public bool Not { get; set; }

    private Exprs _operands;
    public Exprs Operands {
      get {
        return _operands;
      }
      set {
        _operands = value;
        this.SetParent(value);
      }
    }

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

    public bool HasSubQuery {
      get {
        return this.Query != null;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.Visit(this, 0);
      if(this.HasSubQuery) {
        this.Query.Accept(visitor);
      } else {
        this.Operands.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }

  }
}
