
namespace MiniSqlParser
{
  public class UnqualifiedColumnName : Node
  {
    public UnqualifiedColumnName(Identifier name) {
      this.Comments = new Comments(1);
      this.Name = name;
    }

    internal UnqualifiedColumnName(Identifier name, Comments comments) {
      this.Name = name;
      this.Comments = comments;
    }

    public Identifier Name { get; set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
