using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;

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

      //set the path to the assembly to load.
      info.ApplicationBase = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);

      if (!string.IsNullOrEmpty(testMethodInfo.AppConfigFile))
      {
        info.ConfigurationFile = testMethodInfo.AppConfigFile;
      }

      AppDomain domain = AppDomain.CreateDomain(testDomainName,
                                                null,
                                                info,
                                                GetPermissionSet());
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
  }
}