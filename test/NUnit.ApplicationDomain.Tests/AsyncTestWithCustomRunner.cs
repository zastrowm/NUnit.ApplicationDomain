using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  internal class AsyncTestWithCustomRunner : IAsyncTestResultHandler
  {
    [Test]
    public async Task VerifyThatCustomRunnerIsInvoked()
    {
      AppDomainRunner.DataStore.Set("ShouldBeFalse", false);

      // we should end up NOT waiting for this
      await Task.Delay(TimeSpan.FromSeconds(3));
      AppDomainRunner.DataStore.Set("ShouldBeFalse", true);
    }

    [TearDown]
    public void Teardown()
    {
      if (!AppDomainRunner.IsInTestAppDomain)
        return;

      Assert.That(AppDomainRunner.DataStore.Get<bool>("ShouldBeFalse"), Is.False);
      Assert.That(AppDomainRunner.DataStore.Get<bool>("Runner"), Is.True);
    }

    public void Process(Task task)
    {
      AppDomainRunner.DataStore.Set<bool>("Runner", true);
    }
  }
}