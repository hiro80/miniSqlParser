
namespace MiniSqlParser
{
  public class NotPredicate : Predicate
  {
    public NotPredicate(Predicate operand) {
      this.Comments = new Comments(1);
      this.Operand = operand;
    }

    internal NotPredicate(Predicate operand, Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
    }

    private Predicate _operand;
    public Predicate Operand {
      get {
        return _operand;
      }
      set {
        _operand = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
