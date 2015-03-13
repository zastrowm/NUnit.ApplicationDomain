using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class TestFixtureSetupAndSetupTestsBase
  {
    public const int StartValue = 9;
    public static int Index = StartValue;

    [SetUp]
    public void Setup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base Setup", 1);
    }

    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureSetup", 3);
    }

    [TestFixtureTearDown]
    public void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base TestFixtureTeardown", 7);
    }

    [TearDown]
    public void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Base Teardown", 9);
    }

    protected void Verify(string text, int expectedIncrement)
    {
      Console.WriteLine(text);

      Index++;
      Assert.AreEqual(StartValue + expectedIncrement, Index);
    }
  }

  [RunInApplicationDomain]
  public class TestFixtureSetupAndSetupTests : TestFixtureSetupAndSetupTestsBase
  {
    [SetUp]
    public void Setup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived Setup", 2);
    }

    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureSetup", 4);
    }

    /// <summary>
    /// Expected calls:
    /// Base Setup
    ///   Derived Setup
    ///     Base TestFixtureSetup
    ///       Derived TestFixtureSetup
    ///         Test Method
    ///       Derived TestFixtureTeardown
    ///     Base TestFixtureTeardown
    ///   Derived Teardown
    /// Base Teardown
    /// </summary>
    [Test]
    [Description("Test method")]
    public void Test_method()
    {
      Verify("Test Method", 5);
    }

    [TestFixtureTearDown]
    public void TestFixtureTeardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived TestFixtureTeardown", 6);
    }

    [TearDown]
    public void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Verify("Derived Teardown", 8);
    }
  }
}