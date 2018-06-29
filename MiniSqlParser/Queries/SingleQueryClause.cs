using System.Collections.Generic;

namespace MiniSqlParser
{
  public class SingleQueryClause: Node, IQueryClause
  {
    internal SingleQueryClause(QuantifierType quantifier
                              , bool hasTop
                              , int top
                              , bool hasWildcard
                              , ResultColumns results
                              , IFromSource from
                              , Predicate where
                              , GroupBy groupBy
                              , Predicate having
                              , Comments comments) {
      _quantifier = quantifier;
      _hasTop = hasTop;
      this.Top = top;
      _hasWildcard = hasWildcard;
      this.Results = results;
      _from = from;
      _where = where;
      this.GroupBy = groupBy;
      _having = having;
      this.Comments = comments;
      //this.IsSubQuery = true;

      this.SetParent(from);
      this.SetParent(where);
      this.SetParent(having);
    }

    public static SingleQueryClause WrapInSelectStar(IFromSource from){
      return new SingleQueryClause(QuantifierType.None
                                  , false
                                  , 0
                                  , true
                                  , new ResultColumns()
                                  , from
                                  , null
                                  , new GroupBy()
                                  , null
                                  , new Comments(3));
    }

    private QuantifierType _quantifier;
    public QuantifierType Quantifier {
      get {
        return _quantifier;
      }
      set {
        this.CorrectComments2(_quantifier, value, 1);
        _quantifier = value;
      }
    }

    private bool _hasTop;
    public bool HasTop {
      get {
        return _hasTop;
      }
      set {
        this.CorrectComments(_hasTop, value, 1, this.Quantifier != QuantifierType.None);
        this.CorrectComments(_hasTop, value, 1, this.Quantifier != QuantifierType.None);
        _hasTop = value;
      }
    }

    public int Top { get; set; }

    private bool _hasWildcard;
    public bool HasWildcard {
      get {
        return _hasWildcard;
      }
      set {
        this.CorrectComments(_hasWildcard, value, 1, this.Quantifier != QuantifierType.None
                                                   , this.HasTop, this.HasTop);
        _hasWildcard = value;
      }
    }

    private ResultColumns _results;
    public ResultColumns Results {
      get {
        return _results;
      }
      private set {
        _results = value;
        this.SetParent(value);
      }
    }

    private IFromSource _from;
    public IFromSource From {
      get {
        return _from;
      }
      set {
        this.CorrectComments(_from, value, 1, this.Quantifier != QuantifierType.None
                                            , this.HasTop, this.HasTop
                                            , this.HasWildcard);
        _from = value;
        this.SetParent(value);
      }
    }

    private Predicate _where;
    public Predicate Where {
      get {
        return _where;
      }
      set {
        this.CorrectComments(_where, value, 1, this.Quantifier != QuantifierType.None
                                              , this.HasTop, this.HasTop
                                              , this.HasWildcard
                                              , this.HasFrom);
        _where = value;
        this.SetParent(value);
      }
    }

    private GroupBy _groupBy;
    public GroupBy GroupBy {
      get {
        return _groupBy;
      }
      private set {
        _groupBy = value;
        this.SetParent(value);
      }
    }

    private Predicate _having;
    public Predicate Having {
      get {
        return _having;
      }
      set {
        this.CorrectComments(_having, value, 1, this.Quantifier != QuantifierType.None
                                                , this.HasTop, this.HasTop
                                                , this.HasWildcard
                                                , this.HasFrom
                                                , this.HasWhere);
        _having = value;
        this.SetParent(value);
      }
    }

    public bool HasFrom {
      get {
        return _from != null;
      }
    }

    public bool HasWhere {
      get {
        return _where != null;
      }
    }

    public bool HasGroupBy {
      get {
        return this.GroupBy != null && this.GroupBy.Count > 0;
      }
    }

    public bool HasHaving {
      get {
        return _having != null;
      }
    }

    public QueryType Type {
      get {
        return QueryType.Single;
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

      int offset = 1;
      if (this.Quantifier != QuantifierType.None){
        offset += 1;
      }
      if(this.HasTop){
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
      }

      if(this.HasGroupBy){
        this.GroupBy.Accept(visitor);
        if(this.HasHaving){
          visitor.VisitOnHaving(this,offset);
          this.Having.Accept(visitor);
          ++offset;
        }
      }

      visitor.VisitAfter(this);
    }

    public void AcceptOnMainQuery(IVisitor visitor) {
      visitor.VisitBefore(this);
      visitor.VisitAfter(this);
    }
  }
}
