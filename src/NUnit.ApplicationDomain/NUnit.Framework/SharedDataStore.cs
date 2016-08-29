using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Framework
{
  /// <summary>
  ///  An object whose properties are stored between a test in the normal app-domain and the test
  ///  executing in the test-domain.
  /// </summary>
  public class SharedDataStore : MarshalByRefObject
  {
    private readonly Dictionary<string, object> _lookup
      = new Dictionary<string, object>();

    /// <summary> Gets an object with the given key. </summary>
    /// <typeparam name="T"> The type of object to retrieve. </typeparam>
    /// <param name="key"> The key of the object to retrieve. </param>
    /// <returns> An object. </returns>
    public T Get<T>(string key)
    {
      return (T)_lookup[key];
    }

    /// <summary> Attempts to get the given item. </summary>
    /// <typeparam name="T"> The type of data to retrieve. </typeparam>
    /// <param name="key"> The key of the object to retrieve. </param>
    /// <param name="value"> [out] The value or default(T) if the object was not in the data-store. </param>
    /// <returns>
    ///  True if the value was present and is contained within <paramref name="value"/>, false
    ///  otherwise.
    /// </returns>
    public bool TryGet<T>(string key, out T value)
    {
      object raw;
      if (_lookup.TryGetValue(key, out raw))
      {
        value = (T)raw;
        return true;
      }

      value = default(T);
      return false;
    }

    /// <summary> Sets the given value for the given key. </summary>
    /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
    ///  illegal values. </exception>
    /// <typeparam name="T"> The type of value to set. </typeparam>
    /// <param name="key"> The key of the object to set. </param>
    /// <param name="value"> The value of the key to set.  The object must be serializable (or if not
    ///  in the test-appdomain, must derive from MarshalByRefObject). </param>
    public void Set<T>(string key, T value)
    {
      _lookup[key] = value;
    }
  }
}