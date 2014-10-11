using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class AppDomainTestRunnerBaseTests
  {
    internal class NonPublicClass : AppDomainTestRunnerBase
    {
      [Test, ExpectedException(typeof(InvalidOperationException))]
      [Description("Only public classes can be run in application domain")]
      public void Only_public_classes_can_be_run_in_application_domain()
      {
        try
        {
          RunInDomain();
        }
        catch (InvalidOperationException)
        {
          Assert.Pass();
        }
      }
    }
  }
}