using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Framework
{
  /// <summary> Helps to run a test in another application domain. </summary>
  public static class AppDomainRunner
  {
    /// <summary> The name of the app-domain in which tests are run. </summary>
    public const string TestAppDomainName = "NUnit.ApplicationDomain ({9421D297-D477-4CEE-9C09-38BCC1AB5176})";

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
  }
}