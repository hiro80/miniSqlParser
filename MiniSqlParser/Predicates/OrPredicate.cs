
namespace MiniSqlParser
{
  public class OrPredicate: Predicate
  {
    public OrPredicate(Predicate left, Predicate right) {
      this.Comments = new Comments(1);
      this.Left = left;
      this.Right = right;
    }

    internal OrPredicate(Predicate left, Predicate right, Comments comments) {
      this.Comments = comments;
      this.Left = left;
      this.Right = right;
    }

    private Predicate _left;
    public Predicate Left {
      get {
        return _left;
      }
      set {
        _left = value;
        this.SetParent(value);
      }
    }

    private Predicate _right;
    public Predicate Right {
      get {
        return _right;
      }
      set {
        _right = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Left.Accept(visitor);
      visitor.Visit(this);
      this.Right.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
