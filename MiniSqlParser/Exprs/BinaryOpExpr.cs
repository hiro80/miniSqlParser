
namespace MiniSqlParser
{
  public class BinaryOpExpr : Expr
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

    public ExpOperator Operator { get; private set; }

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

    public BinaryOpExpr(Expr left, ExpOperator op, Expr right) {
      this.Comments = new Comments(1);
      this.Left = left;
      this.Operator = op;
      this.Right = right;
      this.SetParent(left);
      this.SetParent(right);
    }

    internal BinaryOpExpr(Expr left, ExpOperator op, Expr right, Comments comments) {
      this.Comments = comments;
      this.Left = left;
      this.Operator = op;
      this.Right = right;
      this.SetParent(left);
      this.SetParent(right);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Left.Accept(visitor);
      visitor.VisitOnOperator(this);
      this.Right.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
