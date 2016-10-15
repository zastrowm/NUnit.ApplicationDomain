using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NUnit.ApplicationDomain
{
  /// <summary>
  ///  A <see cref="IAsyncTestResultHandler"/> that simply invokes <see cref="Task.Wait()"/> on the
  ///  result of a task-returning test.
  /// </summary>
  public class TaskWaitTestResultHandler : IAsyncTestResultHandler
  {
    /// <inheritdoc />
    public void Process(Task task)
      => task.Wait();
  }
}