using System.Collections.Generic;

namespace MiniSqlParser
{
  public class Stmts: NodeCollections<Stmt>
  {
    private Stmts(List<Stmt> stmts) {
      prefixTerminalNodeCount = 0;
      suffiexTerminalNodeCount = 0;
      nodes = stmts;
      foreach(var s in stmts) {
        this.SetParent(s);
      }
    }

    public Stmts(params Stmt[] stmts)
      : this(new List<Stmt>()) {
      this.AddRange(stmts);
    }

    public Stmts(IEnumerable<Stmt> stmts)
      : this(new List<Stmt>()) {
      this.AddRange(stmts);
    }

    internal Stmts(List<Stmt> stmts, Comments comments)
      : this(stmts) {
      this.Comments = comments;
    }

    protected override void AcceptImp(IVisitor visitor) {
      foreach(var s in nodes) {
        s.Accept(visitor);
      }
    }
  }
}
