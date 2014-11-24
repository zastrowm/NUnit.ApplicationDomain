using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class MultipleSetupAndTeardownTestsBase
  {
    [SetUp]
    public void Setup()
    {
      Console.WriteLine("(base) Setup Running...");
      StaticInformation.Count++;
    }

    [TearDown]
    public void Teardown()
    {
      Console.WriteLine("(base) Teardown called");
    }
  }

  [RunInApplicationDomain]
  public class MultipleSetupAndTeardownTests : MultipleSetupAndTeardownTestsBase
  {
    [SetUp]
    public void Setup()
    {
      Console.WriteLine("(derived) Setup Running...");
      StaticInformation.Count++;
    }

    [Test, Explicit("User needs to manually identify if it worked")]
    [Description("Test method")]
    public void Test_method()
    {
      Console.WriteLine("Test method");
    }

    [TearDown]
    public void Teardown()
    {
      Console.WriteLine("(derived) Teardown called");
    }
  }
}