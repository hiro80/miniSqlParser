using System;

namespace MiniSqlParser
{
  public class Identifier
  {
    [System.Diagnostics.DebuggerDisplay("Id: {Id}")]
    private readonly string _idStr;
    private readonly string _unquotedIdStr;

    public Identifier(string id
                    , DBMSType dbmsType = DBMSType.Unknown
                    , bool mySqlAnsiQuotes = false) {
      if(id == null) {
        throw new ArgumentNullException("id"
                                      , string.Format("Identifier \"{0}\" is null.", id));
      }
      _idStr = id;
      _unquotedIdStr = Identifier.Unquote(id, dbmsType, mySqlAnsiQuotes);
    }

    public string Id {
      get {
        return _unquotedIdStr;
      }
    }

    public string ToUpper() {
      return _unquotedIdStr.ToUpper();
    }

    public string ToLower() {
      return _unquotedIdStr.ToLower();
    }

    private static string Unquote(string idStr
                                , DBMSType dbmsType
                                , bool mySqlAnsiQuotes) {
      var length = idStr.Length;
      if(length < 2) {
        return idStr;
      }

      var openQuote = idStr[0];
      var closeQuote = Identifier.GetCloseQuote(openQuote, dbmsType, mySqlAnsiQuotes);
      if(closeQuote == '\0') {
        return idStr;
      }

      if(idStr[length - 1] != closeQuote) {
        string message = string.Format("Identifier \"{0}\" isn't quoted.", idStr);
        throw new ApplicationException(message);
      }

      string innerStr = idStr.Substring(1, length - 2);
      string pattern = string.Format("{0}{0}", closeQuote);
      string replacement = string.Format("{0}", closeQuote);
      return innerStr.Replace(pattern, replacement);
    }

    private static char GetCloseQuote(char openQuote
                                    , DBMSType dbmsType = DBMSType.Unknown
                                    , bool mySqlAnsiQuotes = false) {

      if(dbmsType == DBMSType.MySql && !mySqlAnsiQuotes) {
        switch(openQuote) {
          case '`':
            return '`';
          default:
            return '\0';
        }
      } else if(dbmsType == DBMSType.MsSql || dbmsType == DBMSType.SQLite) {
        switch(openQuote) {
          case '"':
            return '"';
          case '[':
            return ']';
          default:
            return '\0';
        }
      } else {
        switch(openQuote) {
          case '"':
            return '"';
          default:
            return '\0';
        }
      }
    }

    public override string ToString() {
      return _unquotedIdStr;
    }

    public string ToRawString() {
      return _idStr;
    }

    public override int GetHashCode() {
      return _unquotedIdStr.GetHashCode();
    }

    public override bool Equals(object obj) {
      return obj.GetType() == typeof(Identifier) &&
             _unquotedIdStr.Equals(((Identifier)obj)._unquotedIdStr);
    }

    public static bool IsNullOrEmpty(Identifier id) {
      return id == null || string.IsNullOrEmpty(id._unquotedIdStr);
    }

    public static bool Compare(Identifier lId, Identifier rId, bool ignoreCase) {
      return (object)lId == null && (object)rId == null
          || (object)lId != null && (object)rId != null && 
             string.Compare(lId._unquotedIdStr, rId._unquotedIdStr, ignoreCase) == 0;
    }

    public static bool operator ==(Identifier lId, Identifier rId) {
      return Identifier.Compare(lId, rId, false);
    }

    public static bool operator !=(Identifier lId, Identifier rId) {
      return !(lId == rId);
    }

    public static implicit operator string(Identifier id) {
      return id == null ? null : id._unquotedIdStr;
    }

    public static implicit operator Identifier(string idStr) {
      return idStr == null ? null : new Identifier(idStr);
    }
  }
}
