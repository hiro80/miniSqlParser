
namespace MiniSqlParser
{
  public class SubQueryExp : Expr
  {
    public SubQueryExp(IQuery operand) {
      this.Comments = new Comments(2);
      this.Query = operand;
    }

    internal SubQueryExp(IQuery operand, Comments comments) {
      this.Comments = comments;
      this.Query = operand;
    }

    private IQuery _query;
    public IQuery Query {
      get {
        return _query;
      }
      set {
        _query = value;
        this.SetParent(value);
      }
    }

    /// <summary>
    /// SubQueryExpノードがSELECT句サブクエリとして使われていればTrueを返す
    /// </summary>
    /// <param name="expr"></param>
    /// <returns></returns>
    public bool IsUsedInResultColumn() {
      var node = this.Parent;
      while(node is Expr || node.GetType() == typeof(Exprs)) {
        node = node.Parent;
      }
      return node.GetType() == typeof(ResultExpr);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Query.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
