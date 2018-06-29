using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MiniSqlParser
{
  /// <summary>
  /// プレースホルダに値を適用する
  /// </summary>
  [EditorBrowsable(EditorBrowsableState.Never)]
  abstract public class SetPlaceHoldersVisitor : Visitor
  {
    private readonly Dictionary<string, Node> _placeHolders;

    public SetPlaceHoldersVisitor(Dictionary<string, string> placeHolders) {
      _placeHolders = SetPlaceHoldersVisitor.ConvertPlaceHolders(placeHolders);
    }

    public SetPlaceHoldersVisitor(Dictionary<string, Node> placeHolders) {
      if(placeHolders == null) {
        _placeHolders = new Dictionary<string, Node>();
      } else {
        _placeHolders = placeHolders;
      }
    }

    public static Dictionary<string, Node>
    ConvertPlaceHolders(Dictionary<string, string> placeHolders) {
      var ret = new Dictionary<string, Node>();
      if(placeHolders == null) {
        return ret;
      }
      foreach(var placeHolder in placeHolders) {
        var value = MiniSqlParserAST.CreatePlaceHolderNode(placeHolder.Value);
        ret.Add(placeHolder.Key, value);
      }
      return ret;
    }

    public Dictionary<string, Node> PlaceHolders {
      get {
        return _placeHolders;
      }
    }

    override sealed public void Visit(PlaceHolderExpr expr) {
      //
      // プレースホルダに当てはまるExpr式にvisitorを適用する
      //

      if(!_placeHolders.ContainsKey(expr.LabelName)) {
        this.VisitOnPlaceHolder(expr);
        return;
      }

      if(!(_placeHolders[expr.LabelName] is Expr)) {
        throw new CannotBuildASTException("Type of placeholder value is mismatched.");
      }

      var placeValue = (Expr)(_placeHolders[expr.LabelName]);
      if(this.NeedsBracket(expr.Parent, placeValue)) {
        // 定義されたExpr演算子の優先順位に基づき、
        // 必要であればプレースホルダ値を括弧で囲む
        (new BracketedExpr(placeValue)).Accept(this);
      } else {
        placeValue.Accept(this);
      }
    }

    override sealed public void Visit(PlaceHolderPredicate predicate) {
      //
      // プレースホルダに当てはまるPredicate式にvisitorを適用する
      //

      if(!_placeHolders.ContainsKey(predicate.LabelName)) {
        this.VisitOnPlaceHolder(predicate);
        return;
      }

      if(!(_placeHolders[predicate.LabelName] is Predicate)) {
        throw new CannotBuildASTException("Type of placeholder value is mismatched.");
      }

      var placeValue = (Predicate)(_placeHolders[predicate.LabelName]);
      if(this.NeedsBracket(predicate.Parent, placeValue)) {
        // 定義されたPredicate演算子の優先順位に基づき、
        // 必要であればプレースホルダ値を括弧で囲む
        (new BracketedPredicate(placeValue)).Accept(this);
      } else {
        placeValue.Accept(this);
      }
    }

    virtual protected void VisitOnPlaceHolder(PlaceHolderExpr expr) {
    }

    virtual protected void VisitOnPlaceHolder(PlaceHolderPredicate predicate) {
    }

    private bool NeedsBracket(INode parent, Predicate son) {
      if(parent == null) {
        return false;
      }

      var parentType = parent.GetType();
      var sonType = son.GetType();

      if(sonType == typeof(AndPredicate)) {
        if(parentType == typeof(NotPredicate) ||
           parentType == typeof(CollatePredicate)) {
          return true;
        }
      } else if(sonType == typeof(OrPredicate)) {
        if(parentType == typeof(AndPredicate) ||
           parentType == typeof(NotPredicate) ||
           parentType == typeof(CollatePredicate)) {
          return true;
        }
      }
      return false;
    }

    private bool NeedsBracket(INode parent, Expr son) {
      if(parent == null) {
        return false;
      }

      var parentType = parent.GetType();
      var sonType = son.GetType();

      if(sonType == typeof(BinaryOpExpr)) {
        if(parentType == typeof(BinaryOpExpr) ||
           parentType == typeof(BitwiseNotExpr)) {
          return true;
        }
      }
      return false;
    }
  }
}
