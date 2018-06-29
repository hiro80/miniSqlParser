
namespace MiniSqlParser
{
  public class WithDefinition : Node
  {
    public WithDefinition(Table table
                        , UnqualifiedColumnNames columns
                        , IQuery query) {
      this.Comments = new Comments(3);
      this.Table = table;
      this.Columns = columns;
      this.Query = query;
    }

    internal WithDefinition(Table table
                          , UnqualifiedColumnNames columns
                          , IQuery query
                          , Comments comments) {
      this.Comments = comments;
      this.Table = table;
      this.Columns = columns;
      this.Query = query;
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

    private UnqualifiedColumnNames _columns;
    public UnqualifiedColumnNames Columns {
      get {
        return _columns;
      }
      set {
        _columns = value;
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

    public bool HasTableColumnDefinition {
      get {
        return this.Columns != null
            && this.Columns.Count > 0;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Table.Accept(visitor);
      int offset = 0;
      if(HasTableColumnDefinition) {
        this.Columns.Accept(visitor);
      }
      visitor.VisitOnAs(this, offset);
      offset += 1;
      this.Query.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
