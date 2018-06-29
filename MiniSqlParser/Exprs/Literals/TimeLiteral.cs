using System;
using System.Linq;

namespace MiniSqlParser
{
  public class TimeLiteral : Literal
  {
    public TimeLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value;
      this.Fraction = this.GetFraction(value);
    }

    internal TimeLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value;
      this.Fraction = this.GetFraction(value);
    }

    public decimal Fraction { get; set; }

    public bool IsCurrentTime {
      get {
        return this.Value.ToUpper() == "CURRENT_TIME";
      }
    }

    public DateTime ToDateTime() {
      if(this.IsCurrentTime) {
        throw new ApplicationException("Can't convert CURRENT_TIME literal");
      }
      var timeStr = this.Value.Substring(this.Value.IndexOf('\'') + 1, 8);
      return
      DateTime.ParseExact(timeStr
                        , "HH:mm:ss"
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.NoCurrentDateDefault);
    }

    private decimal GetFraction(string value){
      string fractionStr = null;
      if(value.Contains('.')) {
        fractionStr = value.Substring(value.IndexOf('.')).TrimEnd('\'');
      }
      if(fractionStr != null) {
        return Decimal.Parse(fractionStr);
      }else{
        return decimal.Zero;
      }
    }

    public static bool operator ==(TimeLiteral lLiteral, TimeLiteral rLiteral) {
      if(lLiteral.IsCurrentTime && rLiteral.IsCurrentTime) {
        return true;
      } else if(lLiteral.IsCurrentTime || rLiteral.IsCurrentTime) {
        return false;
      } else {
        return lLiteral.ToDateTime() == rLiteral.ToDateTime() &&
               lLiteral.Fraction == rLiteral.Fraction;
      }
    }

    public static bool operator !=(TimeLiteral lLiteral, TimeLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
