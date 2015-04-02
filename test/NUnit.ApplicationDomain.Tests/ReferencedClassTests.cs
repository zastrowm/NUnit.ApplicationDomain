namespace NUnit.ApplicationDomain.Tests
{
  using NUnit.ApplicationDomain.Base.Tests;
  using NUnit.Framework;

  /// <summary>
  /// Tests referencing a class in a different assembly.
  /// </summary>
  [TestFixture]
  public class ReferencedClassTests
  {
    [Test]
    [RunInApplicationDomain]
    public void SetValuesInClassReferringToOtherAssembly()
    {
      var dc = new DerivedClass { DerivedValue = 1, BaseValue = 2 };

      Assert.That(dc.DerivedValue, Is.EqualTo(1));
      Assert.That(dc.BaseValue, Is.EqualTo(2));
    }

    private class DerivedClass : BaseClass
    {
      public int DerivedValue { get; set; }
    }
  }
}