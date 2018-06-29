
namespace MiniSqlParser
{
  public class BinaryOpPredicate : Predicate
  {
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

    public PredicateOperator Operator { get; private set; }

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

    public BinaryOpPredicate(Expr left, PredicateOperator op, Expr right) {
      this.Comments = new Comments(1);
      this.Left = left;
      this.Operator = op;
      this.Right = right;
      this.Left.Parent = this;
      this.Right.Parent = this;
    }

    internal BinaryOpPredicate(Expr left, PredicateOperator op, Expr right, Comments comments) {
      this.Comments = comments;
      this.Left = left;
      this.Operator = op;
      this.Right = right;
      this.Left.Parent = this;
      this.Right.Parent = this;
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
