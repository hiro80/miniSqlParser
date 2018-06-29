
namespace MiniSqlParser
{
  public class TruncateStmt: Stmt
  {

    private Table _table;
    public Table Table {
      get {
        return _table;
      }
      private set {
        _table = value;
        this.SetParent(value);
      }
    }

    public override StmtType Type {
      get {
        return StmtType.Truncate;
      }
    }

    public TruncateStmt(Table table, Comments comments) {
      this.Comments = comments;
      this.Table = table;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Table.Accept(visitor);
      for(var j = 0; j < this.StmtSeparators; ++j) {
        visitor.VisitOnStmtSeparator(this, 2, j);
      }
      visitor.VisitAfter(this);
    }
  }
}
