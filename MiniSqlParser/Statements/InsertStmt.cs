
namespace MiniSqlParser
{ 
  public abstract class InsertStmt : Stmt
  {
    private WithClause _with;
    virtual public WithClause With {
      get {
        return _with;
      }
      set {
        _with = value;
        this.SetParent(value);
      }
    }

    virtual public ConflictType OnConflict { get; protected set; }
    virtual public bool HasIntoKeyword { get; protected set; }

    private Table _table;
    virtual public Table Table {
      get {
        return _table;
      }
      set {
        _table = value;
        this.SetParent(value);
      }
    }

    private UnqualifiedColumnNames _columns;
    virtual public UnqualifiedColumnNames Columns {
      get {
        return _columns;
      }
      set {
        _columns = value;
        this.SetParent(value);
      }
    }

    virtual public bool HasWithClause {
      get {
        return this.With != null;
      }
    }

    virtual public bool HasTableColumns {
      get {
        return this.Columns != null;
      }
    }

    abstract public Assignments GetAssignments(int index);
  }
}
