using NUnit.Framework;

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;

namespace NUnit.ApplicationDomain
{
  /// <summary> All of the arguments for the TestExecutor. </summary>
  internal class TestMethodInformation : MarshalByRefObject
  {
    /// <summary> Creates a test method information from a MethodInfo object. </summary>
    /// <exception cref="ArgumentNullException"> When one or more required arguments are null. </exception>
    /// <param name="type"> The type that should be instantiated in order to run the test
    ///  in the application domain. </param>
    /// <param name="method"> The method. </param>
    /// <returns> The new test method information. </returns>
    public static TestMethodInformation CreateTestMethodInformation(Type type, MethodBase method)
    {
      if (method == null || method.DeclaringType == null)
        throw new ArgumentNullException("method");

      string fullName = type.FullName + "," + type.Assembly.GetName().Name;
      string configFile = FindConfigFile(Assembly.GetAssembly(type));

      return new TestMethodInformation(fullName, method.Name, configFile);
    }

    /// <summary> Creates a test method information from a MethodInfo object. </summary>
    /// <exception cref="ArgumentNullException"> When one or more required arguments are null. </exception>
    /// <param name="type"> The type that should be instantiated in order to run the test
    ///  in the application domain. </param>
    /// <param name="method"> The method. </param>
    /// <param name="testMethodFullName"> The full name of the test method, as passed by NUnit. This includes the values of any parameters. </param>
    /// <returns> The new test method information. </returns>
    public static TestMethodInformation CreateTestMethodInformation(Type type, MethodBase method, string testMethodFullName)
    {
      if (method == null || method.DeclaringType == null)
        throw new ArgumentNullException("method");

      string fullName = type.FullName + "," + type.Assembly.GetName().Name;
      string configFile = FindConfigFile(Assembly.GetAssembly(type));

      return new TestMethodInformation(fullName, method.Name, configFile, (TestCaseAttribute[])method.GetCustomAttributes(typeof(TestCaseAttribute), false), testMethodFullName);
    }

    /// <summary> Try to get the AppConfig file for the assembly. </summary>
    /// <param name="assembly"> The assembly whose app config file should be retrieved. </param>
    /// <returns> The path to the config file, or null if it does not exist. </returns>
    private static string FindConfigFile(Assembly assembly)
    {
      string configFile = new Uri(assembly.EscapedCodeBase).LocalPath + ".config";
      return File.Exists(configFile) ? configFile : null;
    }

    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException">  When one or more required arguments are null. </exception>
    /// <param name="className">  The name of the class that contains the method to run in the
    ///   application domain. </param>
    /// <param name="methodName"> Name of the method to run. </param>
    /// <param name="appConfigFile"> The config file for the method. </param>
    /// <param name="testCaseAttributes"> Any TestCaseAttributes applied to the test method. </param>
    /// <param name="testMethodFullName"> The full name of the test method, as passed by NUnit. This includes the values of any parameters. </param>
    private TestMethodInformation(string className, string methodName, string appConfigFile, TestCaseAttribute[] testCaseAttributes, string testMethodFullName)
    {
      if (className == null)
        throw new ArgumentNullException("className");
      if (methodName == null)
        throw new ArgumentNullException("methodName");

      TypeName = className;
      TestName = methodName;
      AppConfigFile = appConfigFile;
      OutputStream = Console.Out;
      ErrorStream = Console.Error;

      var paramsStart = testMethodFullName.LastIndexOf('(') + 1;
      var paramsEnd = testMethodFullName.LastIndexOf(')');
      if (paramsStart <= 0 || paramsEnd <= 0 || paramsEnd < paramsStart)
      {
        // No parameters found in name.
        return;
      }

      var paramsString = testMethodFullName.Substring(paramsStart, paramsEnd - paramsStart);
      foreach (var testCaseAttribute in testCaseAttributes)
      {
        var argumentsDisplayName = GetArgumentsDisplayName(testCaseAttribute.Arguments);
        if (argumentsDisplayName == paramsString)
        {
          Parameters = testCaseAttribute.Arguments;
          break;
        }
      }
    }

    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException">  When one or more required arguments are null. </exception>
    /// <param name="className">  The name of the class that contains the method to run in the
    ///   application domain. </param>
    /// <param name="methodName"> Name of the method to run. </param>
    /// <param name="appConfigFile"> The config file for the method. </param>
    /// <param name="testCaseAttributes"> Any TestCaseAttributes applied to the test method. </param>
    private TestMethodInformation(string className, string methodName, string appConfigFile)
    {
      if (className == null)
        throw new ArgumentNullException("className");
      if (methodName == null)
        throw new ArgumentNullException("methodName");

      TypeName = className;
      TestName = methodName;
      AppConfigFile = appConfigFile;
      OutputStream = Console.Out;
      ErrorStream = Console.Error;
    }

    private static string GetArgumentsDisplayName(object[] arglist)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < arglist.Length; ++index)
      {
        if (index > 0)
          stringBuilder.Append(",");

        object obj = arglist[index];
        string str = obj == null ? "null" : obj.ToString();
        if (obj is double)
        {
          str = ((double)obj).ToString(CultureInfo.InvariantCulture);
          if (str.IndexOf('.') == -1)
            str += ".0";

          str += "d";
        }
        else if (obj is float)
        {
          str = ((float)obj).ToString(CultureInfo.InvariantCulture);
          if (str.IndexOf('.') == -1)
            str += ".0";

          str += "f";
        }
        else if (obj is long)
          str += "L";
        else if (obj is ulong)
          str += "UL";
        else if (obj is string)
          str = "\"" + str.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";

        stringBuilder.Append(str);
      }

      return stringBuilder.ToString();
    }

    /// <summary>
    ///  The name of the class that contains the method to run in the application domain.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary> Gets or sets the name of the test. </summary>
    public string TestName { get; private set; }

    /// <summary> The app config file for the test. </summary>
    public string AppConfigFile { get; set; }

    /// <summary> System.Out. </summary>
    public TextWriter OutputStream { get; private set; }

    /// <summary> System.Err. </summary>
    public TextWriter ErrorStream { get; private set; }

    /// <summary> Any additional parameters to give to the test method, set via TestCaseAttribute. </summary>
    public object[] Parameters { get; private set; }
  }
}