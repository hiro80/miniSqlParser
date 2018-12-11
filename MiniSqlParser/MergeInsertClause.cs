
namespace MiniSqlParser
{
  public class MergeInsertClause : Node
  {
    private ColumnNames _columns;
    public ColumnNames Columns {
      get {
        return _columns;
      }
      set {
        _columns = value;
        this.SetParent(value);
      }
    }

    private Values _values;
    public Values Values {
      get {
        return _values;
      }
      private set {
        _values = value;
        this.SetParent(value);
      }
    }

    public bool HasTableColumns {
      get {
        return this.Columns != null;
      }
    }

    internal MergeInsertClause(ColumnNames columns
                              , Values values
                              , Comments comments) {
      this.Comments = comments;
      this.Columns = columns;
      this.Values = values;
    }

    public Assignments GetAssignments() {
      //  テーブル列名の指定がない場合は、空のAssignmentsを返す
      if(this.Columns == null || this.Columns.Count == 0) {
        return new Assignments();
      }

      // テーブル列名とVALUES句の要素数が異なる場合はエラーとする
      if(this.Columns.Count != this.Values.Count) {
        throw new InvalidASTStructureError("テーブル列名とVALUES句の要素数が異なります");
      }

      var ret = new Assignments();
      for(var i = 0; i < this.Columns.Count; ++i) {
        var column = new Column(this.Columns[i].Name);
        var assignment = new Assignment(column, this.Values[i]);
        ret.Add(assignment);
      }

      return ret;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.Columns != null) {
        this.Columns.Accept(visitor);
      }
      visitor.VisitOnValues(this, 5);
      this.Values.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
