
namespace MiniSqlParser
{
  public abstract class Predicate : Node
  {
    //public abstract Predicate Clone();
    virtual public Predicate Clone() { return this; }

    //public bool IsAndOnlyPredicate() {
    //  if(this.GetType() == typeof(AndPredicate) &&
    //     ((AndPredicate)this).Left.IsAndOnlyPredicate()) {
    //    // 引数で渡された式木の左部分木のみを葉ノードまで探索する
    //    // 探索途中でORノードが無ければその式木はAND連言である
    //    return true;
    //  } else if(this.GetType() == typeof(OrPredicate)) {
    //    return false;
    //  } else if(this.GetType() == typeof(NotPredicate)) {
    //    return ((NotPredicate)this).Operand.IsAndOnlyPredicate();
    //  } else if(this.GetType() == typeof(CollatePredicate)) {
    //    return ((CollatePredicate)this).Operand.IsAndOnlyPredicate();
    //  } else {
    //    return true;
    //  }
    //}
  }
}
