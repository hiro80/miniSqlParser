
namespace MiniSqlParser
{
  public class WindowFuncExpr : Expr
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

    private QuantifierType _quantifier;
    public QuantifierType Quantifier {
      get {
        return _quantifier;
      }
      set {
        this.CorrectComments2(_quantifier, value, 2, !Identifier.IsNullOrEmpty(_serverName)
                                                   , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                   , !Identifier.IsNullOrEmpty(_schemaName));
        _quantifier = value;
      }
    }

    private bool _hasWildcard;
    public bool HasWildcard {
      get {
        return _hasWildcard;
      }
      set {
        this.CorrectComments(_hasWildcard, value, 2, !Identifier.IsNullOrEmpty(_serverName)
                                                   , !Identifier.IsNullOrEmpty(_dataBaseName)
                                                   , !Identifier.IsNullOrEmpty(_schemaName)
                                                   , this.Quantifier != QuantifierType.None);
        _hasWildcard = value;
      }
    }

    private Exprs _arguments;
    public Exprs Arguments {
      get {
        return _arguments;
      }
      private set {
        _arguments = value;
        this.SetParent(value);
      }
    }

    private PartitionBy _partitionBy;
    public PartitionBy PartitionBy {
      get {
        return _partitionBy;
      }
      private set {
        _partitionBy = value;
        this.SetParent(value);
      }
    }

    private OrderBy _orderBy;
    public OrderBy OrderBy {
      get {
        return _orderBy;
      }
      private set {
        _orderBy = value;
        this.SetParent(value);
      }
    }

    public WindowFuncExpr(Identifier serverName
                        , Identifier databaseName
                        , Identifier schemaName
                        , Identifier name
                        , Exprs arguments
                        , PartitionBy partitionBy
                        , OrderBy orderBy) {
      this.Comments = new Comments(6);
      this.ServerName = serverName;
      this.DataBaseName = databaseName;
      this.SchemaName = schemaName;
      this.Name = name;
      this.Quantifier = QuantifierType.None;
      this.HasWildcard = false;
      this.Arguments = arguments;
      this.PartitionBy = partitionBy;
      this.OrderBy = orderBy;
      //
      if(this.PartitionBy != null) this.PartitionBy.Parent = this;
      if(this.OrderBy != null)this.OrderBy.Parent = this;
    }

    public WindowFuncExpr(Identifier serverName
                        , Identifier databaseName
                        , Identifier schemaName
                        , Identifier name
                        , QuantifierType quantifier
                        , PartitionBy partitionBy
                        , OrderBy orderBy) {
      this.Comments = new Comments(7 + quantifier != QuantifierType.None? 0 : 1);
      this.ServerName = serverName;
      this.DataBaseName = databaseName;
      this.SchemaName = schemaName;
      this.Name = name;
      this.Quantifier = quantifier;
      this.HasWildcard = true;
      this.Arguments = new Exprs();
      this.PartitionBy = partitionBy;
      this.OrderBy = orderBy;
      //
      if(this.PartitionBy != null) this.PartitionBy.Parent = this;
      if(this.OrderBy != null)this.OrderBy.Parent = this;
    }

    internal WindowFuncExpr(Identifier serverName
                          , Identifier databaseName
                          , Identifier schemaName
                          , Identifier name
                          , QuantifierType quantifier
                          , bool hasWildcard
                          , Exprs arguments
                          , PartitionBy partitionBy
                          , OrderBy orderBy
                          , Comments comments) {
      this.Comments = comments;
      _serverName = serverName;
      _dataBaseName = databaseName;
      _schemaName = schemaName;
      _name = name;
      _quantifier = quantifier;
      _hasWildcard = hasWildcard;
      this.Arguments = arguments;
      this.PartitionBy = partitionBy;
      this.OrderBy = orderBy;
      //
      if(this.PartitionBy != null) this.PartitionBy.Parent = this;
      if(this.OrderBy != null)this.OrderBy.Parent = this;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      var offset = 2;
      if(!Identifier.IsNullOrEmpty(this.ServerName)) {
        offset += 2;
      }
      if(!Identifier.IsNullOrEmpty(this.DataBaseName)) {
        offset += 2;
      }
      if(!Identifier.IsNullOrEmpty(this.SchemaName)) {
        offset += 2;
      }
      if(this.Quantifier != QuantifierType.None) {
        offset += 1;
      }
      if(this.HasWildcard) {
        visitor.VisitOnWildCard(this, offset);
        offset += 1;
      } else if(this.Arguments != null){
        this.Arguments.Accept(visitor);
      } 
      // VisitOnOver()にはOVER句でのoffset値を渡すため、
      // 閉じカッコのoffsetを加算する
      offset += 1;
      visitor.VisitOnOver(this, offset);
      if(this.PartitionBy != null) {
        this.PartitionBy.Accept(visitor);
      }
      this.OrderBy.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
