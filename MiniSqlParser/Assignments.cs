using System.Collections.Generic;

namespace MiniSqlParser
{
  public class Assignments: NodeCollections<Assignment>
  {
    private Assignments(List<Assignment> assignments) {
      prefixTerminalNodeCount = 0;
      suffiexTerminalNodeCount = 0;
      nodes = assignments;
      foreach(var a in assignments) {
        this.SetParent(a);
      }
    }

    public Assignments(params Assignment[] assignments)
      : this(new List<Assignment>()) {
      this.Comments = new Comments();
      this.AddRange(assignments);
    }

    public Assignments(IEnumerable<Assignment> assignments)
      : this(new List<Assignment>()) {
      this.Comments = new Comments();
      this.AddRange(assignments);
    }

    internal Assignments(List<Assignment> assignments, Comments comments)
      : this(assignments) {
      this.Comments = comments;
    }

    override protected void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(nodes.Count > 0) {
        nodes[0].Accept(visitor);
      }
      int i;
      for(i = 1; i < nodes.Count; ++i) {
        visitor.VisitOnSeparator(this, 0, i - 1);
        nodes[i].Accept(visitor);
      }
      visitor.VisitAfter(this);
    }

  }
}