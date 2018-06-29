using System.ComponentModel;

namespace MiniSqlParser
{
  public class OffsetFetchClause : Node, ILimitClause
  {
    public OffsetFetchClause(int offset
                            , int fetch
                            , bool fetchWithTies) {
      this.Comments = new Comments();
      this.HasOffset = true;
      this.Offset = offset;
      this.HasFetch = true;
      this.Fetch = fetch;
      this.FetchFromFirst = false;
      this.FetchRowCountType = MiniSqlParser.FetchRowCountType.Integer;
      this.FetchWithTies = fetchWithTies;
    }

    internal OffsetFetchClause(bool hasOffset
                              , int offset
                              , bool hasFetch
                              , int fetch
                              , bool fetchFromFirst
                              , FetchRowCountType fetchRowCountType
                              , bool fetchWithTies
                              , Comments comments) {
      this.Comments = comments;
      _hasOffset = hasOffset;
      this.Offset = offset;
      _hasFetch = hasFetch;
      this.Fetch = fetch;
      this.FetchFromFirst = fetchFromFirst;
      _fetchRowCountType = fetchRowCountType;
      _fetchWithTies = fetchWithTies;
    }

    public LimitClauseType Type {
      get {
        return LimitClauseType.OffsetFetch;
      }
    }

    private bool _hasOffset;
    public bool HasOffset {
      get {
        return _hasOffset;
      }
      set {
        this.CorrectComments(_hasOffset, value, 0);
        this.CorrectComments(_hasOffset, value, 0);
        this.CorrectComments(_hasOffset, value, 0);
        _hasOffset = value;
      }
    }

    public int Offset { get; set; }

    private bool _hasFetch;
    public bool HasFetch {
      get {
        return _hasFetch;
      }
      set {
        var hasOffset = new bool[] { this.HasOffset, this.HasOffset, this.HasOffset };
        this.CorrectComments(_hasFetch, value, 0, hasOffset);
        this.CorrectComments(_hasFetch, value, 0, hasOffset);
        if(this.FetchRowCountType == FetchRowCountType.Integer) {
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
        } else if(this.FetchRowCountType == FetchRowCountType.Percentile) {
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
        } else if(this.FetchRowCountType == FetchRowCountType.None) {
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
        } else {
          throw new InvalidEnumArgumentException("Undefined FetchRowCountType is used"
                                                , (int)this.FetchRowCountType
                                                , typeof(FetchRowCountType));
        }
        this.CorrectComments(_hasFetch, value, 0, hasOffset);
        if(this.FetchWithTies) {
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
        } else {
          this.CorrectComments(_hasFetch, value, 0, hasOffset);
        }
        _hasFetch = value;
      }
    }

    public int Fetch { get; set; }

    public bool FetchFromFirst { get; set; }

    private FetchRowCountType _fetchRowCountType;
    public FetchRowCountType FetchRowCountType {
      get {
        return _fetchRowCountType;
      }
      set {
        var hasOffset = new bool[] { this.HasOffset, this.HasOffset, this.HasOffset };
        this.CorrectComments(_fetchRowCountType == FetchRowCountType.Percentile
                            , value == FetchRowCountType.Percentile
                            , 3
                            , hasOffset);
        _fetchRowCountType = value;
      }
    }

    private bool _fetchWithTies;
    public bool FetchWithTies {
      get {
        return _fetchWithTies;
      }
      set {
        var terminalNodeExists = new bool[] { this.HasOffset, this.HasOffset, this.HasOffset
                                            , this.FetchRowCountType == FetchRowCountType.Percentile };
        this.CorrectComments(_fetchWithTies, value, 4, terminalNodeExists);
        _fetchWithTies = value;
      }
    }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.VisitBefore(this);
      visitor.VisitAfter(this);
    }
  }
}
