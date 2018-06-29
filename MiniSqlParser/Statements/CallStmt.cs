using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSqlParser
{
  public class CallStmt : Stmt
  {
    public override StmtType Type {
      get {
        return StmtType.Call;
      }
    }

    private Identifier _serverName;
    public Identifier ServerName {
      get {
        return _serverName;
      }
      set {
        this.CorrectComments(_serverName, value, 1);
        this.CorrectComments(_serverName, value, 1);
        _serverName = value;
      }
    }

    private Identifier _dataBaseName;
    public Identifier DataBaseName {
      get {
        return _dataBaseName;
      }
      set {
        this.CorrectComments(_dataBaseName, value, 1, !Identifier.IsNullOrEmpty(_serverName));
        this.CorrectComments(_dataBaseName, value, 1, !Identifier.IsNullOrEmpty(_serverName));
        _dataBaseName = value;
      }
    }

    private Identifier _schemaName;
    public Identifier SchemaName {
      get {
        return _schemaName;
      }
      set {
        this.CorrectComments(_schemaName, value, 1, !Identifier.IsNullOrEmpty(_serverName)
                                                  , !Identifier.IsNullOrEmpty(_dataBaseName));

        this.CorrectComments(_schemaName, value, 1, !Identifier.IsNullOrEmpty(_serverName)
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

    public CallStmt(Identifier name
                  , Exprs arguments)
      : this(null, null, null, name, arguments) {
    }

    public CallStmt(Identifier serverName
                  , Identifier databaseName
                  , Identifier schemaName
                  , Identifier name
                  , Exprs arguments) {
      this.Comments = new Comments(4);
      this.ServerName = serverName;
      this.DataBaseName = databaseName;
      this.SchemaName = schemaName;
      this.Name = name;
      this.Arguments = arguments;
    }

    internal CallStmt(Identifier serverName
                    , Identifier databaseName
                    , Identifier schemaName
                    , Identifier name
                    , Exprs arguments
                    , Comments comments) {
      this.Comments = comments;
      _serverName = serverName;
      _dataBaseName = databaseName;
      _schemaName = schemaName;
      _name = name;
      this.Arguments = arguments;
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
      
      visitor.VisitOnLParen(this, offset);
      offset += 1;
      if(this.Arguments != null) {
        this.Arguments.Accept(visitor);
      }
      visitor.VisitOnRParen(this, offset);
      offset += 1;

      for(var j = 0; j < this.StmtSeparators; ++j) {
        visitor.VisitOnStmtSeparator(this, offset, j);
      }
      visitor.VisitAfter(this);
    }
  }
}
