
namespace MiniSqlParser
{
  public class CollatePredicate : Predicate
  {
    public CollatePredicate(Predicate operand
                          , Identifier collation) {
      this.Comments = new Comments(2);
      this.Operand = operand;
      this.Collation = collation;
    }

    internal CollatePredicate(Predicate operand
                            , Identifier collation
                            , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.Collation = collation;
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

    public Identifier Collation { get; set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
