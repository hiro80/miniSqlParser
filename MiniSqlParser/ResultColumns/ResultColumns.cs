using System.Collections;
using System.Collections.Generic;

namespace MiniSqlParser
{
  public class ResultColumns: NodeCollections<ResultColumn>
  {
    private ResultColumns(List<ResultColumn> resultColumns) {
      prefixTerminalNodeCount = 0;
      suffiexTerminalNodeCount = 0;
      nodes = resultColumns;
      foreach(var r in resultColumns) {
        this.SetParent(r);
      }
    }

    public ResultColumns(params ResultColumn[] resultColumns)
      : this(new List<ResultColumn>()) {
      this.Comments = new Comments();
      this.AddRange(resultColumns);
    }

    public ResultColumns(IEnumerable<ResultColumn> resultColumns)
      : this(new List<ResultColumn>()) {
      this.Comments = new Comments();
      this.AddRange(resultColumns);
    }

    internal ResultColumns(List<ResultColumn> resultColumns, Comments comments)
      : this(resultColumns) {
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

    public bool HasTableWildcard() {
      foreach(var result in this) {
        if(result.IsTableWildcard) {
          return true;
        }
      }
      return false;
    }

  }
}
