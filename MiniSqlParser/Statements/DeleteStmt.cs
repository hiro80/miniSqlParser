
namespace MiniSqlParser
{
  public class DeleteStmt : Stmt
  {
    internal DeleteStmt(WithClause with
                      , bool hasFromKeyword
                      , Table table
                      , Table table2
                      , Predicate where
                      , Comments comments) {
      this.Comments = comments;
      this.With = with;
      this.HasFromKeyword = hasFromKeyword;
      this.Table = table;
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
    
    public bool HasFromKeyword { get; private set; }

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

    private Predicate _where;
    public Predicate Where {
      get {
        return _where;
      }
      set {
        this.CorrectComments(_where, value, 1, this.HasFromKeyword);
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
        return StmtType.Delete;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.HasWithClause) {
        this.With.Accept(visitor);
      }
      visitor.VisitOnDelete(this);
      int offset = this.HasFromKeyword ? 2 : 1;
      this.Table.Accept(visitor);
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
