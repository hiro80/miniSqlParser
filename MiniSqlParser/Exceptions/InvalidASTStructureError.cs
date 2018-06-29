using System;
using System.Runtime.Serialization;

namespace MiniSqlParser
{
  [Serializable]
  public class InvalidASTStructureError : Exception
  {
    public InvalidASTStructureError(string message)
      :base(message) {
    }

    public InvalidASTStructureError(string message
                                  , Exception inner)
      : base(message, inner) {
    }

    public InvalidASTStructureError(SerializationInfo info
                                  , StreamingContext context)
      : base(info, context) {
    }
  }
}
