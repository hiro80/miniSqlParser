using System.Collections.Generic;

namespace MiniSqlParser
{
  public class ValuesList : NodeCollections<Values>
  {
    private ValuesList(List<Values> valuesList) {
      prefixTerminalNodeCount = 0;
      suffiexTerminalNodeCount = 0;
      nodes = valuesList;
      foreach(var values in valuesList) {
        this.SetParent(values);
      }
    }

    public ValuesList(params Values[] valuesList)
      : this(new List<Values>()) {
      this.Comments = new Comments();
      this.AddRange(valuesList);
    }

    public ValuesList(IEnumerable<Values> valuesList)
      : this(new List<Values>()) {
      this.Comments = new Comments();
      this.AddRange(valuesList);
    }

    internal ValuesList(List<Values> valuesList, Comments comments)
      : this(valuesList) {
      this.Comments = comments;
    }

    protected override void AcceptImp(IVisitor visitor) {
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
