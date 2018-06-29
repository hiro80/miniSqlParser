using System.Collections.Generic;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("Name: {Name}")]
  public class Table : Node, IFromSource
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

    private bool _hasAs;
    public bool HasAs {
      get {
        return _hasAs;
      }
      set {
        this.CorrectComments(_hasAs, value, 1, !Identifier.IsNullOrEmpty(_serverName)
                                              , !Identifier.IsNullOrEmpty(_dataBaseName)
                                              , !Identifier.IsNullOrEmpty(_schemaName));
        this.CorrectComments(_hasAs, value, 1, !Identifier.IsNullOrEmpty(_serverName)
                                              , !Identifier.IsNullOrEmpty(_dataBaseName)
                                              , !Identifier.IsNullOrEmpty(_schemaName));
        _hasAs = value;
      }
    }

    private Identifier _aliasName;
    public Identifier AliasName {
      get {
        return _aliasName;
      }
      set {
        this.CorrectComments(_aliasName, value, 1, !Identifier.IsNullOrEmpty(_serverName)
                                                  , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                  , !Identifier.IsNullOrEmpty(_schemaName)
                                                  , _hasAs);
        this.CorrectComments(_aliasName, value, 1, !Identifier.IsNullOrEmpty(_serverName)
                                                  , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                  , !Identifier.IsNullOrEmpty(_schemaName));
        _aliasName = value;
      }
    }

    /// <summary>
    /// コメント別名を候補に含めないで別名を取得する
    /// </summary>
    /// <returns></returns>
    public Identifier GetAliasOrTableName() {
      if(!Identifier.IsNullOrEmpty(_aliasName)) {
        return _aliasName;
      } else {
        return _name;
      }
    }

    /// <summary>
    /// コメント別名も候補に含めて別名を取得する
    /// </summary>
    /// <returns></returns>
    public Identifier GetAliasOrTableName2() {
      if(!Identifier.IsNullOrEmpty(_aliasName)) {
        return _aliasName;
      } else if(!Identifier.IsNullOrEmpty(this.ImplicitAliasName)) {
        return this.ImplicitAliasName;
      } else {
        return _name;
      }
    }

    public FromSourceType Type {
      get {
        return FromSourceType.Table;
      }
    }

    // Documentation Commentで指定されたテーブル列名
    public string ImplicitAliasName { get; internal set; }

    public Identifier IndexServerName { get; private set; }
    public Identifier IndexDataBaseName { get; private set; }
    public Identifier IndexSchemaName { get; private set; }
    public Identifier IndexName { get; private set; }
    public bool HasNotIndexed { get; private set; }

    public Table Clone() {
      var ret = new Table(_serverName
                        , _dataBaseName
                        , _schemaName
                        , _name
                        , _hasAs
                        , _aliasName
                        , this.ImplicitAliasName
                        , this.IndexServerName
                        , this.IndexDataBaseName
                        , this.IndexSchemaName
                        , this.IndexName
                        , this.HasNotIndexed
                        , this.Comments.Clone());
      ret.Attachment = this.Attachment;
      return ret;
    }

    internal Table(Identifier serverName
                 , Identifier databaseName
                 , Identifier schemaName
                 , Identifier name
                 , Comments comments)
      : this(serverName
           , databaseName
           , schemaName
           , name
           , false
           , null
           , null
           , null
           , null
           , null
           , null
           , false,comments){
    }

    internal Table(Identifier serverName
                 , Identifier databaseName
                 , Identifier schemaName
                 , Identifier name
                 , bool hasAs
                 , Identifier aliasName
                 , string implicitAliasName
                 , Comments comments)
      : this(serverName
            , databaseName
            , schemaName
            , name
            , hasAs
            , aliasName
            , implicitAliasName
            , null
            , null
            , null
            , null
            , false
            , comments) {
    }

    internal Table(Identifier serverName
                  , Identifier databaseName
                  , Identifier schemaName
                  , Identifier name
                  , bool hasAs
                  , Identifier aliasName
                  , string implicitAliasName
                  , Identifier indexServerName
                  , Identifier indexDatabaseName
                  , Identifier indexSchemaName
                  , Identifier indexName
                  , bool hasNotIndexed
                  , Comments comments) {
      _serverName = serverName;
      _dataBaseName = databaseName;
      _schemaName = schemaName;
      _name = name;
      _hasAs = hasAs;
      _aliasName = aliasName;
      this.ImplicitAliasName = implicitAliasName;
      this.IndexServerName = indexServerName;
      this.IndexDataBaseName = indexDatabaseName;
      this.IndexSchemaName = indexSchemaName;
      this.IndexName = indexName;
      this.HasNotIndexed = hasNotIndexed;
      this.Comments = comments;
    }

    public Table(Identifier name)
      : this(null
            , null
            , null
            , name
            , false
            , null
            , null
            , null
            , null
            , null
            , false) {
    }

    public Table(Identifier name
               , bool hasAs
               , Identifier aliasName)
      : this(null
            , null
            , null
            , name
            , hasAs
            , aliasName
            , null
            , null
            , null
            , null
            , false) {
    }

    public Table(Identifier schemaName
               , Identifier name)
      : this(null
            , null
            , schemaName
            , name
            , false
            , null
            , null
            , null
            , null
            , null
            , false) {
    }

    public Table(Identifier serverName
               , Identifier databaseName
               , Identifier schemaName
               , Identifier name)
      : this(serverName
            , databaseName
            , schemaName
            , name
            , false
            , null
            , null
            , null
            , null
            , null
            , false) {
    }

    public Table(Identifier serverName
               , Identifier databaseName
               , Identifier schemaName
               , Identifier name
               , bool hasAs
               , Identifier aliasName)
      : this(serverName
            , databaseName
            , schemaName
            , name
            , hasAs
            , aliasName
            , null
            , null
            , null
            , null
            , false) {
    }

    public Table(Identifier serverName
                , Identifier databaseName
                , Identifier schemaName
                , Identifier name
                , bool hasAs
                , Identifier aliasName
                , Identifier indexServerName
                , Identifier indexDatabaseName
                , Identifier indexSchemaName
                , Identifier indexName
                , bool hasNotIndexed) {
      _serverName = serverName;
      _dataBaseName = databaseName;
      _schemaName = schemaName;
      _name = name;
      _hasAs = hasAs;
      _aliasName = aliasName;
      this.IndexServerName = indexServerName;
      this.IndexDataBaseName = indexDatabaseName;
      this.IndexSchemaName = indexSchemaName;
      this.IndexName = indexName;
      this.HasNotIndexed = hasNotIndexed;

      // コメントスロット数を計算する
      var n = CountTrue(!string.IsNullOrEmpty(indexName)) * 3;
      var m = CountTrue(!string.IsNullOrEmpty(serverName)
                      , !string.IsNullOrEmpty(databaseName)
                      , !string.IsNullOrEmpty(schemaName)
                      , hasNotIndexed
                      , !string.IsNullOrEmpty(indexServerName)
                      , !string.IsNullOrEmpty(indexDatabaseName)
                      , !string.IsNullOrEmpty(indexSchemaName)) * 2;
      var l = CountTrue(HasAs
                      , !string.IsNullOrEmpty(aliasName));

      this.Comments = new Comments(n + m + l + 1);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
