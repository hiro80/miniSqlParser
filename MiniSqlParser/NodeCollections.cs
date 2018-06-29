using System;
using System.Collections;
using System.Collections.Generic;

namespace MiniSqlParser
{
  public abstract class NodeCollections<TNode>: Node, IEnumerable<TNode>
    where TNode: INode
  {
    protected List<TNode> nodes;
    protected int prefixTerminalNodeCount;
    protected int suffiexTerminalNodeCount;

    virtual public IEnumerator<TNode> GetEnumerator() {
      return nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return nodes.GetEnumerator();
    }

    virtual public void Add(TNode node) {
      if(nodes.Count == 0) {
        for(var i = 0; i < prefixTerminalNodeCount + suffiexTerminalNodeCount; ++i) {
          this.Comments.Add(null);
        }
      } else if(this.Comments != null) {
        var i = this.Comments.Count - suffiexTerminalNodeCount;
        this.Comments.Insert(i, null);
      } else {
        // this.Commentsがnullの場合
        // (StmtsはCommentsを使用していないのでnullに設定されている)
      }
      nodes.Add(node);
      this.SetParent(node);
    }

    virtual public void AddRange(IEnumerable<TNode> nodes) {
      foreach(var n in nodes) {
        this.Add(n);
      }
    }

    virtual public void AddRange(params TNode[] nodes) {
      foreach(var n in nodes) {
        this.Add(n);
      }
    }

    virtual public void Insert(int i, TNode node) {
      if(i < 0) {
        i = reverseIndex(i);
      }
      if(this.Comments == null) {
        // this.Commentsがnullの場合
        // (StmtsはCommentsを使用していないのでnullに設定されている)
      } else if(nodes.Count == 0) {
        for(var j = 0; j < prefixTerminalNodeCount + suffiexTerminalNodeCount; ++j) {
          this.Comments.Add(null);
        }
      } else if(i == nodes.Count){
        // リストの末尾にノードが追加された場合
        this.Comments.Insert(i - 1 + prefixTerminalNodeCount, null);
      } else {
        this.Comments.Insert(i + prefixTerminalNodeCount, null);
      }
      nodes.Insert(i, node);
      this.SetParent(node);
    }

    virtual public void RemoveAt(int i) {
      if(i < 0) {
        i = reverseIndex(i);
      }
      nodes.RemoveAt(i);
      if(this.Comments == null) {
        // this.Commentsがnullの場合
        // (StmtsはCommentsを使用していないのでnullに設定されている)
      } else if(nodes.Count == 0) {
        this.Comments.Clear();
      } else if(i == nodes.Count){
        // リストの末尾ノードが削除された場合
        this.Comments.RemoveAt(i - 1 + prefixTerminalNodeCount);
      } else {
        this.Comments.RemoveAt(i + prefixTerminalNodeCount);
      }
    }

    virtual public void Clear() {
      nodes.Clear();
      this.Comments.Clear();
    }

    virtual public int Count {
      get {
        return nodes.Count;
      }
    }

    virtual public TNode this[int i] {
      get {
        if(i < 0) {
          i = reverseIndex(i);
        }
        return nodes[i];
      }
      set {
        if(i < 0) {
          i = reverseIndex(i);
        }
        nodes[i] = value;
        this.SetParent(value);
      }
    }

    // 負数の指定位置は右からの位置指定(-1が右端)である
    private int reverseIndex(int rIndex){
      var ret = nodes.Count + rIndex + 1;
      if(ret < 0) {
        throw new ArgumentOutOfRangeException("rIndex", 
            "Reverse index is out of range in Node Collections");
      }
      return ret;
    }

  }
}
