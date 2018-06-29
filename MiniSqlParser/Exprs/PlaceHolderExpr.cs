using System;

namespace MiniSqlParser
{
  [System.Diagnostics.DebuggerDisplay("Name: {Name}")]
  public class PlaceHolderExpr : Expr
  {
    public PlaceHolderExpr(string label) {
      this.Comments = new Comments(1);
      this.Label = label;
    }

    internal PlaceHolderExpr(string label, Comments comments) {
      this.Comments = comments;
      this.Label = label;
    }

    private string _label;
    public string Label {
      get {
        return _label;
      }
      set {
        if(string.IsNullOrEmpty(value)) {
          throw new ArgumentNullException("プレースホルダ名に空文字は設定できません");
        } else if(value[0] != '@' && value[0] != ':' && value[0] != '?') {
          throw new CannotBuildASTException("Undefined prefix of placeholder is used");
        }
        _label = value;
      }
    }

    public string LabelName {
      get {
        if(_label.Length > 1) {
          return _label.Substring(1, _label.Length - 1);
        } else {
          return "";
        }
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
