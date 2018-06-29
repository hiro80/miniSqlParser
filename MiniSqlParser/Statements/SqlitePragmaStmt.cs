
namespace MiniSqlParser
{
  public class SqlitePragmaStmt : Stmt
  {
    internal SqlitePragmaStmt(PlaceHolderExpr tableName
                            , Comments comments){
      this.Comments = comments;
      this.TableName = tableName;
    }

    internal SqlitePragmaStmt(Table table
                            , Comments comments) {
      this.Comments = comments;
      this.Table = table;
    }

    private PlaceHolderExpr _tableName;
    public PlaceHolderExpr TableName {
      get {
        return _tableName;
      }
      private set {
        _tableName = value;
        this.SetParent(value);
      }
    }

    private Table _table;
    public Table Table {
      get {
        return _table;
      }
      internal set {
        _table = value;
        this.SetParent(value);

        // コメントスロット数は変更なし
        this.TableName = null;
      }
    }

    public bool HasPlaceHolder {
      get {
        return this.TableName != null;
      }
    }

    public override StmtType Type {
      get {
        return StmtType.SqlitePragma;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      visitor.VisitOnLParen(this, 2);
      if(this.HasPlaceHolder) {
        this.TableName.Accept(visitor);
      } else {
        this.Table.Accept(visitor);
      }
      visitor.VisitOnRParen(this, 3);
      for(var j = 0; j < this.StmtSeparators; ++j) {
        visitor.VisitOnStmtSeparator(this, 4, j);
      }
      visitor.VisitAfter(this);
    }
  }
}
