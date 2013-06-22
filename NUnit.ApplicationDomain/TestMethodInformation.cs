using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace NUnit.ApplicationDomain
{
  /// <summary> All of the arguments for the TestExecutor. </summary>
  public class TestMethodInformation : MarshalByRefObject
  {
    /// <summary> Creates a test method information from a MethodINfo object. </summary>
    /// <exception cref="ArgumentNullException">  When one or more required arguments are null. </exception>
    /// <param name="method"> The method. </param>
    /// <returns> The new test method information. </returns>
    public static TestMethodInformation CreateTestMethodInformation(MethodBase method)
    {
      if (method == null || method.DeclaringType == null)
        throw new ArgumentNullException("method");

      string fullName = method.DeclaringType.FullName + "," + method.DeclaringType.Assembly.GetName().Name;
      return new TestMethodInformation(fullName, method.Name);
    }

    /// <summary> Constructor. </summary>
    /// <param name="className">	The name of the class that contains the method to run in the
    ///  application domain. </param>
    /// <param name="methodName"> Name of the method to run. </param>
    public TestMethodInformation(string className, string methodName)
    {
      if (className == null)
        throw new ArgumentNullException("className");
      if (methodName == null)
        throw new ArgumentNullException("methodName");

      TypeName = className;
      TestName = methodName;
      OutputStream = Console.Out;
      ErrorStream = Console.Error;
    }

    /// <summary> The name of the class that contains the method to run in the application domain. </summary>
    public string TypeName { get; set; }

    /// <summary> Gets or sets the name of the test. </summary>
    public string TestName { get; private set; }

    /// <summary> System.Out. </summary>
    public TextWriter OutputStream { get; private set; }

    /// <summary> System.Err. </summary>
    public TextWriter ErrorStream { get; private set; }
  }
}