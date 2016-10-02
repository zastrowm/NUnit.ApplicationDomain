using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class TestContextTests
  {
    [SetUp]
    public void Setup()
    {
      // accessing TestContext.CurrentContext.TestDirectory from the app domain will throw an
      // exception but we need the test directory, so we pass it in via the data store. 
      if (!AppDomainRunner.IsInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set("TestDirectory", TestContext.CurrentContext.TestDirectory);
      }
    }

    [Test, RunInApplicationDomain]
    public void VerifyItFails()
    {
      var testDirectory = AppDomainRunner.DataStore.Get<string>("TestDirectory");
      Console.WriteLine($"The test directory is: {testDirectory}");

      // we can also pass data back into the "normal" domain
      AppDomainRunner.DataStore.Set("ShouldBeSetFromAppDomain", testDirectory);
    }

    [TearDown]
    public void Teardown()
    {
      // okay, make sure everything worked properly now
      if (!AppDomainRunner.IsInTestAppDomain)
      {
        var testDirectory = AppDomainRunner.DataStore.Get<string>("ShouldBeSetFromAppDomain");

        // this should have been set by the test app-domain
        Assert.That(testDirectory, Is.EqualTo(TestContext.CurrentContext.TestDirectory));
      }
    }
  }
}