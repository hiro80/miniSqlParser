using System.Collections.Generic;

namespace MiniSqlParser
{
  public abstract class Stmt : Node
  {
    public abstract StmtType Type { get; }
    public virtual string HeaderComment { get; set; }
    public virtual int StmtSeparators { get; set; }
    public virtual bool AutoWhere { get; internal set; }
    public virtual Dictionary<string, string> PlaceHolderAssignComments { get; internal set; }
    public void AppendSeparators(int separatorCount, Comments comments) {
      if(separatorCount != comments.Count) {
        throw new CannotBuildASTException("セミコロンの数とコメントリストの要素数が一致していません");
      }
      this.StmtSeparators += separatorCount;
      this.Comments.AddRange(comments);
    }
    public void TrimRightSeparators() {
      // SQL文末の";"をコメントと共に削除する
      for(var i=this.Comments.Count-1; this.StmtSeparators > 0; --i){
        --this.StmtSeparators;
        this.Comments.RemoveAt(i);
      }
    }
  }
}
