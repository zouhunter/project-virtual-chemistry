using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CaronteFX
{
  public class CRBiDictionary<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>>, IEnumerable
  {
    IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
    IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

    #region Exception throwing methods

    public void Add(TFirst first, TSecond second)
    {
      if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
        throw new ArgumentException("Duplicate first or second");

      firstToSecond.Add(first, second);
      secondToFirst.Add(second, first);
    }

    public TSecond GetByFirst(TFirst first)
    {
      TSecond second;
      if (!firstToSecond.TryGetValue(first, out second))
        throw new ArgumentException("first");

      return second;
    }

    public TFirst GetBySecond(TSecond second)
    {
      TFirst first;
      if (!secondToFirst.TryGetValue(second, out first))
        throw new ArgumentException("second");

      return first;
    }

    public void RemoveByFirst(TFirst first)
    {
      TSecond second;
      if (!firstToSecond.TryGetValue(first, out second))
        throw new ArgumentException("first");

      firstToSecond.Remove(first);
      secondToFirst.Remove(second);
    }

    public void RemoveBySecond(TSecond second)
    {
      TFirst first;
      if (!secondToFirst.TryGetValue(second, out first))
        throw new ArgumentException("second");

      secondToFirst.Remove(second);
      firstToSecond.Remove(first);
    }

    #endregion

    #region Try methods

    public Boolean TryAdd(TFirst first, TSecond second)
    {
      if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
        return false;

      firstToSecond.Add(first, second);
      secondToFirst.Add(second, first);
      return true;
    }


    public Boolean TryGetByFirst(TFirst first, out TSecond second)
    {
      bool isFound = firstToSecond.TryGetValue(first, out second);
      return isFound;
    }


    public Boolean TryGetBySecond(TSecond second, out TFirst first)
    {
      bool isFound = secondToFirst.TryGetValue(second, out first);
      return isFound;
    }


    public Boolean TryRemoveByFirst(TFirst first)
    {
      TSecond second;
      if (!firstToSecond.TryGetValue(first, out second))
        return false;

      firstToSecond.Remove(first);
      secondToFirst.Remove(second);
      return true;
    }

    public Boolean TryRemoveBySecond(TSecond second)
    {
      TFirst first;
      if (!secondToFirst.TryGetValue(second, out first))
        return false;

      secondToFirst.Remove(second);
      firstToSecond.Remove(first);
      return true;
    }

    #endregion

    public Int32 Count
    {
      get { return firstToSecond.Count; }
    }

 
    public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()
    {
      return ( firstToSecond.GetEnumerator() );
    }
 

    IEnumerator IEnumerable.GetEnumerator()
    {
        var me = this as IEnumerable<KeyValuePair<TFirst, TSecond>>;
        return me.GetEnumerator();
    }

    public ICollection<TFirst> AllFirst()
    {
      return (firstToSecond.Keys);
    }

    public ICollection<TSecond> AllSecond()
    {
      return (secondToFirst.Keys);
    }

    public void Clear()
    {
      firstToSecond.Clear();
      secondToFirst.Clear();
    }
  } //CRBiDictionary
} //namespace Caronte

