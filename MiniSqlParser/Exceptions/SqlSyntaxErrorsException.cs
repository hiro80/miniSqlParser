using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace MiniSqlParser
{
  [Serializable]
  public class SqlSyntaxErrorsException : Exception
  {
    public string FailedSql { get; set; }

    [Serializable]
    public class Error
    {
      public int Line { get; set; }
      public int Column { get; set; }
      public string Message { get; set; }
      public Exception Inner { get; set; }
      public Error(int line, int column, string message, Exception inner) {
        this.Line = line;
        this.Column = column;
        this.Message = message;
        this.Inner = inner;
      }
    }

    public List<Error> Errors { get; set; }

    public SqlSyntaxErrorsException(string failedSql) {
      this.FailedSql = failedSql;
      this.Errors = new List<Error>();
    }

    public SqlSyntaxErrorsException(SerializationInfo info
                                  , StreamingContext context)
      : base(info, context) {
      // 固有のメンバ変数をデシリアライズする
      this.FailedSql = info.GetString("failedSql");
      this.Errors = (List<Error>)info.GetValue("errors", typeof(List<Error>));
    }

    //public override void GetObjectData(SerializationInfo info, StreamingContext context){
    //  base.GetObjectData(info, context);
    //  // 固有のメンバ変数をシリアライズする
    //  info.AddValue("failedSql", this.FailedSql);
    //  info.AddValue("errors", this.Errors);
    //}

    public void AddError(int line, int column, string message, Exception inner) {
      this.Errors.Add(new Error(line, column, message, inner));
    }
  }
}
