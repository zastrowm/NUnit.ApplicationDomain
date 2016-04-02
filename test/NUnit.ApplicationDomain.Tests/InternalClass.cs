using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class InternalClass
  {
    [Test, RunInApplicationDomain]
    public void FirstTest()
    {
      // we expect the first test to pass
      Assert.AreEqual(13, StaticInformation.Count);
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }

    [Test, RunInApplicationDomain]
    public void SecondTest()
    {
      Assert.AreEqual(13, StaticInformation.Count);
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }
  }
}