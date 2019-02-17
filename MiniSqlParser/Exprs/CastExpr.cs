
namespace MiniSqlParser
{
  public class CastExpr : Expr
  {
    public CastExpr(Expr operand
                  , Identifier typeName
                  , bool isPostgreSqlHistoricalCast=false) {
      if(isPostgreSqlHistoricalCast){
        this.Comments = new Comments(2);
      }else{
        this.Comments = new Comments(5);
      }
      this.Operand = operand;
      this.TypeName = typeName;
      this.IsPostgreSqlHistoricalCast = isPostgreSqlHistoricalCast;
    }

    internal CastExpr(Expr operand
                  , Identifier typeName
                  , bool isPostgreSqlHistoricalCast
                  , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      this.TypeName = typeName;
      this.IsPostgreSqlHistoricalCast = isPostgreSqlHistoricalCast;
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

    public bool IsPostgreSqlHistoricalCast { get; private set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
