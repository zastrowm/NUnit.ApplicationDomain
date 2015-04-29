using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
      // Forward this data from the outside application domain
      Console.SetOut(testMethodInfo.OutputStream);
      Console.SetError(testMethodInfo.ErrorStream);

      Type typeUnderTest = testMethodInfo.TypeUnderTest;

      object instance = Activator.CreateInstance(typeUnderTest);

      Exception exceptionCaught = null;

      try
      {
        // mark this test as being in the app domain.  As soon as we're done, we're going to tear down
        // the app domain, so there is no need to set this back to false. 
        AppDomainRunner.IsInTestAppDomain = true;

        foreach (var setupMethod in testMethodInfo.Methods.SetupMethods)
        {
          setupMethod.Invoke(instance, null);
        }

        testMethodInfo.MethodUnderTest.Invoke(instance, testMethodInfo.Arguments);

        foreach (var teardownMethod in testMethodInfo.Methods.TeardownMethods)
        {
          teardownMethod.Invoke(instance, null);
        }
      }
      catch (TargetInvocationException e)
      {
        // TODO when moving to .NET 4.5, find out if using ExceptionDispatchInfo.Capture helps at all. 
        exceptionCaught = e.InnerException;
      }

      return exceptionCaught;
    }
  }
}