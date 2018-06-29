
namespace MiniSqlParser
{
  public class CastExpr : Expr
  {
    public CastExpr(Expr operand, Identifier typeName) {
      this.Comments = new Comments(5);
      this.Operand = operand;
      this.TypeName = typeName;
    }

    internal CastExpr(Expr operand, Identifier typeName, Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.TypeName = typeName;
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

    public Identifier TypeName { get; set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
