using System;

namespace MiniSqlParser
{
  public class StringLiteral : Literal
  {
    public StringLiteral(string value) {
      this.Comments = new Comments(1);
      this.Value = value;
    }

    internal StringLiteral(string value, Comments comments) {
      this.Comments = comments;
      this.Value = value;
    }

    public bool IsNString {
      get {
        return this.Value[0] == 'N' || this.Value[0] == 'n';
      }
    }

    public string Unquote() {
      int idx = 0;
      int startIdx;
      int innerStrLength;
      
      if(this.Value[0] == 'N' || this.Value[0] == 'n') {
        ++idx;
      }

      if(this.Value[idx] == '\'' || this.Value[idx] == '"') {
        startIdx = idx + 1;
        innerStrLength = this.Value.Length - startIdx - 1;
        char openQuote = this.Value[idx];
        string innerStr = this.Value.Substring(startIdx, innerStrLength);
        string pattern = string.Format("{0}{0}", openQuote);
        string replacement = string.Format("{0}", openQuote);
        return innerStr.Replace(pattern, replacement);

      } else if(this.Value[idx] == 'Q' || this.Value[idx] == 'q') {
        startIdx = idx + 3;
        innerStrLength = this.Value.Length - startIdx - 2;
        return this.Value.Substring(startIdx, innerStrLength);

      } else {
        string message = string.Format("Undefined string literal quote \"{0}\" is usesd.", this.Value[idx]);
        throw new ApplicationException(message);
      }
    }

    public static bool operator ==(StringLiteral lLiteral, StringLiteral rLiteral) {
      // UNICODEの正準等価性に基づく比較ではない
      return lLiteral.Unquote() == rLiteral.Unquote();
    }

    public static bool operator !=(StringLiteral lLiteral, StringLiteral rLiteral) {
      return !(lLiteral == rLiteral);
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
