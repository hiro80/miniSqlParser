using System.Collections.Generic;

namespace MiniSqlParser
{
  public class Exprs: NodeCollections<Expr>
  {
    private Exprs(List<Expr> exprs) {
      prefixTerminalNodeCount = 0;
      suffiexTerminalNodeCount = 0;
      nodes = exprs;
      foreach(var e in exprs) {
        this.SetParent(e);
      }
    }

    public Exprs(params Expr[] exprs)
      : this(new List<Expr>()) {
      this.Comments = new Comments();
      this.AddRange(exprs);
    }

    public Exprs(IEnumerable<Expr> exprs)
      : this(new List<Expr>()) {
      this.Comments = new Comments();
      this.AddRange(exprs);
    }

    internal Exprs(List<Expr> exprs, Comments comments)
      : this(exprs) {
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
