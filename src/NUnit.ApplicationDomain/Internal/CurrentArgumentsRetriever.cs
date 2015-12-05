using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Internal;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary>
  ///  Facilitates retrieving <code>NUnit.Core.TestExecutionContext.CurrentContext.arguments</code>
  /// </summary>
  internal static class CurrentArgumentsRetriever
  {
    /// <summary>
    ///  Attempts to get the arguments for the current executing test by using reflection to get at  
    ///  <code>NUnit.Core.TestExecutionContext.CurrentContext.arguments</code>
    /// </summary>
    /// <returns> The current arguments for the test, or null of none are available. </returns>
    public static object[] GetCurrentTestArguments()
    {
      // we don't cache the rest of the getters, because we're not sure if the properties actually
      // change or not throughout the lifecycle, so for safety we we-reflect every time. 
      var currentTest = TestExecutionContext.CurrentContext.CurrentTest;

      Type currentTestType = currentTest.GetType();

      var argumentsField = currentTestType.GetProperty(
        "Arguments",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);

      return (object[])argumentsField.GetValue(currentTest, null);
    }
  }
}