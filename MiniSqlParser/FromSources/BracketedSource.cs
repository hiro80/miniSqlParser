
namespace MiniSqlParser
{
  public class BracketedSource : Node, IFromSource
  {
    public BracketedSource(IFromSource operand
                          , bool hasAs
                          , Identifier aliasName) {
      this.Comments = new Comments(2);
      this.Operand = operand;
      this.HasAs = hasAs;
      this.AliasName = aliasName;
    }

    internal BracketedSource(IFromSource operand
                            , bool hasAs
                            , Identifier aliasName
                            , Comments comments) {
      this.Comments = comments;
      this.Operand = operand;
      _hasAs = hasAs;
      _aliasName = aliasName;
    }

    private IFromSource _operand;
    public IFromSource Operand {
      get {
        return _operand;
      }
      set {
        _operand = value;
        this.SetParent(value);
      }
    }

    private bool _hasAs;
    public bool HasAs {
      get {
        return _hasAs;
      }
      set {
        this.CorrectComments(_hasAs, value, 2);
        _hasAs = value;
      }
    }

    private Identifier _aliasName;
    public Identifier AliasName {
      get {
        return _aliasName;
      }
      set {
        this.CorrectComments(_aliasName, value, 2, this.HasAs);
        _aliasName = value;
      }
    }

    public FromSourceType Type {
      get {
        return FromSourceType.Bracketed;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Operand.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
