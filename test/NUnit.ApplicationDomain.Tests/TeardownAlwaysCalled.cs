using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  /// <summary>
  ///  At one point, there was a bug where teardown was not being called when a test failed.  This
  ///  class verifies that this no longer occurs.
  /// </summary>
  [TestFixture, RunInApplicationDomain]
  internal class TeardownAlwaysCalled
  {
    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set("setup", true);
      }
    }

    [Test]
    public void SuccessTest()
    {
      if (AppDomainRunner.IsInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set("test", true);
      }
    }

    [Test]
    public void ExceptionTest()
    {
      if (AppDomainRunner.IsInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set("test", true);
      }

      Assert.Fail("Teardown should still be run");
    }

    [TearDown]
    public void TearDown()
    {
      if (AppDomainRunner.IsInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set("teardown", true);
      }

      if (AppDomainRunner.IsNotInTestAppDomain)
      {
        bool didRunSetup = AppDomainRunner.DataStore.Get<bool>("setup");
        bool didRunTest = AppDomainRunner.DataStore.Get<bool>("test");
        bool didRunTeardown = AppDomainRunner.DataStore.Get<bool>("teardown");

        Assert.That(didRunSetup, Is.True);
        Assert.That(didRunTest, Is.True);
        Assert.That(didRunTeardown, Is.True);
      }
    }
  }
}