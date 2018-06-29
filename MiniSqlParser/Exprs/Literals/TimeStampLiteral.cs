using System;
using System.Linq;

namespace MiniSqlParser
{
  public class TimeStampLiteral : Literal
  {
    public TimeStampLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value;
      this.Fraction = this.GetFraction(value);
    }

    internal TimeStampLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value;
      this.Fraction = this.GetFraction(value);
    }

    public decimal Fraction { get; set; }

    public bool IsCurrentTimeStamp {
      get {
        return this.Value.ToUpper() == "CURRENT_TIMESTAMP";
      }
    }

    public DateTime ToDateTime() {
      if(this.IsCurrentTimeStamp) {
        throw new ApplicationException("Can't convert CURRENT_TIMESTAMP literal");
      }
      var datetimeStr = this.Value.Substring(this.Value.IndexOf('\'') + 1, 19);
      return
      DateTime.ParseExact(datetimeStr
                        , new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss"
                                        ,"yyyy-MM-dd'T'HH:mm:ss", "yyyy/MM/dd'T'HH:mm:ss"}
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.None);
    }

    private decimal GetFraction(string value) {
      string fractionStr = null;
      if(value.Contains('.')) {
        fractionStr = value.Substring(value.IndexOf('.')).TrimEnd('\'');
      }
      if(fractionStr != null) {
        return Decimal.Parse(fractionStr);
      } else {
        return decimal.Zero;
      }
    }

    public static bool operator ==(TimeStampLiteral lLiteral, TimeStampLiteral rLiteral) {
      if(lLiteral.IsCurrentTimeStamp && rLiteral.IsCurrentTimeStamp) {
        return true;
      } else if(lLiteral.IsCurrentTimeStamp || rLiteral.IsCurrentTimeStamp) {
        return false;
      } else {
        return lLiteral.ToDateTime() == rLiteral.ToDateTime() &&
               lLiteral.Fraction == rLiteral.Fraction;
      }
    }

    public static bool operator !=(TimeStampLiteral lLiteral, TimeStampLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
