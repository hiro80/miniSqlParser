
namespace MiniSqlParser
{
  public class MergeStmt : Stmt
  {
    internal MergeStmt(WithClause with
                      , Table table
                      , Table usingTable
                      , AliasedQuery usingQuery
                      , Predicate constraint
                      , MergeUpdateClause updateClause
                      , MergeInsertClause insertClause
                      , bool updateBeforeInsert
                      , Comments comments) {
      this.Comments = comments;
      this.With = with;
      this.Table = table;
      this.UsingTable = usingTable;
      this.UsingQuery = usingQuery;
      this.Constraint = constraint;
      this.UpdateClause = updateClause;
      this.InsertClause = insertClause;
      this.UpdateBeforeInsert = updateBeforeInsert;
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

    private Table _usingTable;
    public Table UsingTable {
      get {
        return _usingTable;
      }
      private set {
        _usingTable = value;
        this.SetParent(value);
      }
    }

    private AliasedQuery _usingQuery;
    public AliasedQuery UsingQuery {
      get {
        return _usingQuery;
      }
      set {
        _usingQuery = value;
        this.SetParent(value);
      }
    }

    private Predicate _constraint;
    public Predicate Constraint {
      get {
        return _constraint;
      }
      set {
        _constraint = value;
        this.SetParent(value);
      }
    }

    private MergeUpdateClause _updateClause;
    public MergeUpdateClause UpdateClause {
      get {
        return _updateClause;
      }
      set {
        _updateClause = value;
        this.SetParent(value);
      }
    }

    private MergeInsertClause _insertClause;
    public MergeInsertClause InsertClause {
      get {
        return _insertClause;
      }
      set {
        _insertClause = value;
        this.SetParent(value);
      }
    }

    public bool UpdateBeforeInsert { get; set; }

    public bool HasWithClause {
      get {
        return this.With != null;
      }
    }

    public override StmtType Type {
      get {
        return StmtType.Merge;
      }
    }

    public bool HasUsingTable {
      get {
        return this.UsingTable != null;
      }
    }

    public bool HasUpdateClause {
      get {
        return this.UpdateClause != null;
      }
    }

    public bool HasInsertClause {
      get {
        return this.InsertClause != null;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.HasWithClause) {
        this.With.Accept(visitor);
      }

      visitor.VisitOnMerge(this);
      int offset = 2;
      this.Table.Accept(visitor);

      visitor.VisitOnUsing(this, offset);
      if(this.HasUsingTable) {
        offset += 1;
        this.UsingTable.Accept(visitor);
      } else {
        offset += 1;
        this.UsingQuery.Accept(visitor);
      }

      visitor.VisitOnOn(this, offset);
      offset += 1;
      //visitor.VisitOnLParen(this, offset);
      //offset += 1;
      this.Constraint.Accept(visitor);
      //visitor.VisitOnRParen(this, offset);
      //offset += 1;

      if(this.UpdateBeforeInsert) {
        this.UpdateClause.Accept(visitor);
        if(this.HasInsertClause) {
          this.InsertClause.Accept(visitor);
        }
      } else {
        if(this.HasInsertClause) {
          this.InsertClause.Accept(visitor);
          if(this.HasUpdateClause) {
            this.UpdateClause.Accept(visitor);
          }
        }
      }

      for(var j = 0; j < this.StmtSeparators; ++j) {
        visitor.VisitOnStmtSeparator(this, offset, j);
      }
      visitor.VisitAfter(this);
    }
  }
}
