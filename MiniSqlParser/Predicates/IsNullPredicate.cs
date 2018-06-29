
namespace MiniSqlParser
{
  public class IsNullPredicate : Predicate
  {
    public IsNullPredicate(Expr operand, bool not = false) {
      this.Comments = new Comments(not ? 1 : 0 + 2);
      this.Operand = operand;
      this.Not = not;
    }

    internal IsNullPredicate(Expr operand, bool not, Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.Not = not;
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

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }

  }
}
