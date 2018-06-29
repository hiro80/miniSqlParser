
namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("AliasName: {AliasName}")]
  public class ResultExpr : ResultColumn
  {
    public ResultExpr(Expr value)
      : this(value, false, null) {
    }

    public ResultExpr(Expr value, bool hasAs, Identifier aliasName) {
      this.Comments = new Comments();
      this.Value = value;
      this.HasAs = hasAs;
      this.AliasName = aliasName;
    }

    internal ResultExpr(Expr value, bool hasAs, Identifier aliasName, Comments comments) {
      this.Comments = comments;
      this.Value = value;
      _hasAs = hasAs;
      _aliasName = aliasName;
    }

    private Expr _value;
    public Expr Value {
      get {
        return _value;
      }
      set {
        _value = value;
        this.SetParent(value);
      }
    }

    private bool _hasAs;
    public bool HasAs {
      get {
        return _hasAs;
      }
      set {
        this.CorrectComments(_hasAs, value, 0);
        _hasAs = value;
      }
    }

    private Identifier _aliasName;
    public Identifier AliasName {
      get {
        return _aliasName;
      }
      set {
        this.CorrectComments(_aliasName, value, 0, this.HasAs);
        _aliasName = value;
      }
    }

    public Identifier GetAliasOrColumnName() {
      if(!Identifier.IsNullOrEmpty(_aliasName)) {
        return _aliasName;
      } else {
        // 指定したSELECT句が括弧で包まれている場合は、括弧を剥く
        var value = _value;
        while(value.GetType() == typeof(BracketedExpr)) {
          value = ((BracketedExpr)value).Operand;
        }
        if(value.GetType() == typeof(Column)) {
          return ((Column)value).Name;
        } else {
          return null;
        }
      }
    }

    public override bool IsTableWildcard {
      get {
        return false;
      }
    }

    public override ResultColumn Clone() {
      var ret = new ResultExpr((Expr)_value.Clone(), _hasAs, _aliasName, this.Comments.Clone());
      ret.Attachment = this.Attachment;
      return ret;
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Value.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
