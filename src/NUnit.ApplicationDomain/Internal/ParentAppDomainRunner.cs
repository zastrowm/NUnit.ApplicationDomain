using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Policy;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary> Runs a TestMethodInformation in a child app domain. </summary>
  internal static class ParentAppDomainRunner
  {
    /// <summary> Runs a test in another application domain. </summary>
    /// <param name="testDomainName"> The name to assign to the application domain. </param>
    /// <param name="testMethodInfo"> The arguments to pass to the runner inside the application
    ///  domain. </param>
    /// <returns> The exception that occurred in the test, or null if no exception occurred. </returns>
    internal static Exception Run(string testDomainName, TestMethodInformation testMethodInfo)
    {
      if (testDomainName == null)
        throw new ArgumentNullException("testDomainName");
      if (testMethodInfo == null)
        throw new ArgumentNullException("testMethodInfo");

      var info = new AppDomainSetup();

      //set the path to the NUnit.ApplicationDomain assembly.
      info.ApplicationBase = Path.GetDirectoryName(new Uri(typeof(InDomainTestMethodRunner).Assembly.EscapedCodeBase).LocalPath);

      if (!String.IsNullOrEmpty(testMethodInfo.AppConfigFile))
      {
        info.ConfigurationFile = testMethodInfo.AppConfigFile;
      }

      AppDomain domain = AppDomain.CreateDomain(testDomainName,
                                                null,
                                                info,
                                                GetPermissionSet());

      // Add an assembly resolver for resolving any assemblies not known by the test application domain.
      var assemblyResolver = new InDomainAssemblyResolver(new ResolveHelper());
      domain.AssemblyResolve += assemblyResolver.ResolveEventHandler;

      domain.Load(testMethodInfo.TypeUnderTest.Assembly.GetName());

      var inDomainRunner = domain.CreateInstanceAndUnwrap<InDomainTestMethodRunner>();

      return inDomainRunner.Execute(testMethodInfo);
    }

    /// <summary> Create an instance of the object in the given domain. </summary>
    /// <param name="domain"> The domain in which the object should be constructed. </param>
    /// <typeparam name="T"> The type of the object to construct </typeparam>
    /// <returns> An instance of T, unwrapped from the domain. </returns>
    internal static T CreateInstanceAndUnwrap<T>(this AppDomain domain)
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