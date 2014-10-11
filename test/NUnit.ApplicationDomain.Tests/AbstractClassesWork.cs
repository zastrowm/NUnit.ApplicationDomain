using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public abstract class TestsBase
  {
    [Test, RunInApplicationDomainAttribute]
    public void SomeTest()
    {
    }

    protected abstract void SomeChildSetup();
  }

  [TestFixture]
  public class ActualTestClass : TestsBase
  {
    [Test, RunInApplicationDomainAttribute]
    [Description("Each test is in different appdomain, even works in abstract classes")]
    public void Test1()
    {
      Assert.AreEqual(13, StaticInformation.Count);
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }

    [Test, RunInApplicationDomainAttribute]
    [Description("Each test is in different appdomain, even works in abstract classes")]
    public void Test2()
    {
      Assert.AreEqual(13, StaticInformation.Count);
      // if we weren't in a separate app domain, the next test would fail:
      StaticInformation.Count++;

      Console.WriteLine("In Application domain");
    }

    protected override void SomeChildSetup()
    {
    }
  }
}