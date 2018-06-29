
namespace MiniSqlParser
{
  public class SingleQuery : SingleQueryClause, IQuery
  {
    internal SingleQuery(QuantifierType quantifier
                        , bool hasTop
                        , int top
                        , bool hasWildcard
                        , ResultColumns results
                        , IFromSource from
                        , Predicate where
                        , GroupBy groupBy
                        , Predicate having
                        , OrderBy orderBy
                        , ILimitClause limit
                        , Comments comments)
      : base(quantifier, hasTop, top, hasWildcard, results, from, where, groupBy, having, comments) {
      this.OrderBy = orderBy;
      this.Limit = limit;
      //this.IsSubQuery = true;
    }

    public SingleQuery(SingleQueryClause singleQueryClause
                        , OrderBy orderBy
                        , ILimitClause limit) 
      :this(singleQueryClause.Quantifier
          , singleQueryClause.HasTop
          , singleQueryClause.Top
          , singleQueryClause.HasWildcard
          , singleQueryClause.Results
          , singleQueryClause.From
          , singleQueryClause.Where
          , singleQueryClause.GroupBy
          , singleQueryClause.Having
          , orderBy
          , limit
          , singleQueryClause.Comments){
    }

    internal SingleQuery(SingleQueryClause singleQueryClause)
      : this(singleQueryClause
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

      int offset = 1;
      if(this.Quantifier != QuantifierType.None) {
        offset += 1;
      }
      if(this.HasTop) {
        offset += 2;
      }

      if(visitor.VisitOnFromFirstInQuery) {
        if(this.HasFrom) {
          visitor.VisitOnFrom(this, offset);
          this.From.Accept(visitor);
          ++offset;
        }

        if(this.HasWhere) {
          visitor.VisitOnWhere(this, offset);
          this.Where.Accept(visitor);
          ++offset;
        }

        if(this.HasGroupBy) {
          this.GroupBy.Accept(visitor);
          if(this.HasHaving) {
            visitor.VisitOnHaving(this, offset);
            this.Having.Accept(visitor);
            ++offset;
          }
        }

        if(this.HasWildcard) {
          visitor.VisitOnWildCard(this, offset);
          offset += 1;
        } else {
          this.Results.Accept(visitor);
        }

      } else {
        if(this.HasWildcard) {
          visitor.VisitOnWildCard(this, offset);
          offset += 1;
        } else {
          this.Results.Accept(visitor);
        }

        if(this.HasFrom) {
          visitor.VisitOnFrom(this, offset);
          this.From.Accept(visitor);
          ++offset;
        }

        if(this.HasWhere) {
          visitor.VisitOnWhere(this, offset);
          this.Where.Accept(visitor);
          ++offset;
        }

        if(this.HasGroupBy) {
          this.GroupBy.Accept(visitor);
          if(this.HasHaving) {
            visitor.VisitOnHaving(this, offset);
            this.Having.Accept(visitor);
            ++offset;
          }
        }
      }

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
