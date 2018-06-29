
namespace MiniSqlParser
{
  public class SignedNumberExpr : Expr
  {
    public SignedNumberExpr(Sign sign, UNumericLiteral operand) {
      this.Comments = new Comments(2);
      this.Sign = sign;
      this.Operand = operand;
    }

    internal SignedNumberExpr(Sign sign, UNumericLiteral operand, Comments comments) {
      this.Comments = comments;
      this.Sign = sign;
      this.Operand = operand;
    }

    public Sign Sign { get; set; }

    private Literal _operand;
    public Literal Operand {
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
