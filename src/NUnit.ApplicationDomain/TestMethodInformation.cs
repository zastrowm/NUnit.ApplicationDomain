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

      TryFindParameters();
    }

    private void TryFindParameters()
    {
      var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                  where assembly.FullName.Contains("nunit.core")
                  from type in assembly.GetTypes()
                  where type.FullName == "NUnit.Core.TestExecutionContext"
                  select type;

      var execContext = types.FirstOrDefault();
      if (execContext == null)
        return;

      var currentContextProperty = execContext.GetProperty("CurrentContext", BindingFlags.Public | BindingFlags.Static);
      dynamic currentContext = currentContextProperty.GetValue(null, null);

      var currentTest = currentContext.CurrentTest;
      var argumentsField = ((Type)currentTest.GetType()).GetField(
        "arguments",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
      Parameters = argumentsField.GetValue(currentTest);
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