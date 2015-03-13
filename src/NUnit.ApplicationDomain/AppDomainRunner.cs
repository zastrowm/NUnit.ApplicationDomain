﻿using System;
using System.Collections.Generic;
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
      info.ApplicationBase = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);

      if (!string.IsNullOrEmpty(testMethodInfo.AppConfigFile))
      {
        info.ConfigurationFile = testMethodInfo.AppConfigFile;
      }

      AppDomain domain = AppDomain.CreateDomain(testDomainName,
                                                null,
                                                info,
                                                GetPermissionSet());

      // Add an assembly resolver, which knows the path for the NUnit.framework assembly
      // and the assembly containing the test to run.
      var ar = new AssemblyResolver(assembly);
      domain.AssemblyResolve += ar.ResolveEventHandler;

      domain.Load(assembly.GetName());

      var instance = (InDomainRunner) domain.CreateInstanceAndUnwrap(
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
    private class AssemblyResolver
    {
      /// <summary>
      /// Creates an assembly resolver for all assemblies which might not be
      /// in the same path as the NUnit.ApplicationDomain assembly.
      /// </summary>
      /// <param name="assembly">
      /// The assembly that contains the test to run.
      /// </param>
      public AssemblyResolver(Assembly assembly)
      {
        // Store the name and location of the NUnit.framework assembly.
        var nunitFrameworkAssembly = typeof(TestAttribute).Assembly;
        this.NUnitFrameworkLocation = nunitFrameworkAssembly.Location;
        this.NUnitFrameworkName = nunitFrameworkAssembly.FullName;
        this.NUnitFrameworkSimpleName = new AssemblyName(this.NUnitFrameworkName).Name;

        // Store the name and location of assembly containing the test to run.
        this.AssemblyLocation = assembly.Location;
        this.AssemblyName = assembly.FullName;
        this.AssemblySimpleName = new AssemblyName(this.AssemblyName).Name;
      }

      private string NUnitFrameworkName { get; set; }
      private string NUnitFrameworkSimpleName { get; set; }
      private string NUnitFrameworkLocation { get; set; }

      private string AssemblyName { get; set; }
      private string AssemblySimpleName { get; set; }
      private string AssemblyLocation { get; set; }

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
        if (args.Name == this.NUnitFrameworkName || args.Name == this.NUnitFrameworkSimpleName)
        {
          // The NUnit.framework assembly could not be loaded
          // => load it from the location known via the constructor.
          var assembly = Assembly.LoadFrom(this.NUnitFrameworkLocation);
          if (assembly == null)
            throw new InvalidOperationException("nunit.framework assembly not found.");

          return assembly;
        }

        if (args.Name == this.AssemblyName || args.Name == this.AssemblySimpleName)
        {
          // The assembly containing the test to run could not be loaded
          // => load it from the location known via the constructor.
          var assembly = Assembly.LoadFrom(this.AssemblyLocation);
          if (assembly == null)
            throw new InvalidOperationException("Assembly containing the tests not found.");

          return assembly;
        }

        return null;
      }
    }
  }
}