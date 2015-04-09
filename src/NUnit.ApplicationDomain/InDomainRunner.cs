using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using NUnit.Framework;

namespace NUnit.ApplicationDomain
{
  /// <summary> Executes a test method in the application domain. </summary>
  internal sealed class InDomainRunner : MarshalByRefObject
  {
    /// <summary> Executes the test method indicates by <paramref name="args"/>. </summary>
    /// <param name="args"> Information that describes the test method to execute. </param>
    /// <returns> The exception that occurred as a result of executing the method. </returns>
    public Exception Execute(TestMethodInformation args)
    {
      // Forward this data from the outside application domain
      Console.SetOut(args.OutputStream);
      Console.SetError(args.ErrorStream);

      Type typeUnderTest = Type.GetType(args.TypeName);

      if (typeUnderTest == null)
        throw new ArgumentException("ClassName did not point to a valid type", "args");

      // get all of the setup methods in the type
      var setupMethods = GetMethodsWithAttributes<TestFixtureSetUpAttribute>(typeUnderTest);
      setupMethods.AddRange(GetMethodsWithAttributes<SetUpAttribute>(typeUnderTest));

      // we want most-derived last
      setupMethods.Reverse();

      // get all of the teardown methods in the type (it is already the way we want it).
      var teardownMethods = GetMethodsWithAttributes<TestFixtureTearDownAttribute>(typeUnderTest);
      teardownMethods.AddRange(GetMethodsWithAttributes<TearDownAttribute>(typeUnderTest));

      var testMethod = typeUnderTest.GetMethod(args.TestName);

      object instance = Activator.CreateInstance(typeUnderTest);

      return ExecuteTestMethod(instance, setupMethods, testMethod, args.Parameters, teardownMethods);
    }

    /// <summary>
    ///  Invokes the test method between the invocation of any setup methods and teardown methods.
    /// </summary>
    /// <param name="instance"> The instance that is having its test method invoked. </param>
    /// <param name="setupMethods"> The setup methods to invoke prior to invoking the test method. </param>
    /// <param name="testMethod"> The actual method under test. </param>
    /// <param name="parameters"> The parameters, potentially set via TestCaseAttribute. </param>
    /// <param name="teardownMethods"> The teardown methods to invoke prior to invoking the test
    ///  method. </param>
    /// <returns> Any exception that occurred while executing the test. </returns>
    private static Exception ExecuteTestMethod(object instance,
                                               IEnumerable<MethodInfo> setupMethods,
                                               MethodInfo testMethod,
                                               object[] parameters,
                                               IEnumerable<MethodInfo> teardownMethods)
    {
      Exception exceptionCaught = null;

      try
      {
        // mark this test as being in the app domain
        AppDomainRunner.IsInTestAppDomain = true;

        foreach (var setupMethod in setupMethods)
        {
          setupMethod.Invoke(instance, null);
        }

        testMethod.Invoke(instance, parameters);

        foreach (var teardownMethod in teardownMethods)
        {
          teardownMethod.Invoke(instance, null);
        }
      }
      catch (TargetInvocationException e)
      {
        exceptionCaught = e.InnerException;
      }

      return exceptionCaught;
    }

    /// <summary>
    ///  Get all methods in the type's hiearachy that have the designated attribute.
    /// </summary>
    /// <returns>
    ///  Returns methods further down in the type hiearachy first, followed by each subsequent type's
    ///  parents' methods.
    /// </returns>
    private static List<MethodInfo> GetMethodsWithAttributes<T>(Type typeUnderTest)
      where T : Attribute
    {
      var methodsFound = new List<MethodInfo>();

      while (typeUnderTest != null)
      {
        const BindingFlags searchFlags = BindingFlags.DeclaredOnly
                                         | BindingFlags.Instance
                                         | BindingFlags.Public
                                         | BindingFlags.NonPublic;

        // get only methods that do not have any parameters and have exactly one Setup method
        var methodsOnCurrentType = from method in typeUnderTest.GetMethods(searchFlags)
                                   where method.GetParameters().Length == 0
                                   let setupAttribute = (T[])method.GetCustomAttributes(typeof(T), false)
                                   where setupAttribute.Length == 1
                                   select method;

        methodsFound.AddRange(methodsOnCurrentType);

        // now get the Setup methods in the base type
        typeUnderTest = typeUnderTest.BaseType;
      }

      return methodsFound;
    }
  }
}