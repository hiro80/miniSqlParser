using System;
using System.Collections.Generic;

namespace MiniSqlParser
{
  public interface INode
  {
    INode Parent { get; set; }
    Comments Comments { get; set; }
    //INode Clone();
    void Accept(IVisitor visitor);
  }

  public abstract class Node: INode
  {
    public INode Parent { get; set; }
    public Comments Comments { get; set; }
    /// <summary>
    /// 添付オブジェクト(使用目的自由)
    /// </summary>
    public object Attachment { get; set; }

    virtual public void Accept(IVisitor visitor) {
      if(visitor == null) {
        throw new ArgumentNullException("visitor");
      }
      this.AcceptImp(visitor);
    }
    abstract protected void AcceptImp(IVisitor visitor);

    protected void SetParent(INode node) {
      if(node != null) {
        node.Parent = this;
      }
    }

    protected static int CountTrue(params bool[] args) {
      int ret = 0;
      foreach(var b in args) {
        ret += b ? 1 : 0; 
      }
      return ret;
    }

    protected void CorrectComments(INode propValue
                                  , INode value
                                  , int offset
                                  , params bool[] terminalNodeExistsArray){
      if(value != null) {
        if(propValue == null) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.Insert(offset + i, null);
        }
        value.Parent = this;
      } else {
        if(propValue != null) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.RemoveAt(offset + i);
        }
      }
    }

    protected void CorrectComments<T>(NodeCollections<T> propValue
                                    , INode value
                                    , int offset
                                    , params bool[] terminalNodeExistsArray)
    where T : INode{
      if(value != null) {
        if(propValue == null || propValue.Count == 0) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.Insert(offset + i, null);
        }
        value.Parent = this;
      } else {
        if(propValue != null && propValue.Count > 0) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.RemoveAt(offset + i);
        }
      }
    }

    protected void CorrectComments2<TEnum>(TEnum propValue
                                          , TEnum value
                                          , int offset
                                          , params bool[] terminalNodeExistsArray)
    where TEnum: struct, System.IConvertible {
      int propValueId = propValue.ToInt32(null);
      int valueId = value.ToInt32(null);
      int nullValueId = default(TEnum).ToInt32(null);
      if(valueId != nullValueId) {
        if(propValueId == nullValueId) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.Insert(offset + i, null);
        }
      } else {
        if(propValueId != nullValueId) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.RemoveAt(offset + i);
        }
      }
    }

    protected void CorrectComments(string propValue
                                  , string value
                                  , int offset
                                  , params bool[] terminalNodeExistsArray) {
      if(!string.IsNullOrEmpty(value)) {
        if(string.IsNullOrEmpty(propValue)) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.Insert(offset + i, null);
        }
      } else {
        if(!string.IsNullOrEmpty(propValue)) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.RemoveAt(offset + i);
        }
      }
    }

    protected void CorrectComments(bool propValue
                                  , bool value
                                  , int offset
                                  , params bool[] terminalNodeExistsArray) {
      if(value) {
        if(!propValue) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.Insert(offset + i, null);
        }
      } else {
        if(propValue) {
          var i = CountTrue(terminalNodeExistsArray);
          this.Comments.RemoveAt(offset + i);
        }
      }
    }
  }
}
