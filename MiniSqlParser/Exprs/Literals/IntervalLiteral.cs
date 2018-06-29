using System.Collections.Generic;

namespace MiniSqlParser
{
  public class IntervalLiteral : Literal
  {
    public IntervalLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value.Trim();
    }

    internal IntervalLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value.Trim();
    }

    public static bool operator ==(IntervalLiteral lLiteral, IntervalLiteral rLiteral) {
      return string.Compare(lLiteral.Value, rLiteral.Value, true) == 0;
    }

    public static bool operator !=(IntervalLiteral lLiteral, IntervalLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
