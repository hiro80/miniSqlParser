
namespace MiniSqlParser
{
  public class BracketedPredicate : Predicate
  {
    public BracketedPredicate(Predicate operand) {
      this.Comments = new Comments(2);
      this.Operand = operand;
    }

    internal BracketedPredicate(Predicate operand, Comments comments) {
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
