
namespace MiniSqlParser
{
  public class ExistsPredicate : Predicate
  {
    public ExistsPredicate(IQuery query) {
      this.Comments = new Comments(3);
      this.Query = query;
    }

    internal ExistsPredicate(IQuery query, Comments comments) {
      this.Comments = comments;
      this.Query = query;
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

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Query.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
