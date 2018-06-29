
namespace MiniSqlParser
{
  public class Default : Node, IValue
  {
    public Default() {
      this.Comments = new Comments(1);
    }

    internal Default(Comments comments) {
      this.Comments = comments;
    }

    public IValue Clone() {
      return new Default(this.Comments.Clone());
    }

    public bool IsDefault {
      get {
        return true;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
