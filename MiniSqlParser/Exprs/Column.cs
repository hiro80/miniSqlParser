namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("Name: {Name}")]
  public class Column : Expr
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
        this.CorrectComments(_tableAliasName, value, 0, !Identifier.IsNullOrEmpty(_serverName)
                                                       , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                       , !Identifier.IsNullOrEmpty(_schemaName));
        this.CorrectComments(_tableAliasName, value, 0, !Identifier.IsNullOrEmpty(_serverName)
                                                       , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                       , !Identifier.IsNullOrEmpty(_schemaName));
        _tableAliasName = value;
      }
    }

    private Identifier _name;
    public Identifier Name {
      get {
        return _name;
      }
      set {
        if(Identifier.IsNullOrEmpty(value)) {
          throw new System.ArgumentNullException("");
        }
        _name = value;
      }
    }

    public bool HasOuterJoinKeyword { get; internal set; }

    public Column(Identifier name)
      : this(null, null, null, null, name, false) {
    }

    public Column(Identifier tableAliasName
                , Identifier name)
      : this(null, null, null, tableAliasName, name, false) {
    }

    public Column(Identifier serverName
                , Identifier databaseName
                , Identifier schemaName
                , Identifier tableAliasName
                , Identifier name
                , bool hasOuterJoinKeyword) {
      this.Comments = new Comments(1);
      this.ServerName = serverName;
      this.DataBaseName = databaseName;
      this.SchemaName = schemaName;
      this.TableAliasName = tableAliasName;
      this.Name = name;
      this.HasOuterJoinKeyword = hasOuterJoinKeyword;
    }

    internal Column(Identifier serverName
                  , Identifier databaseName
                  , Identifier schemaName
                  , Identifier tableAliasName
                  , Identifier name
                  , bool hasOuterJoinKeyword
                  , Comments comments) {
      this.Comments = comments;
      _serverName = serverName;
      _dataBaseName = databaseName;
      _schemaName = schemaName;
      _tableAliasName = tableAliasName;
      _name = name;
      this.HasOuterJoinKeyword = hasOuterJoinKeyword;
    }

    public override IValue Clone() {
      return new Column(_serverName
                      , _dataBaseName
                      , _schemaName
                      , _tableAliasName
                      , _name
                      , this.HasOuterJoinKeyword
                      , this.Comments.Clone());
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
