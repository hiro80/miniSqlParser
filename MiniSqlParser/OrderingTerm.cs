
namespace MiniSqlParser
{
  public class OrderingTerm : Node
  {
    public OrderingTerm(Expr term
                       , Identifier collation
                       , OrderSpec orderSpec
                       , NullOrder nullOrder) {
      this.Comments = new Comments();
      this.Term = term;
      this.Collation = collation;
      this.OrderSpec = orderSpec;
      this.NullOrder = nullOrder;
    }

    internal OrderingTerm(Expr term
                         , Identifier collation
                         , OrderSpec orderSpec
                         , NullOrder nullOrder
                         , Comments comments) {
      this.Comments = comments;
      this.Term = term;
      _collation = collation;
      _orderSpec = orderSpec;
      _nullOrder = nullOrder;
    }

    private Expr _term;
    public Expr Term {
      get {
        return _term;
      }
      set {
        _term = value;
        this.SetParent(value);
      }
    }

    private Identifier _collation;
    public Identifier Collation {
      get {
        return _collation;
      }
      set {
        this.CorrectComments(_collation, value, 0);
        this.CorrectComments(_collation, value, 0);
        _collation = value;
      }
    }

    private OrderSpec _orderSpec;
    public OrderSpec OrderSpec {
      get {
        return _orderSpec;
      }
      set {
        this.CorrectComments2(_orderSpec, value, 0, this.HasCollation
                                                  , this.HasCollation);
        _orderSpec = value;
      }
    }

    private NullOrder _nullOrder;
    public NullOrder NullOrder {
      get {
        return _nullOrder;
      }
      set {
        this.CorrectComments2(_nullOrder, value, 0, this.HasCollation
                                                  , this.HasCollation 
                                                  , this.HasOrderSpec);
        this.CorrectComments2(_nullOrder, value, 0, this.HasCollation
                                                  , this.HasCollation
                                                  , this.HasOrderSpec);
        _nullOrder = value;
      }
    }

    public bool HasCollation {
      get {
        return !string.IsNullOrEmpty(this.Collation);
      }
    }

    public bool HasOrderSpec {
      get {
        return _orderSpec != OrderSpec.None;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      this.Term.Accept(visitor);
      visitor.VisitAfter(this);
    } 
  }
}
