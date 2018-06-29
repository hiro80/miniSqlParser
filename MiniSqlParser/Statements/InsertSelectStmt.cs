
namespace MiniSqlParser
{
  public class InsertSelectStmt: InsertStmt
  {
    internal InsertSelectStmt(WithClause with
                            , ConflictType onConflict
                            , bool hasIntoKeyword
                            , Table table
                            , UnqualifiedColumnNames columns
                            , IQuery query
                            , Comments comments) {
      this.Comments = comments;
      this.With = with;
      this.OnConflict = onConflict;
      this.HasIntoKeyword = hasIntoKeyword;
      this.Table = table;
      this.Columns = columns;
      this.Query = query;
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

    public override StmtType Type {
      get {
        return StmtType.InsertSelect;
      }
    }

    public override Assignments GetAssignments(int index) {
      //  テーブル列名の指定がない場合は、空のAssignmentsを返す
      if(this.Columns == null || this.Columns.Count == 0) {
        return new Assignments();
      }

      // Compound Queryの場合は空のAssignmentsを返す
      SingleQueryClause singleQuery = null;
      if(this.Query.Type == QueryType.Compound){
        return new Assignments();
      }else if(this.Query.Type == QueryType.Bracketed){
        IQueryClause query = ((BracketedQuery)this.Query).Operand;
        // 括弧を剥ぐ
        while(query.Type == QueryType.Bracketed){
          query = ((BracketedQueryClause)query).Operand;
        }
        if(query.Type != QueryType.Single){
          return new Assignments();
        }
        singleQuery = (SingleQueryClause)query;
      } else {
        singleQuery = (SingleQuery)this.Query;
      }
      
      // SELECT *の場合は空のAssignmentsを返す
      if(singleQuery.HasWildcard) {
        return new Assignments();
      }

      // テーブル列名とSELECT句の要素数が異なる場合はエラーとする
      if(this.Columns.Count != singleQuery.Results.Count) {
        throw new InvalidASTStructureError("テーブル列名とSELECT句の要素数が異なります");
      }

      var ret = new Assignments();
      for(var i = 0; i < Columns.Count; ++i) {
        var column = new Column(this.Columns[i].Name);
        var result = singleQuery.Results[i];
        // SELECT句にTable WildCardが含まれる場合は、空のAssignmentsを返す
        if(result.IsTableWildcard) {
          return new Assignments();
        }
        var value = ((ResultExpr)result).Value;
        var assignment = new Assignment(column, value);
        ret.Add(assignment);
      }

      return ret;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);

      if(this.HasWithClause) {
        this.With.Accept(visitor);
      }
      visitor.VisitOnInsert(this);
      int offset = this.HasIntoKeyword ? 2 : 1;
      offset += this.OnConflict != ConflictType.None ? 2 : 0;

      this.Table.Accept(visitor);

      // Columnリストの走査
      if(this.HasTableColumns) {
        this.Columns.Accept(visitor);
      }

      visitor.VisitBeforeQuery(this, offset);

      this.Query.Accept(visitor);

      for(var i = 0; i < this.StmtSeparators; ++i) {
        visitor.VisitOnStmtSeparator(this, offset, i);
      }

      visitor.VisitAfter(this);
    }
  }
}
