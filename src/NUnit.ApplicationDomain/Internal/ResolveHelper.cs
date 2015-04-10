using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary> Helps to resolve the types in another app domain. </summary>
  /// <remarks>
  ///  The methods are invoked and marshalled from the test app domain into the original domain.
  /// </remarks>
  [Serializable]
  internal class ResolveHelper : MarshalByRefObject
  {
    public string ResolveLocationOfAssembly(string assemblyName)
    {
      try
      {
        // Load the assembly. if loading fails, it can throw FileNotFoundException or
        // FileLoadException. Ignore those; this will return null. 
        var assembly = Assembly.Load(assemblyName);
        return new Uri(assembly.EscapedCodeBase).LocalPath;
      }
      catch (FileNotFoundException)
      {
      }
      catch (FileLoadException)
      {
      }

      return null;
    }
  }
}