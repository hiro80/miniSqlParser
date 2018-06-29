using System;
using System.Runtime.Serialization;

namespace MiniSqlParser
{
  [Serializable]
  public class CannotStringifierException : Exception
  {
    public CannotStringifierException(string message)
      :base(message) {
    }

    public CannotStringifierException(string message
                                     , Exception inner)
      : base(message, inner) {
    }

    public CannotStringifierException(SerializationInfo info
                                     , StreamingContext context)
      : base(info, context) {
    }
  }
}
