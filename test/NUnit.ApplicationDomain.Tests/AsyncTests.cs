using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  internal class AsyncTests
  {
    public static bool DidGetSet;

    [Test]
    public async Task VerifyAsync()
    {
      await Task.Delay(TimeSpan.FromSeconds(1));

      AppDomainRunner.DataStore.Set("", true);
      DidGetSet = true;
    }

    [TearDown]
    public void Teardown()
    {
      if (AppDomainRunner.IsInTestAppDomain)
      {
        Assert.That(DidGetSet, Is.True);
      }
      else
      {
        Assert.That(DidGetSet, Is.False);
      }

      Assert.That(AppDomainRunner.DataStore.Get<bool>(""), Is.True);
    }
  }
}