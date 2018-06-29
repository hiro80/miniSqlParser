
namespace MiniSqlParser
{
  public class SelectStmt : Stmt
  {
    public SelectStmt(WithClause with, IQuery query, ForUpdateClause forUpdate) {
      this.With = with;
      this.Query = query;
      this.ForUpdate = forUpdate;
      this.Comments = new Comments();
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

    private IQuery _query;
    public IQuery Query {
      get {
        return _query;
      }
      set {
        _query = value;
        this.SetParent(value);
      }
    }

    private ForUpdateClause _forUpdate;
    public ForUpdateClause ForUpdate {
      get {
        return _forUpdate;
      }
      set {
        _forUpdate = value;
        this.SetParent(value);
      }
    }

    public bool HasWithClause {
      get {
        return this.With != null;
      }
    }

    public bool HasForUpdate {
      get {
        return this.ForUpdate != null;
      }
    }

    public override StmtType Type {
      get {
        return StmtType.Select;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.HasWithClause) {
        this.With.Accept(visitor);
      }
      this.Query.Accept(visitor);
      if(this.HasForUpdate) {
        this.ForUpdate.Accept(visitor);
      }
      for(var i = 0; i < this.StmtSeparators; ++i) {
        visitor.VisitOnStmtSeparator(this, 0, i);
      }
      visitor.VisitAfter(this);
    }
  }
}
