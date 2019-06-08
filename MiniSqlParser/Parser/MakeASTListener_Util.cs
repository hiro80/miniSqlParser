using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace MiniSqlParser
{
  internal partial class MakeASTListener: MiniSqlParserBaseListener
  {
    public bool HasSqlAccessorSyntaxError {
      get {
        return _saSyntaxErrorsException != null;
      }
    }

    public SqlSyntaxErrorsException ThrowSqlAccessorException() {
      return _saSyntaxErrorsException;
    }

    private void AddSqlAccessorSyntaxError(string message, ParserRuleContext context) {
      if(_saSyntaxErrorsException == null) {
        _saSyntaxErrorsException =
            new SqlSyntaxErrorsException(_tokens.TokenSource.InputStream.ToString());
      }
      _saSyntaxErrorsException.AddError(context.Start.Line, context.Start.Column, message, null);
    }

    // contextオブジェクトの直下の全ての子ノードからコメントを取得する
    private Comments GetComments(ParserRuleContext context) {
      var ret = new Comments();

      if(context == null) {
        return ret;
      }

      var start = context.Start.TokenIndex;
      for(var i = 0; i < context.ChildCount; ++i) {
        IParseTree child = context.GetChild(i);

        // 終端ノードの直後のコメントだけを取得する
        if(child.ChildCount > 1) {
          continue;
        } else if(child.ChildCount == 1) {
          if(   child.GetType() == typeof(MiniSqlParserParser.IdentifierContext)
             || child.GetType() == typeof(MiniSqlParserParser.Column_aliasContext)
             || child.GetType() == typeof(MiniSqlParserParser.Table_aliasContext)
             || child.GetType() == typeof(MiniSqlParserParser.Collation_nameContext)
             || child.GetType() == typeof(MiniSqlParserParser.Constraint_nameContext)) {
            child = child.GetChild(0);
          } else {
            continue;
          }
        }

        var childTokenIndex = child.SourceInterval.b;
        //IList<IToken> commentTokens = _tokens.GetHiddenTokensToRight(childTokenIndex, 1);
        IList<IToken> commentTokens = this.GetHiddenTokensToRight(childTokenIndex);
        if(commentTokens == null || commentTokens.Count == 0) {
          ret.Add(null);
        } else {
          string comment = null;
          foreach(var commentToken in commentTokens) {
            comment += commentToken.Text;
          }
          ret.Add(comment);
        }
      }

      return ret;
    }

    private List<IToken> GetHiddenTokensToRight(int childTokenIndex) {
      var ret = new List<IToken>(_tokens.Size - 1);
      foreach(var t in _tokens.GetTokens(childTokenIndex + 1, _tokens.Size - 1)) {
        if(t.Channel > 0) {
          ret.Add(t);
        } else {
          break;
        }
      }
      return ret;
    }

    private Comments GetComments(ITerminalNode tnode) {
      if(tnode == null) {
        return null;
      }
      return this.GetComments(tnode.Symbol);
    }

    private Comments GetComments(IToken prevToken) {
      var ret = new Comments();

      //IList<IToken> commentTokens = _tokens.GetHiddenTokensToRight(prevToken.TokenIndex, 1);
      IList<IToken> commentTokens = this.GetHiddenTokensToRight(prevToken.TokenIndex);
      if(commentTokens == null) {
        ret.Add(null);
        return ret;
      }

      string comment = null;
      foreach(var commentToken in commentTokens) {
        comment += commentToken.Text;
      }
      ret.Add(comment);
      return ret;
    }

    // 入力文字列の最初のコメントを取得する
    private string GetHeaderComment(int stmtStartIndex) {
      string ret = null;

      int index = stmtStartIndex - 1;
      while(index >= 0) {
        var tokens = _tokens.GetTokens(index, index);
        if(tokens == null || tokens.Count == 0 || tokens[0].Channel == 0) {
          break;
        }
        ret = tokens[0].Text + ret;
        --index;
      }
      return ret;
    }

    private Dictionary<string, string> GetDefaultValuePlaceHolders(int stmtStartIndex) {
      var ret = new Dictionary<string,string>();

      int index = stmtStartIndex - 1;
      while(index >= 0) {
        var tokens = _tokens.GetTokens(index, index);
        if(tokens == null || tokens.Count == 0 || tokens[0].Channel == 0) {
          break;
        }
        IToken commentToken = tokens[0];
        if(commentToken.Channel == 3) {
          string text = commentToken.Text;

          var nameStartIndex = text.IndexOf('@', 3);
          var nameEndIndex = text.IndexOf('=', nameStartIndex + 1);
          var name = text.Substring(nameStartIndex + 1, nameEndIndex - nameStartIndex - 1).TrimEnd();

          var valueStartIndex = text.IndexOf('"', nameEndIndex + 1);
          var valueEndIndex = text.LastIndexOf('"', text.Length - 3);
          var value = text.Substring(valueStartIndex + 1, valueEndIndex - valueStartIndex - 1).Replace("\"\"", "\"");

          // 1つのSQL文に同じ名称のプレースホルダ値定義コメントがあれば
          // 後に記述されたほうを優先する
          if(!ret.ContainsKey(name)) {
            ret.Add(name, value);
          }
        }
        --index;
      }
      return ret;
    }

    private bool GetHeaderAutoWhere(int stmtStartIndex) {
      bool ret = true;

      int index = stmtStartIndex - 1;
      while(index >= 0) {
        var tokens = _tokens.GetTokens(index, index);
        if(tokens == null || tokens.Count == 0 || tokens[0].Channel == 0) {
          break;
        }
        IToken commentToken = tokens[0];
        if(commentToken.Channel == 4) {
          return false;
        }
        --index;
      }
      return ret;
    }

    private void ShiftLastCommentToNextStmt(Stmts stmts) {
      // 入力文の先頭のコメントを取得する
      // (先頭がコメントでない場合はnull)
      //string lastComment = this.GetTopComment();
      //bool lastAutoWhere = this.GetTopAutoWhere();
      string lastComment = stmts[0].HeaderComment;
      bool lastAutoWhere = stmts[0].AutoWhere;

      // SQL文の先頭のコメントをHeaderCommentに設定する
      foreach(var stmt in stmts) {
        stmt.HeaderComment = lastComment;
        stmt.AutoWhere = lastAutoWhere;
        // SQL文の先頭のコメントは、直前のSQL文の末尾のコメントとして格納されている.
        // HeaderCommentと重複しないようにするため、これを削除する
        if(stmt.StmtSeparators > 0 && stmt.Comments.Count > 0) {
          lastComment = stmt.Comments.Last;
          stmt.Comments[stmt.Comments.Count - 1] = null;
          lastAutoWhere = stmt.AutoWhere;
        } else {
          lastComment = null;
          lastAutoWhere = true;
        }
      }

      // SQL文の末尾のコメントを格納する次のSQL文がない場合は、
      // NullStmtを新規に生成してこれに格納する.
      if(lastComment != null) {
        var nullStmt = new NullStmt(0, new Comments());
        nullStmt.HeaderComment = lastComment;
        stmts.Add(nullStmt);
      }
    }

    //private string GetText(IToken token) {
    //  if(token == null) {
    //    return null;
    //  }
    //  return token.Text;
    //}

    //private string GetText(ParserRuleContext context) {
    //  if(context == null) {
    //    return null;
    //  }
    //  return context.GetText();
    //}

    private Identifier GetIdentifier(IToken token) {
      if(token == null) {
        return null;
      }
      return new Identifier(token.Text, this.DBMSType);
    }

    private Identifier GetIdentifier(ParserRuleContext context) {
      if(context == null) {
        return null;
      }
      return new Identifier(context.GetText(), this.DBMSType);
    }

    private string GetTableAliasNameFromDocComment(MiniSqlParserParser.Aliased_table_nameContext context) {
      return this.GetTableAliasNameFromDocComment(context.table_name());
    }

    private string GetTableAliasNameFromDocComment(MiniSqlParserParser.Hinted_table_nameContext context) {
      return this.GetTableAliasNameFromDocComment(context.table_name());
    }

    private string GetTableAliasNameFromDocComment(MiniSqlParserParser.Table_nameContext context) {
      var afterTableName = context.Stop;
      IList<IToken> commentTokens = _tokens.GetHiddenTokensToRight(afterTableName.TokenIndex, 2);

      if(commentTokens == null) {
        return null;
      }

      IToken commentToken = commentTokens[0];
      if(commentToken == null) {
        return null;
      }

      // コメント文字列からテーブル別名を抽出する
      return commentToken.Text.Substring(3, commentToken.Text.Length - 3 - 2).Trim();
    }

    //private Dictionary<string, string> GetPlaceHolderAssignComments(ITerminalNode context) {
    //  var ret = new Dictionary<string,string>();
    //  var afterScols = context.SourceInterval.b;

    //  do {
    //    IList<IToken> commentTokens = _tokens.GetHiddenTokensToRight(afterScols, 3);
    //    if(commentTokens == null) {
    //      return ret;
    //    }

    //    IToken commentToken = commentTokens[0];
    //    var text = commentToken.Text;
        
    //    var nameStartIndex= text.IndexOf('@', 3);
    //    var nameEndIndex = text.IndexOf('=', nameStartIndex + 1);
    //    var name = text.Substring(nameStartIndex + 1, nameEndIndex - nameStartIndex - 1).TrimEnd();

    //    var valueStartIndex = text.IndexOf('"', nameEndIndex + 1);
    //    var valueEndIndex = text.LastIndexOf('"', text.Length - 3);
    //    var value = text.Substring(valueStartIndex + 1, valueEndIndex - valueStartIndex - 1).Replace("\"\"", "\"");

    //    ret.Add(name, value);

    //    ++afterScols;
    //  } while(true);
    //}

    //private bool GetAutoWhere(Antlr4.Runtime.Tree.ITerminalNode context) {
    //  var afterScols = context.SourceInterval.b;
    //  IList<IToken> commentTokens = _tokens.GetHiddenTokensToRight(afterScols, 4);

    //  if(commentTokens == null) {
    //    return true;
    //  }

    //  IToken commentToken = commentTokens[0];
    //  if(commentToken == null) {
    //    return true;
    //  }

    //  // コメント文字列がAutoWhere設定値であれば文法定義よりその値はfalseである
    //  return false;
    //}

    private string Coalesce(params ITerminalNode[] nodes) {
      foreach(var n in nodes) {
        if(n != null && !string.IsNullOrEmpty(n.Symbol.Text)) {
          return n.Symbol.Text;
        }
      }
      return null;
    }

    private void PrintStack() {
      foreach(var item in _stack) {
        Trace.WriteLine(item.ToString());
      }
    }
  }
}