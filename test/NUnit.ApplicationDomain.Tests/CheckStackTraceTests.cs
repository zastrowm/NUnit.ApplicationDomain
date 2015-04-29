using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [Explicit, RunInApplicationDomain]
  public class CheckStackTraceTests
  {
    [Test]
    [Description("Allows a developer to view the exception message that occurs when a test fails in the app domain")]
    public void CheckExceptionMessage()
    {
      Console.WriteLine("In the app domain");
      SomeMethod();
    }

    private static void SomeMethod()
    {
      AnotherMethod();
    }

    private static void AnotherMethod()
    {
      Assert.AreEqual("This", "Should Fail");
    }
  }
}