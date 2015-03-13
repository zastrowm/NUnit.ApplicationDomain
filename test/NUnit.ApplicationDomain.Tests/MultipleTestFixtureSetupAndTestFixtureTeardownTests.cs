using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class MultipleTestFixtureSetupAndTestFixtureTeardownTestsBase
  {
    public const int StartValue = 9;
    public static int Index = StartValue;

    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureSetup", 1);
    }

    [TestFixtureTearDown]
    public void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureTeardown", 5);
    }

    protected void Verify(string text, int expectedIncrement)
    {
      Console.WriteLine(text);

      Index++;
      Assert.AreEqual(StartValue + expectedIncrement, Index);
    }
  }

  [RunInApplicationDomain]
  public class MultipleTestFixtureSetupAndTestFixtureTeardownTests : MultipleTestFixtureSetupAndTestFixtureTeardownTestsBase
  {
    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureSetup", 2);
    }

    [Test]
    [Description("Test method")]
    public void Test_method()
    {
      Verify("Test Method", 3);
    }

    [TestFixtureTearDown]
    public void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureTeardown", 4);
    }
  }
}