using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniSqlParser
{
  /// <summary>
  /// 大文字小文字の区別を優先してKeyを検索するDictionary
  /// </summary>
  public class BestCaseDictionary<TValue> : IDictionary<string, TValue>
  {
    private Dictionary<string, TValue>                     _caseSensitiveKeyDic;
    private Dictionary<string, Dictionary<string, TValue>> _caseInSensitiveKeyDic;

    public BestCaseDictionary() {
      _caseSensitiveKeyDic = new Dictionary<string, TValue>();
      _caseInSensitiveKeyDic = new Dictionary<string, Dictionary<string, TValue>>();
    }

    public BestCaseDictionary(Dictionary<string,TValue> dictionary) : this() {
      foreach(var keyValue in dictionary) {
        this.Add(keyValue);
      }
    }

    #region IDictionary<string,TValue> メンバー

    public void Add(string key, TValue value) {
      // Case SensitiveなKeyでTValueを登録する
      _caseSensitiveKeyDic.Add(key, value);

      // Case In-SensitiveなKeyでTValueを登録する
      if(_caseInSensitiveKeyDic.ContainsKey(key.ToUpper())) {
        _caseInSensitiveKeyDic[key.ToUpper()].Add(key, value);
      } else {
        var subDic = new Dictionary<string, TValue>();
        subDic.Add(key, value);
        _caseInSensitiveKeyDic.Add(key.ToUpper(), subDic);
      }
    }

    public bool ContainsKey(string key) {
      if(_caseSensitiveKeyDic.ContainsKey(key)) {
        // Case Sensitiveで適合するKeyがある場合はTrueを返す
        return true;
      } else if(_caseInSensitiveKeyDic.ContainsKey(key.ToUpper())) {
        // Case Sensitiveで適合するKeyが無い場合、かつ
        // Case In-SensitiveなKeyが複数登録されている場合はFlaseを返す
        var subDic = _caseInSensitiveKeyDic[key.ToUpper()];
        return subDic.Count == 1;
      } else {
        return false;
      }
    }

    public ICollection<string> Keys {
      get { return _caseSensitiveKeyDic.Keys; }
    }

    public bool Remove(string key) {
      throw new NotImplementedException();
    }

    public bool TryGetValue(string key, out TValue value) {
      throw new NotImplementedException();
    }

    public ICollection<TValue> Values {
      get { return _caseSensitiveKeyDic.Values; }
    }

    public TValue this[string key] {
      get {
        if(_caseSensitiveKeyDic.ContainsKey(key)) {
          // Case Sensitiveで適合するKeyがある場合
          return _caseSensitiveKeyDic[key];
        } else if(_caseInSensitiveKeyDic.ContainsKey(key.ToUpper())) {
          // Case Sensitiveで適合するKeyが無い場合、かつ
          // Case In-SensitiveなKeyが複数登録されている場合
          var subDic = _caseInSensitiveKeyDic[key.ToUpper()];
          if(subDic.Count > 1) {
            throw new KeyNotFoundException("Multi case pattern key exists.");
          }
          return subDic.Values.First();
        } else {
          throw new KeyNotFoundException("No key exists.");
        }
      }
      set {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ICollection<KeyValuePair<string,TValue>> メンバー

    public void Add(KeyValuePair<string, TValue> item) {
      this.Add(item.Key, item.Value);
    }

    public void Clear() {
      throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<string, TValue> item) {
      throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex) {
      throw new NotImplementedException();
    }

    public int Count {
      get { return _caseSensitiveKeyDic.Count(); }
    }

    public bool IsReadOnly {
      get { throw new NotImplementedException(); }
    }

    public bool Remove(KeyValuePair<string, TValue> item) {
      throw new NotImplementedException();
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,TValue>> メンバー

    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() {
      return _caseSensitiveKeyDic.GetEnumerator();
    }

    #endregion

    #region IEnumerable メンバー

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    #endregion
  }
}
