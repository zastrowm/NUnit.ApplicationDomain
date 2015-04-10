using System;
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
    /// <summary> The name of the app-domain in which tests are run. </summary>
    public const string TestAppDomainName = " NUnit.ApplicationDomain ({9421D297-D477-4CEE-9C09-38BCC1AB5176})";

    /// <summary>
    ///  Returns true if the current test is being executed in an application domain created by the
    ///  <see cref="RunInApplicationDomainAttribute"/>
    /// </summary>
    public static bool IsInTestAppDomain { get; internal set; }

    /// <summary>
    ///  Returns false if the current test is being executed in an application domain created by the
    ///  <see cref="RunInApplicationDomainAttribute"/>
    /// </summary>
    /// <remarks> Equivalent to !IsInTestAppDomain. </remarks>
    public static bool IsNotInTestAppDomain
    {
      get { return !IsInTestAppDomain; }
    }

    /// <summary> Runs a test in another application domain. </summary>
    /// <param name="testDomainName"> The name to assign to the application domain. </param>
    /// <param name="testMethodInfo"> The arguments to pass to the runner inside the application
    ///  domain. </param>
    /// <returns> The exception that occurred in the test, or null if no exception occurred. </returns>
    internal static Exception Run(string testDomainName, TestMethodInformation testMethodInfo)
    {
      if (!testMethodInfo.TypeUnderTest.IsPublic)
        throw new InvalidOperationException("Class under test must be declared as public");

      var info = new AppDomainSetup();

      //set the path to the NUnit.ApplicationDomain assembly.
      info.ApplicationBase = Path.GetDirectoryName(new Uri(typeof(InDomainRunner).Assembly.EscapedCodeBase).LocalPath);

      if (!String.IsNullOrEmpty(testMethodInfo.AppConfigFile))
      {
        info.ConfigurationFile = testMethodInfo.AppConfigFile;
      }

      AppDomain domain = AppDomain.CreateDomain(testDomainName,
                                                null,
                                                info,
                                                GetPermissionSet());

      // Add an assembly resolver for resolving any assemblies not known by the test application domain.
      var assemblyResolver = new AssemblyResolver(AppDomain.CurrentDomain);
      domain.AssemblyResolve += assemblyResolver.ResolveEventHandler;

      domain.Load(testMethodInfo.TypeUnderTest.Assembly.GetName());

      var inDomainRunner = CreateInDomain<InDomainRunner>(domain);

      return inDomainRunner.Execute(testMethodInfo);
    }

    /// <summary> Create an instance of the object in the given domain. </summary>
    /// <param name="domain"> The domain in which the object should be constructed. </param>
    /// <typeparam name="T"> The type of the object to construct </typeparam>
    /// <returns> An instance of T, unwrapped from the domain. </returns>
    internal static T CreateInDomain<T>(AppDomain domain)
    {
      return (T)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName,
                                               typeof(T).FullName);
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
  }
}