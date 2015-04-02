using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using NUnit.Framework;

namespace NUnit.ApplicationDomain
{
  /// <summary> Helps to run a test in another application domain. </summary>
  public static class AppDomainRunner
  {
    /// <summary>
    ///  Returns true if the current test is being executed in an application domain created by the
    ///  <typeparamref name="NUnit.Framework.RunInAppDomainAttribute"/>.
    /// </summary>
    public static bool IsInTestAppDomain { get; internal set; }

    /// <summary> Runs a test in another application domain. </summary>
    /// <param name="testDomainName"> The name to assign to the application domain. </param>
    /// <param name="assembly"> The assembly that contains the test to run. </param>
    /// <param name="testMethodInfo"> The arguments to pass to the runner inside the application domain. </param>
    /// <returns> The exception that occurred in the test, or null if no exception occurred. </returns>
    internal static Exception Run(string testDomainName, Assembly assembly, TestMethodInformation testMethodInfo)
    {
      Verify(assembly, testMethodInfo);

      var info = new AppDomainSetup();

      //set the path to the NUnit.ApplicationDomain assembly.
      info.ApplicationBase = Path.GetDirectoryName(new Uri(typeof(InDomainRunner).Assembly.EscapedCodeBase).LocalPath);

      if (!string.IsNullOrEmpty(testMethodInfo.AppConfigFile))
      {
        info.ConfigurationFile = testMethodInfo.AppConfigFile;
      }

      AppDomain domain = AppDomain.CreateDomain(testDomainName,
                                                null,
                                                info,
                                                GetPermissionSet());

      // Add an assembly resolver for resolving any assemblies not known by the test application domain.
      var ar = new AssemblyResolver(AppDomain.CurrentDomain);
      domain.AssemblyResolve += ar.ResolveEventHandler;

      domain.Load(assembly.GetName());

      var instance = (InDomainRunner)domain.CreateInstanceAndUnwrap(
        typeof(InDomainRunner).Assembly.FullName,
        typeof(InDomainRunner).FullName);

      Exception exception = instance.Execute(testMethodInfo);

      if (exception != null)
      {
        return exception;
      }

      return null;
    }

    /// <summary> Verifies that the type can be created from the application domain. </summary>
    /// <param name="assembly"> The assembly that contains the test to run. </param>
    /// <param name="args"> The arguments to pass to the runner inside the application domain. </param>
    private static void Verify(Assembly assembly, TestMethodInformation args)
    {
      Type type = Type.GetType(args.TypeName);

      if (!type.IsPublic)
        throw new InvalidOperationException("Class under test must be declared as public");
    }

    /// <summary>
    /// create a permission set
    /// </summary>
    private static PermissionSet GetPermissionSet()
    {
      //create an evidence of type zone
      var ev = new Evidence();
      ev.AddHostEvidence(new Zone(SecurityZone.MyComputer));

      //return the PermissionSets specific to the type of zone
      return SecurityManager.GetStandardSandbox(ev);
    }

    [Serializable]
    private class ResolveHelper : MarshalByRefObject
    {
      public string ResolveLocationOfAssembly(string assemblyName)
      {
        Assembly assembly = null;

        // Load the assembly.
        // If loading fails, it can throw FileNotFoundException or FileLoadException.
        // Ignore those; this will return null.
        try
        {
          assembly = Assembly.Load(assemblyName);
        }
        catch (FileNotFoundException)
        {
        }
        catch (FileLoadException)
        {
        }

        return assembly != null ? new Uri(assembly.EscapedCodeBase).LocalPath : null;
      }
    }

    [Serializable]
    private class AssemblyResolver
    {
      /// <summary>
      /// Creates an assembly resolver for all assemblies which might not be
      /// in the same path as the NUnit.ApplicationDomain assembly.
      /// </summary>
      public AssemblyResolver(AppDomain parentAppDomain)
      {
        this.ParentAppDomain = parentAppDomain;

        // Create the resolve helper in the parent appdomain.
        // The parent appdomain might know or can load the assembly, so ask it indirectly via ResolveHelper.
        this.ResolveHelper =
          (ResolveHelper)
          this.ParentAppDomain.CreateInstanceAndUnwrap(typeof(ResolveHelper).Assembly.FullName, typeof(ResolveHelper).FullName);

        this.ResolvedAssemblies = new Dictionary<string, Assembly>();
      }

      private AppDomain ParentAppDomain { get; set; }

      private ResolveHelper ResolveHelper { get; set; }

      private Dictionary<string, Assembly> ResolvedAssemblies { get; set; }

      /// <summary>
      /// Called when an assembly could not be resolved.
      /// </summary>
      /// <param name="sender">
      /// The caller.
      /// </param>
      /// <param name="args">
      /// The assembly which could not be resolved.
      /// </param>
      /// <returns>
      /// The assembly, if known by the resolver; null, otherwise.
      /// </returns>
      public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
      {
        // Check the dictionary.
        Assembly assembly;
        if (this.ResolvedAssemblies.TryGetValue(args.Name, out assembly))
        {
          return assembly;
        }

        // Not yet known => Store null in the dictionary (helps against stack overflow if a recursive call happens).
        this.ResolvedAssemblies[args.Name] = null;

        var assemblyLocation = this.ResolveHelper.ResolveLocationOfAssembly(args.Name);
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
          // The resolve helper found the assembly.
          assembly = Assembly.LoadFrom(assemblyLocation);
          this.ResolvedAssemblies[args.Name] = assembly;
          return assembly;
        }

        Debug.WriteLine("Unknown assembly to be loaded: '{0}', requested by '{1}'", args.Name, args.RequestingAssembly);
        return null;
      }
    }
  }
}