using System.Collections.Generic;

namespace MiniSqlParser
{
  public class PartitionBy : NodeCollections<PartitioningTerm>
  {
    private PartitionBy(List<PartitioningTerm> terms) {
      prefixTerminalNodeCount = 2;
      suffiexTerminalNodeCount = 0;
      nodes = terms;
      foreach(var t in terms) {
        this.SetParent(t);
      }
    }

    public PartitionBy(params PartitioningTerm[] terms)
      : this(new List<PartitioningTerm>()) {
      this.Comments = new Comments();
      this.AddRange(terms);
    }

    public PartitionBy(IEnumerable<PartitioningTerm> terms)
      : this(new List<PartitioningTerm>()) {
      this.Comments = new Comments();
      this.AddRange(terms);
    }

    internal PartitionBy(List<PartitioningTerm> terms, Comments comments)
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
