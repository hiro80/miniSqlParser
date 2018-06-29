
namespace MiniSqlParser
{
  public class NullStmt : Stmt
  {
    internal NullStmt(int stmtSeparatorCount, Comments comments) {
      this.StmtSeparators = stmtSeparatorCount;
      this.Comments = comments;
    }

    public override StmtType Type {
      get {
        return StmtType.Null;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      for(var i = 0; i < this.StmtSeparators; ++i) {
        visitor.VisitOnStmtSeparator(this, 0, i);
      }
      visitor.VisitAfter(this);
    }
  }
}
