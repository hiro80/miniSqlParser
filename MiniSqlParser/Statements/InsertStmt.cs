
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

    virtual public bool IsReplaceStmt { get; protected set; }
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

    private UnqualifiedColumnNames _conflictColumns;
    virtual public UnqualifiedColumnNames ConflictColumns {
      get {
        return _conflictColumns;
      }
      set {
        _conflictColumns = value;
        this.SetParent(value);
      }
    }

    virtual public string ConstraintName { get; protected set; }

    private Assignments _updateaAsignments;
    virtual public Assignments UpdateAssignments {
      get {
        return _updateaAsignments;
      }
      protected set {
        _updateaAsignments = value;
        this.SetParent(value);
      }
    }

    private Predicate _updateWhere;
    public Predicate UpdateWhere {
      get {
        return _updateWhere;
      }
      protected set {
        _updateWhere = value;
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

    virtual public bool IsPostgreSqlUpsert {
      get {
        return (_conflictColumns != null && _conflictColumns.Count > 0)
               || !string.IsNullOrEmpty(ConstraintName);
      }
    }
  }
}
