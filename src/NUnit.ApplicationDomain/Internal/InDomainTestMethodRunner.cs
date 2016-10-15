using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary> Executes a test method in the application domain. </summary>
  /// <returns> Runs in the test app domain. </returns>
  internal sealed class InDomainTestMethodRunner : MarshalByRefObject
  {
    /// <summary> Executes the test method indicates by <paramref name="testMethodInfo"/>. </summary>
    /// <param name="testMethodInfo"> Information that describes the test method to execute. </param>
    /// <returns> The exception that occurred as a result of executing the method. </returns>
    public Exception Execute(TestMethodInformation testMethodInfo)
    {
      AppDomainRunner.HiddenDataStore = testMethodInfo.DataStore;

      // Forward this data from the outside application domain
      Console.SetOut(testMethodInfo.OutputStream);
      Console.SetError(testMethodInfo.ErrorStream);

      Type typeUnderTest = testMethodInfo.TypeUnderTest;

      object instance = Activator.CreateInstance(typeUnderTest);

      Exception exceptionCaught = null;

      // mark this test as being in the app domain.  As soon as we're done, we're going to tear down
      // the app domain, so there is no need to set this back to false. 
      AppDomainRunner.IsInTestAppDomain = true;

      // run setup and test, with an exception handler
      exceptionCaught = RunSetupAndTest(testMethodInfo, instance);
      var teardownException = RunTeardown(testMethodInfo, instance);

      return exceptionCaught ?? teardownException;
    }

    /// <summary> Runs the setup and test method, returning the exception that occurred or null if no exception was fired. </summary>
    private static Exception RunSetupAndTest(TestMethodInformation testMethodInfo, object instance)
    {
      try
      {
        foreach (var setupMethod in testMethodInfo.Methods.SetupMethods)
        {
          setupMethod.Invoke(instance, null);
        }

        var taskResult = testMethodInfo.MethodUnderTest.Invoke(instance, testMethodInfo.Arguments) as Task;
        if (taskResult != null)
        {
          var handler = CreateAsyncTestResultHandler(instance);
          handler.Process(taskResult);
        }
      }
      catch (TargetInvocationException e)
      {
        return e.InnerException;
      }

      return null;
    }

    /// <summary>
    ///  Creates the <see cref="IAsyncTestResultHandler"/> to handle an
    ///  async test result.
    /// </summary>
    private static IAsyncTestResultHandler CreateAsyncTestResultHandler(object instance)
    {
      return instance as IAsyncTestResultHandler
             ?? new TaskWaitTestResultHandler();
    }

    /// <summary> Run each teardown method, returning the first exception that occurred, if any. </summary>
    private static Exception RunTeardown(TestMethodInformation testMethodInfo, object instance)
    {
      Exception exception = null;

      foreach (var teardownMethod in testMethodInfo.Methods.TeardownMethods)
      {
        try
        {
          teardownMethod.Invoke(instance, null);
        }
        catch (TargetInvocationException e)
        {
          // we only save the first exception
          if (exception == null)
          {
            exception = e.InnerException;
          }
        }
      }

      return exception;
    }
  }
}