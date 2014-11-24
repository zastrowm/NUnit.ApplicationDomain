using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.ApplicationDomain;

namespace NUnit.Framework
{
  /// <summary> Indicates that a test should be run in a separate application domain. </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
  public class RunInApplicationDomainAttribute : Attribute, ITestAction
  {
    /// <summary> The name to give to the application domain in which the test should be run. </summary>
    public string Name { get; private set; }

    /// <summary> Constructor. </summary>
    public RunInApplicationDomainAttribute()
      : this(null) {}

    /// <summary> Constructor. </summary>
    /// <param name="name"> The name to give to the application domain in which the test should be run. </param>
    [Obsolete("Use the parameter-less constructor overload")]
    public RunInApplicationDomainAttribute(string name)
    {
      Name = name ?? AppDomainTestRunnerBase.DefaultAppDomainName;
    }

    /// <summary>
    /// Executed before each test is run
    /// </summary>
    /// <param name="testDetails">Provides details about the test that is going to be run.</param>
    public void BeforeTest(TestDetails testDetails)
    {
      // only continue execution if
      if (AppDomain.CurrentDomain.FriendlyName == Name)
        return;

      var testClassType = testDetails.Fixture != null
        ? testDetails.Fixture.GetType()
        : testDetails.Method.DeclaringType;

      Exception exception = ApplicationDomain.AppDomainRunner.Run(
        Name,
        testClassType.Assembly,
        TestMethodInformation.CreateTestMethodInformation(testClassType, testDetails.Method));

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

    /// <summary>
    /// Executed after each test is run
    /// </summary>
    /// <param name="testDetails">Provides details about the test that has just been run.</param>
    public void AfterTest(TestDetails testDetails)
    {

    }

    /// <summary>
    /// Provides the target for the action attribute
    /// </summary>
    /// <returns>
    /// The target for the action attribute
    /// </returns>
    public ActionTargets Targets
    {
      get { return ActionTargets.Test; }
    }
  }
}