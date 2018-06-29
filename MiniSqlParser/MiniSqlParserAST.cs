using System.Collections.Generic;
using Antlr4.Runtime;

namespace MiniSqlParser
{
  public class MiniSqlParserAST
  {
    private MiniSqlParserAST() { }

    private static void SetDbmsType(MiniSqlParserLexer lexer
                                  , MiniSqlParserParser parser
                                  , DBMSType dbmsType) {
      if(dbmsType == DBMSType.Unknown) {
        lexer.IsOracle = true;
        parser.IsOracle = true;
        lexer.IsMySql = true;
        parser.IsMySql = true;
        lexer.IsSQLite = true;
        parser.IsSQLite = true;
        lexer.IsMsSql = true;
        parser.IsMsSql = true;
        lexer.IsPostgreSql = true;
        parser.IsPostgreSql = true;
        lexer.IsPervasive = true;
        parser.IsPervasive = true;
        // Unkownの場合""で囲まれた文字列をIDENTIFIERとして
        // 認識させるためMySqlAnsiQuotes=trueとする.
        lexer.MySqlAnsiQuotes = true;
      } else if(dbmsType == DBMSType.Oracle) {
        lexer.IsOracle = true;
        parser.IsOracle = true;
      } else if(dbmsType == DBMSType.MySql) {
        lexer.IsMySql = true;
        parser.IsMySql = true;
      } else if(dbmsType == DBMSType.SQLite) {
        lexer.IsSQLite = true;
        parser.IsSQLite = true;
      } else if(dbmsType == DBMSType.MsSql) {
        lexer.IsMsSql = true;
        parser.IsMsSql = true;
      } else if(dbmsType == DBMSType.PostgreSql) {
        lexer.IsPostgreSql = true;
        parser.IsPostgreSql = true;
      } else if(dbmsType == DBMSType.Pervasive) {
        lexer.IsPervasive = true;
        parser.IsPervasive = true;
      }
    }

    private static MiniSqlParserParser CreateParser(string inputStr
                                                  , DBMSType dbmsType
                                                  , bool forSqlAccessor) {
      var input = new AntlrInputStream(inputStr);
      var lexer = new MiniSqlParserLexer(input);
      var tokens = new CommonTokenStream(lexer);
      var parser = new MiniSqlParserParser(tokens);
      var astListener = new MakeASTListener(tokens, dbmsType, forSqlAccessor);
      var errorListener = new CumulativeErrorListener();
      var lexerErrorListener = new CumulativeLexerErrorListener();

      MiniSqlParserAST.SetDbmsType(lexer, parser, dbmsType);

      // 文法で曖昧な箇所は動的にしか発見できないらしい
      //parser.AddErrorListener(new DiagnosticErrorListener());
      //parser.Interpreter.PredictionMode = PredictionMode.LlExactAmbigDetection;

      lexer.RemoveErrorListeners();
      lexer.AddErrorListener(lexerErrorListener);

      parser.AddParseListener(astListener);
      parser.RemoveErrorListeners();
      parser.AddErrorListener(errorListener);

      return parser;
    }

    public static Stmts CreateStmts(string sqls
                                  , DBMSType dbmsType = DBMSType.Unknown
                                  , bool forSqlAccessor = false) {
      var parser = MiniSqlParserAST.CreateParser(sqls, dbmsType, forSqlAccessor);
      var astListener = (MakeASTListener)parser.ParseListeners[0];
      var errorListener = (CumulativeErrorListener)parser.ErrorListeners[0];

      // SQL文を解析する
      var context = parser.stmts_root();

      if(errorListener.HasSyntaxError) {
        throw errorListener.ThrowException();
      } else if(astListener.HasSqlAccessorSyntaxError) {
        throw astListener.ThrowSqlAccessorException();
      }
      return (Stmts)astListener.GetAST();
    }

    public static Stmt CreateStmt(string sql
                                , DBMSType dbmsType = DBMSType.Unknown
                                , bool forSqlAccessor = false) {
      var parser = MiniSqlParserAST.CreateParser(sql, dbmsType, forSqlAccessor);
      var astListener = (MakeASTListener)parser.ParseListeners[0];
      var errorListener = (CumulativeErrorListener)parser.ErrorListeners[0];

      // SQL文を解析する
      var context = parser.stmt_root();

      if(errorListener.HasSyntaxError) {
        throw errorListener.ThrowException();
      } else if(astListener.HasSqlAccessorSyntaxError) {
        throw astListener.ThrowSqlAccessorException();
      }
      return (Stmt)astListener.GetAST();
    }

    public static IQuery CreateQuery(string query
                                    , DBMSType dbmsType = DBMSType.Unknown
                                    , bool forSqlAccessor = false) {
      var parser = MiniSqlParserAST.CreateParser(query, dbmsType, forSqlAccessor);
      var astListener = (MakeASTListener)parser.ParseListeners[0];
      var errorListener = (CumulativeErrorListener)parser.ErrorListeners[0];

      // SQL文を解析する
      var context = parser.query_root();

      if(errorListener.HasSyntaxError) {
        throw errorListener.ThrowException();
      } else if(astListener.HasSqlAccessorSyntaxError) {
        throw astListener.ThrowSqlAccessorException();
      }
      return (IQuery)astListener.GetAST();
    }

    public static Node CreatePlaceHolderNode(string placeHolderValue
                                           , DBMSType dbmsType = DBMSType.Unknown
                                           , bool forSqlAccessor = false) {
      var parser = MiniSqlParserAST.CreateParser(placeHolderValue, dbmsType, forSqlAccessor);
      var astListener = (MakeASTListener)parser.ParseListeners[0];
      var errorListener = (CumulativeErrorListener)parser.ErrorListeners[0];

      // SQL文を解析する
      var context = parser.placeholder_root();

      if(errorListener.HasSyntaxError) {
        throw errorListener.ThrowException();
      } else if(astListener.HasSqlAccessorSyntaxError) {
        throw astListener.ThrowSqlAccessorException();
      }
      return (Node)astListener.GetAST();
    }

    public static Predicate CreatePredicate(string predicate
                                          , DBMSType dbmsType = DBMSType.Unknown
                                          , bool forSqlAccessor = false) {
      var parser = MiniSqlParserAST.CreateParser(predicate, dbmsType, forSqlAccessor);
      var astListener = (MakeASTListener)parser.ParseListeners[0];
      var errorListener = (CumulativeErrorListener)parser.ErrorListeners[0];

      // SQL文を解析する
      var context = parser.predicate_root();

      if(errorListener.HasSyntaxError) {
        throw errorListener.ThrowException();
      } else if(astListener.HasSqlAccessorSyntaxError) {
        throw astListener.ThrowSqlAccessorException();
      }
      return (Predicate)astListener.GetAST();
    }

    public static Expr CreateExpr(string expr
                                , DBMSType dbmsType = DBMSType.Unknown
                                , bool forSqlAccessor = false) {
      var parser = MiniSqlParserAST.CreateParser(expr, dbmsType, forSqlAccessor);
      var astListener = (MakeASTListener)parser.ParseListeners[0];
      var errorListener = (CumulativeErrorListener)parser.ErrorListeners[0];

      // SQL文を解析する
      var context = parser.expr_root();

      if(errorListener.HasSyntaxError) {
        throw errorListener.ThrowException();
      } else if(astListener.HasSqlAccessorSyntaxError) {
        throw astListener.ThrowSqlAccessorException();
      }
      return (Expr)astListener.GetAST();
    }

  }
}
