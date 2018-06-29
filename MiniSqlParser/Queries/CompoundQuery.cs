
namespace MiniSqlParser
{
  public class CompoundQuery : CompoundQueryClause, IQuery
  {
    internal CompoundQuery(IQueryClause left
                          , CompoundType operater
                          , IQueryClause right
                          , OrderBy orderBy
                          , ILimitClause limit
                          , Comments comments)
      : base(left, operater, right, comments) {
      this.Left = left;
      this.Operator = operater;
      this.Right = right;
      this.OrderBy = orderBy;
      this.Limit = limit;
      //this.IsSubQuery = true;
    }

    internal CompoundQuery(CompoundQueryClause compoundQueryClause
                         , OrderBy orderBy
                         , ILimitClause limit)
      : this(compoundQueryClause.Left
            , compoundQueryClause.Operator
            , compoundQueryClause.Right
            , orderBy
            , limit
            , compoundQueryClause.Comments) {
    }

    internal CompoundQuery(CompoundQueryClause compoundQueryClause)
      : this(compoundQueryClause
            , new OrderBy()
            , null){
    }

    private OrderBy _orderBy;
    public OrderBy OrderBy {
      get {
        return _orderBy;
      }
      private set {
        _orderBy = value;
        this.SetParent(value);
      }
    }

    private ILimitClause _limit;
    public ILimitClause Limit {
      get {
        return _limit;
      }
      set {
        _limit = value;
        this.SetParent(value);
      }
    }

    public bool HasOrderBy {
      get {
        return this.OrderBy != null && this.OrderBy.Count > 0;
      }
    }

    public bool HasLimit {
      get {
        return _limit != null;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      int offset = 0; 

      this.Left.Accept(visitor);

      visitor.VisitOnCompoundOp(this, offset);
      offset += this.Operator == CompoundType.UnionAll ? 2 : 1;

      this.Right.Accept(visitor);

      if(this.HasOrderBy) {
        this.OrderBy.Accept(visitor);
      }

      if(this.HasLimit) {
        this.Limit.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }
}
