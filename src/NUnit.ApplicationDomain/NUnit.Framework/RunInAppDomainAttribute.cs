using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.ApplicationDomain;

namespace NUnit.Framework
{
  /// <summary> Indicates that a test should be run in a separate application domain. </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
  public class RunInApplicationDomainAttribute : TestActionAttribute
  {
    /// <summary> The AppDomain name to use if the application domain is not explicitly specified. </summary>
    public const string DefaultAppDomainName = "{9421D297-D477-4CEE-9C09-38BCC1AB5176}";

    /// <summary> The name to give to the application domain in which the test should be run. </summary>
    public string Name { get; private set; }

    /// <summary> Constructor. </summary>
    public RunInApplicationDomainAttribute()
    {
      Name = DefaultAppDomainName;
    }

    /// <inheritdoc />
    public override void BeforeTest(TestDetails testDetails)
    {
      // only continue execution if
      if (AppDomain.CurrentDomain.FriendlyName == Name)
        return;

      RunInApplicationDomain(testDetails);
    }

    /// <summary>
    ///  Check if we're in the "test" appdomain, and if we aren't, run the given test in an appdomain,
    ///  capture the result, and propegate it back.
    /// </summary>
    private void RunInApplicationDomain(TestDetails testDetails)
    {
      var testClassType = testDetails.Fixture != null
        ? testDetails.Fixture.GetType()
        : testDetails.Method.DeclaringType;

      var methodData = TestMethodInformation.CreateTestMethodInformation(testClassType, testDetails.Method);

      Exception exception = AppDomainRunner.Run(Name, testClassType.Assembly, methodData);

      if (exception == null)
      {
        Assert.Pass();
      }

      if (exception is AssertionException)
      {
        Console.Error.WriteLine("\nAssertion failed in Application Domain");
      }
      else
      {
        Console.Error.WriteLine("\nException thrown in application domain");
      }

      throw exception;
    }

    /// <inheritdoc />
    public override ActionTargets Targets
    {
      get { return ActionTargets.Test; }
    }
  }
}