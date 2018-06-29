
namespace MiniSqlParser
{
  public class TableWildcard: ResultColumn
  {
    private Identifier _serverName;
    public Identifier ServerName {
      get {
        return _serverName;
      }
      set {
        this.CorrectComments(_serverName, value, 0);
        this.CorrectComments(_serverName, value, 0);
        _serverName = value;
      }
    }

    private Identifier _dataBaseName;
    public Identifier DataBaseName {
      get {
        return _dataBaseName;
      }
      set {
        this.CorrectComments(_dataBaseName, value, 0, !Identifier.IsNullOrEmpty(_serverName));
        this.CorrectComments(_dataBaseName, value, 0, !Identifier.IsNullOrEmpty(_serverName));
        _dataBaseName = value;
      }
    }

    private Identifier _schemaName;
    public Identifier SchemaName {
      get {
        return _schemaName;
      }
      set {
        this.CorrectComments(_schemaName, value, 0, !Identifier.IsNullOrEmpty(_serverName)
                                                  , !Identifier.IsNullOrEmpty(_dataBaseName));

        this.CorrectComments(_schemaName, value, 0, !Identifier.IsNullOrEmpty(_serverName)
                                                  , !Identifier.IsNullOrEmpty(_dataBaseName));
        _schemaName = value;
      }
    }

    private Identifier _tableAliasName;
    public Identifier TableAliasName {
      get {
        return _tableAliasName;
      }
      set {
        if(Identifier.IsNullOrEmpty(value)) {
          throw new System.ArgumentNullException("");
        }
        this.CorrectComments(_tableAliasName, value, 0, !Identifier.IsNullOrEmpty(_serverName)
                                                       , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                       , !Identifier.IsNullOrEmpty(_schemaName));
        this.CorrectComments(_tableAliasName, value, 0, !Identifier.IsNullOrEmpty(_serverName)
                                                       , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                       , !Identifier.IsNullOrEmpty(_schemaName));
        _tableAliasName = value;
      }
    }

    public override bool IsTableWildcard {
      get {
        return true;
      }
    }

    public TableWildcard(Identifier tableAliasName)
      : this(null, null, null, tableAliasName) {
    }

    public TableWildcard(Identifier serverName
                       , Identifier databaseName
                       , Identifier schemaName
                       , Identifier tableAliasName) {
      this.Comments = new Comments(1);
      this.ServerName = serverName;
      this.DataBaseName = databaseName;
      this.SchemaName = schemaName;
      this.TableAliasName = tableAliasName;
    }

    internal TableWildcard(Identifier serverName
                         , Identifier databaseName
                         , Identifier schemaName
                         , Identifier tableName
                         , Comments comments) {
      this.Comments = comments;
      _serverName = serverName;
      _dataBaseName = databaseName;
      _schemaName = schemaName;
      _tableAliasName = tableName;
    }

    public override ResultColumn Clone() {
      var ret = new TableWildcard(_serverName
                             , _dataBaseName
                             , _schemaName
                             , _tableAliasName
                             , this.Comments.Clone());
      ret.Attachment = this.Attachment;
      return ret;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
