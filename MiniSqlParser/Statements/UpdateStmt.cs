
namespace MiniSqlParser
{
  public class UpdateStmt : Stmt
  {
    internal UpdateStmt(WithClause with
                      , ConflictType onConflict
                      , Table table
                      , Assignments assignments
                      , Table table2
                      , Predicate where
                      , Comments comments) {
      this.Comments = comments;
      this.With = with;
      this.OnConflict = onConflict;
      this.Table = table;
      this.Assignments = assignments;
      this.Table2 = table2;
      _where = where;
      this.SetParent(where);
    }

    private WithClause _with;
    public WithClause With {
      get {
        return _with;
      }
      set {
        _with = value;
        this.SetParent(value);
      }
    }
    
    public ConflictType OnConflict { get; private set; }

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

    private Table _table2;
    public Table Table2 {
      get {
        return _table2;
      }
      private set {
        _table2 = value;
        this.SetParent(value);
      }
    }

    private Assignments _assignments;
    public  Assignments Assignments {
      get {
        return _assignments;
      }
      private set {
        _assignments = value;
        this.SetParent(value);
      }
    }

    private Predicate _where;
    public Predicate Where {
      get {
        return _where;
      }
      set {
        this.CorrectComments(_where, value, 2, this.OnConflict != ConflictType.None);
        _where = value;
        this.SetParent(value);
      }
    }

    public bool HasWithClause {
      get {
        return this.With != null;
      }
    }

    public override StmtType Type {
      get {
        return StmtType.Update;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.HasWithClause) {
        this.With.Accept(visitor);
      }
      visitor.VisitOnUpdate(this);
      int offset = 1;
      offset += this.OnConflict != ConflictType.None ? 2 : 0;
      this.Table.Accept(visitor);
      visitor.VisitOnSet(this, offset);
      offset += 1;
      this.Assignments.Accept(visitor);
      if(this.Table2 != null) {
        visitor.VisitOnFrom2(this, offset);
        offset += 1;
        this.Table2.Accept(visitor);
      }
      if(this.Where != null) {
        visitor.VisitOnWhere(this, offset);
        offset += 1;
        this.Where.Accept(visitor);
      }
      for(var j = 0; j < this.StmtSeparators; ++j) {
        visitor.VisitOnStmtSeparator(this, offset, j);
      }
      visitor.VisitAfter(this);
    }
  }
}
