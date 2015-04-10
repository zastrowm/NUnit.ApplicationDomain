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
    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException"> When one or more required arguments are null. </exception>
    /// <param name="typeUnderTest"> The type that the method belongs to and which will be instantiated in the
    ///  test app domain. </param>
    /// <param name="method"> Name of the method to run. </param>
    public TestMethodInformation(Type typeUnderTest, MethodBase method)
    {
      if (typeUnderTest == null)
        throw new ArgumentNullException("typeUnderTest");
      if (method == null)
        throw new ArgumentNullException("method");
      if (method.DeclaringType == null)
        throw new ArgumentNullException("method");

      string configFile = FindConfigFile(Assembly.GetAssembly(typeUnderTest));

      TypeUnderTest = typeUnderTest;
      MethodUnderTest = method;
      AppConfigFile = configFile;

      OutputStream = Console.Out;
      ErrorStream = Console.Error;

      Arguments = CurrentArgumentsRetriever.GetCurrentTestArguments();
    }

    /// <summary>
    ///  The name of the class that contains the method to run in the application domain.
    /// </summary>
    public Type TypeUnderTest { get; set; }

    /// <summary> Gets or sets the name of the test. </summary>
    public MethodBase MethodUnderTest { get; private set; }

    /// <summary>
    ///  Any additional parameters to give to the test method, normally set via TestCaseAttribute.
    /// </summary>
    public object[] Arguments { get; private set; }

    /// <summary> The app config file for the test. </summary>
    public string AppConfigFile { get; set; }

    /// <summary> System.Out. </summary>
    public TextWriter OutputStream { get; private set; }

    /// <summary> System.Err. </summary>
    public TextWriter ErrorStream { get; private set; }

    /// <summary> Try to get the AppConfig file for the assembly. </summary>
    /// <param name="assembly"> The assembly whose app config file should be retrieved. </param>
    /// <returns> The path to the config file, or null if it does not exist. </returns>
    private static string FindConfigFile(Assembly assembly)
    {
      string configFile = new Uri(assembly.EscapedCodeBase).LocalPath + ".config";
      return File.Exists(configFile) ? configFile : null;
    }
  }
}