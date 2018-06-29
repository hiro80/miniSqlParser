using System.Collections.Generic;

namespace MiniSqlParser
{
  public class ForUpdateOfClause: NodeCollections<Column>
  {
    private ForUpdateOfClause(List<Column> columnNames) {
      prefixTerminalNodeCount = 1;
      suffiexTerminalNodeCount = 0;
      nodes = columnNames;
      foreach(var c in columnNames) {
        this.SetParent(c);
      }
    }

    public ForUpdateOfClause(params Column[] columnNames)
      : this(new List<Column>()) {
      this.Comments = new Comments();
      this.AddRange(columnNames);
    }

    public ForUpdateOfClause(IEnumerable<Column> columnNames)
      : this(new List<Column>()) {
      this.Comments = new Comments();
      this.AddRange(columnNames);
    }

    internal ForUpdateOfClause(List<Column> columnNames, Comments comments)
      : this(columnNames) {
      this.Comments = comments;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(nodes.Count > 0) {
        nodes[0].Accept(visitor);
      }
      int i;
      for(i = 1; i < nodes.Count; ++i) {
        visitor.VisitOnSeparator(this, 1, i - 1);
        nodes[i].Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }
}
