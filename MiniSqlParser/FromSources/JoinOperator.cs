
namespace MiniSqlParser
{
  public class JoinOperator: Node
  {
    internal JoinOperator(JoinType joinType
                        , bool hasNaturalKeyword
                        , bool hasOuterKeyword
                        , Comments comments) {
      this.JoinType = joinType;
      this.HasNaturalKeyword = hasNaturalKeyword;
      this.HasOuterKeyword = hasOuterKeyword;
      this.Comments = comments;
    }

    public JoinType JoinType { get; set; }
    public bool HasNaturalKeyword { get; set; }
    public bool HasOuterKeyword { get; set; }

    protected override void AcceptImp(IVisitor visitor) {
      visitor.Visit(this);
    }
  }
}
