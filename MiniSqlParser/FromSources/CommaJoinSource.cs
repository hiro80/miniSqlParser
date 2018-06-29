
namespace MiniSqlParser
{
  public class CommaJoinSource: Node, IFromSource
  {
    internal CommaJoinSource(IFromSource left
                            , IFromSource right
                            , Comments comments) {
      this.Comments = comments;
      this.Left = left;
      this.Right = right;
    }

    private IFromSource _left;
    public IFromSource Left {
      get {
        return _left;
      }
      private set {
        _left = value;
        this.SetParent(value);
      }
    }

    private IFromSource _right;
    public IFromSource Right {
      get {
        return _right;
      }
      private set {
        _right = value;
        this.SetParent(value);
      }
    }

    public FromSourceType Type {
      get {
        return FromSourceType.CommaJoin;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Left.Accept(visitor);
      visitor.VisitOnSeparator(this, 0, 0);
      this.Right.Accept(visitor);
      visitor.VisitAfter(this);
    }
  }
}
