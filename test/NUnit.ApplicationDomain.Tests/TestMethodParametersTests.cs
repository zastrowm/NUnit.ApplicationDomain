using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public class TestMethodParametersTests
  {
    [TestCase(1, "1")]
    [TestCase(2, "2")]
    [RunInApplicationDomain]
    [Description("Using TestCase")]
    public void Parameters_using_testcase(int number, string numberString)
    {
      Assert.That(number.ToString(), Is.EqualTo(numberString));
    }
  }
}