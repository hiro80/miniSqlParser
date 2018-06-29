using System.Collections.Generic;
using System.Linq;

public class Comments
{
  private List<string> _comments = new List<string>();

  internal Comments() {
  }

  internal Comments(int count) {
    for(var i = 0; i < count; ++i) {
      _comments.Add(null);
    }
  }

  public string this[int i] {
    get {
      return _comments[i];
    }
    set {
      _comments[i] = value;
    }
  }
  public string Last{
    get{
      return _comments.Last();
    }
  }
  public int Count {
    get {
      return _comments.Count;
    }
  }
  public void UnSetAll() {
    for(var i = 0; i < _comments.Count; ++i) {
      _comments[i] = null;
    }
  }
  internal void Add(string comment) {
    _comments.Add(comment);
  }
  internal void AddRange(Comments comments) {
    _comments.AddRange(comments._comments);
  }
  internal void Insert(int i, string comment) {
    _comments.Insert(i, comment);
  }
  internal void InsertRange(int i, Comments comments) {
    _comments.InsertRange(i, comments._comments);
  }
  internal void RemoveAt(int i) {
    _comments.RemoveAt(i);
  }
  internal void Clear() {
    _comments.Clear();
  }
  internal Comments Clone() {
    var clone = new Comments();
    clone._comments.AddRange(_comments);
    return clone;
  }
}