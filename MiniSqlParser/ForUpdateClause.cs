
namespace MiniSqlParser
{
  public class ForUpdateClause : Node
  {
    internal ForUpdateClause(ForUpdateOfClause ofClause
                            , WaitType waitType
                            , int waitTime
                            , Comments comments) {
      this.Comments = comments;
      this.OfClause = ofClause;
      this.WaitType = waitType;
      this.WaitTime = waitTime;
    }

    private ForUpdateOfClause _ofClause;
    public ForUpdateOfClause OfClause {
      get {
        return _ofClause;
      }
      set {
        _ofClause = value;
        this.SetParent(value);
      }
    }

    public WaitType WaitType { get; set; }

    public int WaitTime { get; set; }

    public bool HasWaitTime {
      get {
        return this.WaitTime >= 0;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      if(this.OfClause != null) {
        this.OfClause.Accept(visitor);
      }
      visitor.VisitAfter(this);
    }
  }
}
