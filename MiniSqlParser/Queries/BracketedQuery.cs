
namespace MiniSqlParser
{
  public class BracketedQuery: BracketedQueryClause, IQuery
  {
    internal BracketedQuery(IQueryClause operand
                          , OrderBy orderBy
                          , ILimitClause limit
                          , Comments comments)
      : base(operand, comments) {
      this.Operand = operand;
      this.OrderBy = orderBy;
      this.Limit = limit;
      //this.IsSubQuery = true;
    }

    internal BracketedQuery(BracketedQueryClause bracketedQueryClause
                          , OrderBy orderBy
                          , ILimitClause limit)
      : this(bracketedQueryClause.Operand
            , orderBy
            , limit
            , bracketedQueryClause.Comments) {
    }

    internal BracketedQuery(BracketedQueryClause bracketedQueryClause)
      : this(bracketedQueryClause
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

      visitor.VisitOnLParen(this, offset);
      ++offset;
      this.Operand.Accept(visitor);
      visitor.VisitOnRParen(this, offset);
      ++offset;

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
