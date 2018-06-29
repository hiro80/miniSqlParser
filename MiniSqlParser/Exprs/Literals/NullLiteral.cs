namespace MiniSqlParser
{
  public class NullLiteral : Literal
  {
    public NullLiteral() {
      this.Comments = new Comments(1);
      this.Value = "Null";
    }

    internal NullLiteral(Comments comments) {
      this.Comments = comments;
      this.Value = "Null";
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }

    public static bool operator ==(NullLiteral lLiteral, NullLiteral rLiteral) {
      // NULL同士の比較はFALSEである
      return false;
    }

    public static bool operator !=(NullLiteral lLiteral, NullLiteral rLiteral) {
      return false;
    }
  }
}
