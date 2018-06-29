using System.ComponentModel;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("AliasName: {AliasName}")]
  public class AliasedQuery : Node, IFromSource
  {
    public AliasedQuery(IQueryClause queryClause, bool hasAs, Identifier aliasName)
      :this(CreateQuery(queryClause),hasAs,aliasName){
    }

    public AliasedQuery(IQuery query, bool hasAs, Identifier aliasName) {
      this.Comments = new Comments(2);
      this.Query = query;
      this.HasAs = hasAs;
      this.AliasName = aliasName;
    }

    internal AliasedQuery(IQuery query, bool hasAs, Identifier aliasName, Comments comments) {
      this.Comments = comments;
      this.Query = query;
      _hasAs = hasAs;
      _aliasName = aliasName;
    }

    private static IQuery CreateQuery(IQueryClause queryClause) {
      if(queryClause.Type == QueryType.Single) {
        return new SingleQuery((SingleQueryClause)queryClause);
      } else if(queryClause.Type == QueryType.Compound) {
        return new CompoundQuery((CompoundQueryClause)queryClause);
      } else if(queryClause.Type == QueryType.Bracketed) {
        return new BracketedQuery((BracketedQueryClause)queryClause);
      } else {
        throw new InvalidEnumArgumentException("Undefined QueryType is used"
                                              , (int)queryClause.Type
                                              , typeof(QueryType));
      }
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

    private bool _hasAs;
    public bool HasAs {
      get {
        return _hasAs;
      }
      set {
        this.CorrectComments(_hasAs, value, 2);
        _hasAs = value;
      }
    }

    private Identifier _aliasName;
    public Identifier AliasName {
      get {
        return _aliasName;
      }
      set {
        this.CorrectComments(_aliasName, value, 2, _hasAs);
        _aliasName = value;
      }
    }

    public FromSourceType Type {
      get {
        return FromSourceType.AliasedQuery;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Query.Accept(visitor);
      visitor.VisitAfter(this);
    }

  }
}
