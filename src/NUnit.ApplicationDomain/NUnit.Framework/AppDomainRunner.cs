using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Framework
{
  /// <summary> Helps to run a test in another application domain. </summary>
  public static class AppDomainRunner
  {
    /// <summary> The name of the app-domain in which tests are run. </summary>
    public static readonly string TestAppDomainName
      = "NUnit.ApplicationDomain ({9421D297-D477-4CEE-9C09-38BCC1AB5176})";

    /// <summary> Static constructor. </summary>
    static AppDomainRunner()
    {
      ShouldIncludeAppDomainErrorMessages = true;
    }

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

    /// <summary>
    ///  True if messages should be printed to standard output when a test failure occurs while in the
    ///  test app domain. 
    /// </summary>
    /// <remarks> True by default. </remarks>
    public static bool ShouldIncludeAppDomainErrorMessages { get; set; }
  }
}