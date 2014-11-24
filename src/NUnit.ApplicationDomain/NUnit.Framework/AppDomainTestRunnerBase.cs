using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.ApplicationDomain;

namespace NUnit.Framework
{
  /// <summary> Base class for a test class that wants to run tests in an application. </summary>
  /// <remarks> RunInApplicationDomainAttribute should be preferred over this class. </remarks>
  public abstract class AppDomainTestRunnerBase
  {
    /// <summary> The AppDomain name to use if the application domain is not explicitly specified. </summary>
    public const string DefaultAppDomainName = "NUnit.AppDomain";

    /// <summary>
    ///  Run this test in an application domain.  Invoke this method early in the method inside of an
    ///  if statement, existing if the method returns true.
    /// </summary>
    /// <exception cref="ArgumentNullException">  When one or more required arguments are null. </exception>
    /// <param name="appDomain">  (optional) the name of the application domain to run the test in. </param>
    /// <returns> true if it succeeds, false if it fails. </returns>
    /// <example>
    ///   [Test]
    ///   public void TestMethod()
    ///   {
    ///     if (RunInDomain())
    ///       return;
    ///
    ///     // ... app domain specific testing ...
    ///   }
    /// </example>
    [Obsolete("Use RunInAppDomainAttribute instead")]
    public void RunInDomain(string appDomain = DefaultAppDomainName)
    {
      if (appDomain == null)
        throw new ArgumentNullException("appDomain");

      if (AppDomain.CurrentDomain.FriendlyName == appDomain)
        return;

      var stackTrace = new StackTrace();

      // get the current method from the stack trace
      MethodBase testMethod = stackTrace.GetFrame(1).GetMethod();
      TestMethodInformation args = TestMethodInformation.CreateTestMethodInformation(testMethod.DeclaringType,
                                                                                     testMethod);

      var exception = ApplicationDomain.AppDomainRunner.Run(appDomain, testMethod.DeclaringType.Assembly, args);

      if (exception == null)
      {
        Assert.Pass();
      }

      if (exception is AssertionException)
      {
        Console.WriteLine("Assert failed in Application Domain");
      }
      else
      {
        Console.WriteLine("Exception thrown in application domain");
      }

      throw exception;
    }
  }
}