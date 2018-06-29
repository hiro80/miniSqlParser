using System;
using Antlr4.Runtime;


namespace MiniSqlParser
{
  class CumulativeLexerErrorListener: IAntlrErrorListener<int>
  {
    private RecognitionException ex;

    public void SyntaxError(System.IO.TextWriter output
                          , IRecognizer recognizer
                          , int offendingSymbol
                          , int line
                          , int charPositionInLine
                          , string msg
                          , RecognitionException e) {
      throw new NotImplementedException();
    }
  }
}
