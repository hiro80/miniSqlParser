
namespace MiniSqlParser
{
  public class PartitioningTerm : Node
  {
    public PartitioningTerm(Expr term
                          , Identifier collation) {
      this.Comments = new Comments();
      this.Term = term;
      this.Collation = collation;
    }

    internal PartitioningTerm(Expr term
                            , Identifier collation
                            , Comments comments) {
      this.Comments = comments;
      this.Term = term;
      _collation = collation;
    }

    private Expr _term;
    public Expr Term {
      get {
        return _term;
      }
      set {
        _term = value;
        this.SetParent(value);
      }
    }

    private Identifier _collation;
    public Identifier Collation {
      get {
        return _collation;
      }
      set {
        this.CorrectComments(_collation, value, 0);
        this.CorrectComments(_collation, value, 0);
        _collation = value;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Term.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
