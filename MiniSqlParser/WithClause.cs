using System.Collections.Generic;

namespace MiniSqlParser
{
  public class WithClause : NodeCollections<WithDefinition>
  {
    private WithClause(bool hasRecursiveKeyword, List<WithDefinition> withDefs) {
      prefixTerminalNodeCount = hasRecursiveKeyword ? 2: 1;
      suffiexTerminalNodeCount = 0;
      this.HasRecursiveKeyword = hasRecursiveKeyword;
      nodes = withDefs;
      foreach(var w in withDefs) {
        this.SetParent(w);
      }
    }

    public WithClause(bool hasRecursiveKeyword
                    , params WithDefinition[] withDefs)
      : this(hasRecursiveKeyword, new List<WithDefinition>()) {
      this.Comments = new Comments();
      this.AddRange(withDefs);
    }

    public WithClause(bool hasRecursiveKeyword
                    , IEnumerable<WithDefinition> withDefs)
      : this(hasRecursiveKeyword, new List<WithDefinition>()) {
      this.Comments = new Comments();
      this.AddRange(withDefs);
    }

    internal WithClause(bool hasRecursiveKeyword
                      , List<WithDefinition> withDefs
                      , Comments comments)
      : this(hasRecursiveKeyword, withDefs) {
      this.Comments = comments;
    }

    public bool HasRecursiveKeyword { get; set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      int offset = 1;
      if(this.HasRecursiveKeyword) {
        offset += 1;
      }
      nodes[0].Accept(visitor);
      int i;
      for(i = 1; i < nodes.Count; ++i) {
        visitor.VisitOnSeparator(this, offset, i - 1);
        nodes[i].Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }
}
