using Antlr4.Runtime;

namespace MiniSqlParser
{
  class CumulativeErrorListener : BaseErrorListener
  {
    private SqlSyntaxErrorsException ex;

    public override void SyntaxError(System.IO.TextWriter output
                                    , IRecognizer recognizer
                                    , IToken offendingSymbol
                                    , int line
                                    , int charPositionInLine
                                    , string msg
                                    , RecognitionException e) {
      if(ex == null) {
        // 最初の文法エラーが発生した後はASTの構築ができないので、
        // ConstructASTListenerを処理させないようにする
        ((Parser)recognizer).RemoveParseListeners();
        var failedSql = ((CommonTokenStream)recognizer.InputStream).TokenSource.InputStream.ToString();
        ex = new SqlSyntaxErrorsException(failedSql);
      }
      ex.AddError(line, charPositionInLine, msg, e);
    }

    public bool HasSyntaxError {
      get {
        return ex != null && ex.Errors.Count > 0;
      }
    }

    public SqlSyntaxErrorsException ThrowException() {
      return ex;
    }
  }
}
