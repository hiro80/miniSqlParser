
namespace MiniSqlParser
{
  public class MergeUpdateClause : Node
  {
    internal MergeUpdateClause(Assignments assignments, Comments comments) {
      this.Comments = comments;
      this.Assignments = assignments;
    }

    private Assignments _assignments;
    public Assignments Assignments {
      get {
        return _assignments;
      }
      private set {
        _assignments = value;
        this.SetParent(value);
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Assignments.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
