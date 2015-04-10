using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary>
  ///  Facilitates retrieving <code>NUnit.Core.TestExecutionContext.CurrentContext.arguments</code>
  /// </summary>
  internal static class CurrentArgumentsRetriever
  {
    private static readonly PropertyInfo TestExecutionContextType;

    /// <summary> Static constructor. </summary>
    static CurrentArgumentsRetriever()
    {
      TestExecutionContextType = FindCurrentContextProperty();
    }

    /// <summary>
    ///  Attempts to get the arguments for the current executing test by using reflection to get at  
    ///  <code>NUnit.Core.TestExecutionContext.CurrentContext.arguments</code>
    /// </summary>
    /// <returns> The current arguments for the test, or null of none are available. </returns>
    public static object[] GetCurrentTestArguments()
    {
      // we never found it, so bail out
      if (TestExecutionContextType == null)
        return null;

      dynamic currentContext = TestExecutionContextType.GetValue(null, null);

      // we don't cache the rest of the getters, because we're not sure if the properties actually
      // change or not throughout the lifecycle, so for safety we we-reflect every time. 
      var currentTest = currentContext.CurrentTest;
      Type currentTestType = currentTest.GetType();

      var argumentsField = currentTestType.GetField(
        "arguments",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

      return argumentsField.GetValue(currentTest);
    }

    /// <summary>
    ///  Get the property that represents the getter for
    ///  <code>NUnit.Core.TestExecutionContext.CurrentContext</code>. This can be safely cached
    ///  because it's a static type and a static property.
    /// </summary>
    private static PropertyInfo FindCurrentContextProperty()
    {
      var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                  where assembly.FullName.Contains("nunit.core")
                  from type in assembly.GetTypes()
                  where type.FullName == "NUnit.Core.TestExecutionContext"
                  select type;

      var textExecutionContextType = types.FirstOrDefault();
      // not sure what would cause this but better safe than sorry
      if (textExecutionContextType == null)
        return null;


      PropertyInfo getter = textExecutionContextType.GetProperty("CurrentContext",
                                                                 BindingFlags.Public | BindingFlags.Static);

      return getter;
    }
  }
}