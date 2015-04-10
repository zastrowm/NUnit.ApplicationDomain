using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary>
  ///  Resolves assemblies by delegating to the parent app domain for assembly locations.
  /// </summary>
  /// <remarks> Runs in the test app domain. </remarks>
  [Serializable]
  internal class InDomainAssemblyResolver
  {
    private readonly Dictionary<string, Assembly> _resolvedAssemblies;
    private readonly ResolveHelper _resolveHelper;

    /// <summary>
    ///  Creates an assembly resolver for all assemblies which might not be in the same path as the
    ///  NUnit.ApplicationDomain assembly.
    /// </summary>
    /// <remarks>
    ///  Although this object is created in the original app domain, it is serialized/copied into the
    ///  test app domain and thus all methods except the constructor are invoked in the test domain.
    /// </remarks>
    /// <param name="resolveHelper"> The resolve helper from the parent app domain. </param>
    public InDomainAssemblyResolver(ResolveHelper resolveHelper)
    {
      _resolvedAssemblies = new Dictionary<string, Assembly>();

      // Create the resolve helper in the parent appdomain.
      // The parent appdomain might know or can load the assembly, so ask it indirectly via ResolveHelper.
      _resolveHelper = resolveHelper;
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
  }
}