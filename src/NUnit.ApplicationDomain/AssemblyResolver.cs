using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NUnit.ApplicationDomain
{
  /// <summary>
  ///  Resolves assemblies by delegating to the parent app domain for assembly locations.
  /// </summary>
  [Serializable]
  internal class AssemblyResolver
  {
    private readonly Dictionary<string, Assembly> _resolvedAssemblies;
    private readonly ResolveHelper _resolveHelper;

    /// <summary>
    /// Creates an assembly resolver for all assemblies which might not be
    /// in the same path as the NUnit.ApplicationDomain assembly.
    /// </summary>
    public AssemblyResolver(AppDomain parentAppDomain)
    {
      if (parentAppDomain == null)
        throw new ArgumentNullException("parentAppDomain");

      _resolvedAssemblies = new Dictionary<string, Assembly>();

      // Create the resolve helper in the parent appdomain.
      // The parent appdomain might know or can load the assembly, so ask it indirectly via ResolveHelper.
      _resolveHelper = AppDomainRunner.CreateInDomain<ResolveHelper>(parentAppDomain);
    }

    /// <inheritdoc />
    public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
    {
      Assembly assembly;
      if (_resolvedAssemblies.TryGetValue(args.Name, out assembly))
      {
        return assembly;
      }

      // Not yet known => Store null in the dictionary (helps against stack overflow if a recursive call happens).
      _resolvedAssemblies[args.Name] = null;

      var assemblyLocation = _resolveHelper.ResolveLocationOfAssembly(args.Name);
      if (!String.IsNullOrEmpty(assemblyLocation))
      {
        // The resolve helper found the assembly.
        assembly = Assembly.LoadFrom(assemblyLocation);
        _resolvedAssemblies[args.Name] = assembly;
        return assembly;
      }

      Debug.WriteLine("Unknown assembly to be loaded: '{0}', requested by '{1}'",
                      args.Name,
                      args.RequestingAssembly);
      return null;
    }

    /// <summary> Helps to resolve the types in another app domain. </summary>
    [Serializable]
    private class ResolveHelper : MarshalByRefObject
    {
      public string ResolveLocationOfAssembly(string assemblyName)
      {
        try
        {
          // Load the assembly.
          // If loading fails, it can throw FileNotFoundException or FileLoadException.
          // Ignore those; this will return null.
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
}