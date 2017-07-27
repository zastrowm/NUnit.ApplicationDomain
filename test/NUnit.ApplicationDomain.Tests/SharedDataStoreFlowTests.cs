using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  internal class SharedDataStoreFlowTests1
  {
    private const string StringKey = "A.String.Key";
    private const string StringValue = "A.String.Value";

    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsNotInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set(StringKey, StringValue);
      }
    }

    [Test, RunInApplicationDomain]
    public void GetSerialiableValueInTest_Works()
    {
      var actualStringValue = AppDomainRunner.DataStore.Get<string>(StringKey);
      Assert.That(actualStringValue, Is.EqualTo(StringValue));
    }
  }

  internal class SharedDataStoreFlowTests2
  {
    private const string StringKey = "A.String.Key";
    private const string StringValue = "A.String.Value";

    [Test, RunInApplicationDomain]
    public void SetSerialiableValueInTest_Works()
    {
      AppDomainRunner.DataStore.Set(StringKey, StringValue);
    }

    [TearDown]
    public void GetSerialiableValueInTest_Works()
    {
      var actualStringValue = AppDomainRunner.DataStore.Get<string>(StringKey);
      Assert.That(actualStringValue, Is.EqualTo(StringValue));
    }
  }

  internal class SharedDataStoreFlowTests3
  {
    private const string MarshallKey = "A.Marshall.Key";

    [SetUp]
    public void Setup()
    {
      if (AppDomainRunner.IsNotInTestAppDomain)
      {
        AppDomainRunner.DataStore.Set(MarshallKey, new MarshallByRefFakeClass());
      }
    }

    [Test, RunInApplicationDomain]
    public void GetMarshallByValueInTest_Works()
    {
      var actualStringValue = AppDomainRunner.DataStore.Get<MarshallByRefFakeClass>(MarshallKey);
      Assert.That(actualStringValue, Is.Not.Null);
    }
  }

  // we currently do not have a good way of enforcing that MarshallByRef only comes from the parent domain
  // so for now we allow it and allow the user to shoot themselves
  // 
  //[RunInApplicationDomain]
  //internal class SharedDataStoreFlowTests4
  //{
  //  private const string MarshallKey = "A.Marshall.Key";

  //  [Test]
  //  public void SetMarshallValueInTest_DoesNotWorks()
  //  {
  //    Assert.Throws<InvalidOperationException>(
  //      () => AppDomainRunner.DataStore.Set(MarshallKey, new MarshallByRefFakeClass()));
  //  }
  //}

  public class MarshallByRefFakeClass : MarshalByRefObject
  {
  }
}