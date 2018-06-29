namespace MiniSqlParser
{
  public class BlobLiteral : Literal
  {
    public BlobLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value;
    }

    internal BlobLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value;
    }

    public string Unquote() {
      return this.Value.Substring(2, this.Value.Length - 3);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }

    public static bool operator ==(BlobLiteral lLiteral, BlobLiteral rLiteral) {
      // 大文字と小文字を区別せずに比較する
      return string.Compare(lLiteral.Unquote(), rLiteral.Unquote(), true) == 0;
    }

    public static bool operator !=(BlobLiteral lLiteral, BlobLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }
  }
}
