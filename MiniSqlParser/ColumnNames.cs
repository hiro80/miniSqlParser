using System.Collections.Generic;

namespace MiniSqlParser
{
  public class ColumnNames: NodeCollections<Column>
  {
    private ColumnNames(List<Column> columnNames) {
      prefixTerminalNodeCount = 1;
      suffiexTerminalNodeCount = 1;
      nodes = columnNames;
      foreach(var c in columnNames) {
        this.SetParent(c);
      }
    }

    public ColumnNames(params Column[] columnNames)
      : this(new List<Column>()) {
      this.Comments = new Comments();
      this.AddRange(columnNames);
    }

    public ColumnNames(IEnumerable<Column> columnNames)
      : this(new List<Column>()) {
      this.Comments = new Comments();
      this.AddRange(columnNames);
    }

    internal ColumnNames(List<Column> columnNames, Comments comments)
      : this(columnNames) {
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
