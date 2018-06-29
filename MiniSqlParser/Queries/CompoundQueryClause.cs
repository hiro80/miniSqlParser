
namespace MiniSqlParser
{
  public class CompoundQueryClause: Node, IQueryClause
  {
    internal CompoundQueryClause(IQueryClause left
                                , CompoundType operater
                                , IQueryClause right
                                , Comments comments) {
      this.Left = left;
      this.Operator = operater;
      this.Right = right;
      this.Comments = comments;
      //this.IsSubQuery = true;
    }

    private IQueryClause _left;
    public IQueryClause Left {
      get {
        return _left;
      }
      set {
        _left = value;
        this.SetParent(value);
      }
    }

    public CompoundType Operator { get; set; }

    private IQueryClause _right;
    public IQueryClause Right {
      get {
        return _right;
      }
      set {
        _right = value;
        this.SetParent(value);
      }
    }

    public QueryType Type {
      get {
        return QueryType.Compound;
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

      this.Left.Accept(visitor);

      visitor.VisitOnCompoundOp(this, offset);
      offset += this.Operator == CompoundType.UnionAll ? 2 : 1;

      this.Right.Accept(visitor);

      visitor.VisitAfter(this);
    }

    public void AcceptOnMainQuery(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Left.AcceptOnMainQuery(visitor);
      this.Right.AcceptOnMainQuery(visitor);
      visitor.VisitAfter(this);
    }
  }
}
