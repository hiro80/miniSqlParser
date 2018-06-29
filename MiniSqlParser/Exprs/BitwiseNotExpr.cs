
namespace MiniSqlParser
{
  public class BitwiseNotExpr : Expr
  {
    public BitwiseNotExpr(Expr operand) {
      this.Comments = new Comments(1);
      this.Operand = operand;
      this.SetParent(operand);
    }

    internal BitwiseNotExpr(Expr operand, Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.SetParent(operand);
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

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
