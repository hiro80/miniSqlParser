
namespace MiniSqlParser
{
  public class IsPredicate : Predicate
  {
    public IsPredicate(Expr left, bool not, Expr right) {
      this.Comments = new Comments(not ? 1 : 0 + 1);
      this.Left = left;
      this.Not = not;
      this.Right = right;
    }

    internal IsPredicate(Expr left, bool not, Expr right, Comments comments) {
      this.Comments = comments;
      this.Left = left;
      this.Not = not;
      this.Right = right;
    }

    private Expr _left;
    public Expr Left {
      get {
        return _left;
      }
      set {
        _left = value;
        this.SetParent(value);
      }
    }

    public bool Not { get; set; }

    private Expr _right;
    public Expr Right {
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
      visitor.Visit(this, 0);
      this.Right.Accept(visitor);
      visitor.VisitAfter(this);
    }

  }
}
