using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using NUnit.Framework;

namespace NUnit.ApplicationDomain
{
  /// <summary> Executes a test method in the application domain. </summary>
  public sealed class InDomainRunner : MarshalByRefObject
  {
    /// <summary> Executes the test method indicates by <paramref name="args"/>. </summary>
    /// <param name="args"> Information that describes the test method to execute. </param>
    /// <returns> The exception that occurred as a result of executing the method. </returns>
    public Exception Execute(TestMethodInformation args)
    {
      // Forward this data from the outside application domain
      Console.SetOut(args.OutputStream);
      Console.SetError(args.ErrorStream);

      Console.WriteLine(args.TypeName);
      Type typeUnderTest = Type.GetType(args.TypeName);

      if (typeUnderTest == null)
        throw new ArgumentException("ClassName did not point to a valid type", "args");

      // get all of the setup methods in the 
      IEnumerable<MethodInfo> setupMethods = from method in typeUnderTest.GetMethods()
                                             let setups =
                                               (SetUpAttribute[])
                                               method.GetCustomAttributes(typeof(SetUpAttribute), false)
                                             where setups.Length == 1
                                             select method;

      MethodInfo setupMethod = setupMethods.FirstOrDefault();
      MethodInfo testMethod = typeUnderTest.GetMethod(args.TestName);

      object instance = Activator.CreateInstance(typeUnderTest);

      Exception exceptionCaught = null;

      try
      {
        if (setupMethod != null)
        {
          setupMethod.Invoke(instance, null);
        }

        testMethod.Invoke(instance, null);
      }
      catch (TargetInvocationException e)
      {
        exceptionCaught = e.InnerException;
      }

      return exceptionCaught;
    }
  }
}