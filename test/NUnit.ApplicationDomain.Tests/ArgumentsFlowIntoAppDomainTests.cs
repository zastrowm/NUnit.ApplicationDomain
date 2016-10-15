using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public class ArgumentsFlowIntoAppDomainTests
  {
    [TestCase(1, "1")]
    [TestCase(2, "2")]
    [RunInApplicationDomain]
    [Description("Using TestCase")]
    public void Parameters_using_testcase(int number, string numberString)
    {
      Assert.That(number.ToString(), Is.EqualTo(numberString));
    }

    [TestCase("test\"")]
    [TestCase(@"1
2")]
    [RunInApplicationDomain]
    public void String_parameters_using_testcase(string value)
    {
      Assert.That(value, Is.Not.Null);
      Assert.That(value, Is.Not.Empty);
    }

    [TestCase(1)]
    [TestCase(2.1)]
    [RunInApplicationDomain]
    public void Double_parameters_using_testcase(double value)
    {
      Assert.That(value, Is.GreaterThan(0));
    }

    [TestCase(1f)]
    [TestCase(2.1f)]
    [RunInApplicationDomain]
    public void Float_parameters_using_testcase(float value)
    {
      Assert.That(value, Is.GreaterThan(0));
    }

    [TestCase(1L)]
    [TestCase(2L)]
    [RunInApplicationDomain]
    public void Long_parameters_using_testcase(long value)
    {
      Assert.That(value, Is.GreaterThan(0));
    }

    [TestCase(1UL)]
    [TestCase(2UL)]
    [RunInApplicationDomain]
    public void ULong_parameters_using_testcase(ulong value)
    {
      Assert.That(value, Is.GreaterThan(0));
    }

    [TestCase(1)]
    [TestCase(2)]
    [RunInApplicationDomain]
    public void Each_test_run_only_once_in_app_domain(int number)
    {
      SingleCallTest.Set();
    }

    private static class SingleCallTest
    {
      private static bool IsSet { get; set; }

      public static void Set()
      {
        if (IsSet)
          throw new InvalidOperationException("Can only set once");

        IsSet = true;
      }
    }
  }
}