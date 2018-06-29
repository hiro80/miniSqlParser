using System;

namespace MiniSqlParser
{
  public class DateLiteral : Literal
  {
    public DateLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value.Trim();
    }

    internal DateLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value.Trim();
    }

    public bool IsCurrentDate {
      get {
        return this.Value.ToUpper() == "CURRENT_DATE";
      }
    }

    public DateTime ToDateTime() {
      if(this.IsCurrentDate) {
        throw new ApplicationException("Can't convert CURRENT_DATE literal");
      }
      var str = this.Value.Remove(0, this.Value.IndexOf('\'')).Trim('\'');
      return
      DateTime.ParseExact(str
                        , new string[] { "yyyy-MM-dd", "yyyy/MM/dd" }
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.None);
    }

    public static bool operator ==(DateLiteral lLiteral, DateLiteral rLiteral) {
      if(lLiteral.IsCurrentDate && rLiteral.IsCurrentDate) {
        return true;
      } else if(lLiteral.IsCurrentDate || rLiteral.IsCurrentDate) {
        return false;
      } else {
        return lLiteral.ToDateTime() == rLiteral.ToDateTime();
      }
    }

    public static bool operator !=(DateLiteral lLiteral, DateLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
