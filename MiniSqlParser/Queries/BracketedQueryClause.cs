
namespace MiniSqlParser
{
  public class BracketedQueryClause: Node, IQueryClause
  {
    internal BracketedQueryClause(IQueryClause operand, Comments comments) {
      this.Operand = operand;
      this.Comments = comments;
      //this.IsSubQuery = true;
    }

    private IQueryClause _operand;
    public IQueryClause Operand {
      get {
        return _operand;
      }
      set {
        _operand = value;
        this.SetParent(value);
      }
    }

    public QueryType Type {
      get {
        return QueryType.Bracketed;
      }
    }

    public bool IsSubQuery {
      get {
        var parent = this.Parent;
        return parent.GetType() != typeof(SelectStmt) ||
               parent is BracketedQueryClause &&
                 ((BracketedQueryClause)parent).Operand.IsSubQuery;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      int offset = 0;
      visitor.VisitOnLParen(this, offset);
      ++offset;
      this.Operand.Accept(visitor);
      visitor.VisitOnRParen(this, offset);
      ++offset;
      visitor.VisitAfter(this);
    }
    public void AcceptOnMainQuery(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
