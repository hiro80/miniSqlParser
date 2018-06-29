using System.Collections.Generic;

namespace MiniSqlParser
{
  public class OrderBy: NodeCollections<OrderingTerm>
  {
    private OrderBy(List<OrderingTerm> terms) {
      prefixTerminalNodeCount = 2;
      suffiexTerminalNodeCount = 0;
      nodes = terms;
      foreach(var t in terms) {
        this.SetParent(t);
      }
    }

    public OrderBy(params OrderingTerm[] terms)
      : this(new List<OrderingTerm>()) {
      this.Comments = new Comments();
      this.AddRange(terms);
    }

    public OrderBy(IEnumerable<OrderingTerm> terms)
      : this(new List<OrderingTerm>()) {
      this.Comments = new Comments();
      this.AddRange(terms);
    }

    internal OrderBy(List<OrderingTerm> terms, Comments comments)
      : this(terms) {
      this.Comments = comments;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(nodes.Count > 0) {
        nodes[0].Accept(visitor);
      }
      for(var i = 1; i < nodes.Count; ++i) {
        visitor.VisitOnSeparator(this, 2, i - 1);
        nodes[i].Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }
}
