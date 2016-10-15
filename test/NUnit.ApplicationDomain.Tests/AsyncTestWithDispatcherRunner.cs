using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  [RunInApplicationDomain]
  internal class AsyncTestWithDispatcherRunner : IAsyncTestResultHandler
  {
    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsNotInTestAppDomain)
        return;

      // install a sync context so that awaits continue on the dispatcher. 
      SynchronizationContext.SetSynchronizationContext(
        new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
    }

    [Test]
    public async Task WaitsForDispatcherToContinue()
    {
      // pretend that for some made-up reason, we need to be in the event loop 
      await Dispatcher.Yield();

      // and pretend that something later triggers that allows us to complete
      await Task.Delay(TimeSpan.FromSeconds(3));

      AppDomainRunner.DataStore.Set<bool>("ran", true);
    }

    [TearDown]
    public void Teardown()
    {
      Assert.That(AppDomainRunner.DataStore.Get<bool>("ran"), Is.True);
    }

    /// <inheritdoc />
    void IAsyncTestResultHandler.Process(Task task)
    {
      // if we just simply did task.Wait(), we would block indefinitely because no-one is message
      // pumping. 

      // instead, tell the dispatcher to run until the task has resolved
      var frame = new DispatcherFrame();
      task.ContinueWith(_1 => frame.Continue = false);
      Dispatcher.PushFrame(frame);

      // propagate any exceptions
      task.Wait();
    }
  }
}