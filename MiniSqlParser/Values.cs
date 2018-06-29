using System.Collections.Generic;

namespace MiniSqlParser
{
  public class Values : NodeCollections<IValue>
  {
    private Values(List<IValue> values) {
      prefixTerminalNodeCount = 1;
      suffiexTerminalNodeCount = 1;
      nodes = values;
      foreach(var v in values) {
        this.SetParent(v);
      }
    }

    public Values(params IValue[] values)
      : this(new List<IValue>()) {
      this.Comments = new Comments();
      this.AddRange(values);
    }

    public Values(IEnumerable<IValue> values)
      : this(new List<IValue>()) {
      this.Comments = new Comments();
      this.AddRange(values);
    }

    internal Values(List<IValue> values, Comments comments)
      : this(values) {
      this.Comments = comments;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      visitor.VisitOnLParen(this, 0);
      if(nodes.Count > 0) {
        nodes[0].Accept(visitor);
      }
      int i;
      for(i = 1; i < nodes.Count; ++i) {
        visitor.VisitOnSeparator(this, 1, i - 1);
        nodes[i].Accept(visitor);
      }
      visitor.VisitOnRParen(this, i);
      visitor.VisitAfter(this);
    }
  }
}
