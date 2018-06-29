
namespace MiniSqlParser
{
  public class BracketedExpr : Expr
  {
    public BracketedExpr(Expr operand) {
      this.Comments = new Comments(2);
      this.Operand = operand;
    }

    internal BracketedExpr(Expr operand, Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
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
