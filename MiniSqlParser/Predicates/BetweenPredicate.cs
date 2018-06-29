
namespace MiniSqlParser
{
  public class BetweenPredicate : Predicate
  {
    public BetweenPredicate(Expr operand
                          , bool not
                          , Expr from
                          , Expr to) {
      this.Comments = new Comments(not ? 1 : 0 + 2);
      this.Operand = operand;
      this.Not = not;
      this.From = from;
      this.To = to;
    }

    internal BetweenPredicate(Expr operand
                            , bool not
                            , Expr from
                            , Expr to
                            , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.Not = not;
      this.From = from;
      this.To = to;
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

    private Expr _from;
    public Expr From {
      get {
        return _from;
      }
      set {
        _from = value;
        this.SetParent(value);
      }
    }

    private Expr _to;
    public Expr To {
      get {
        return _to;
      }
      set {
        _to = value;
        this.SetParent(value);
      }
    }

    public bool Not { get; set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      int offset = this.Not ? 1 : 0;
      visitor.VisitOnBetween(this, offset);
      offset += 1;
      this.From.Accept(visitor);
      visitor.VisitOnAnd(this, offset);
      this.To.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
