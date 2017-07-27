using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public class FactoryTests
  {
    [Test, RunInApplicationDomain(AppDomainFactory = typeof(AppDomainFactoryImplementation))]
    public void VerifyFactoryIsInvoked()
    {
      var data = AppDomain.CurrentDomain.GetData("Example-Data") as bool?;
      Assert.That(data, Is.True);
    }

    public class AppDomainFactoryImplementation : PerTestAppDomainFactory
    {
      protected override AppDomain ConstructAppDomain(TestMethodInformation testMethodInfo, AppDomainSetup appDomainInfo)
      {
        var appDomain = base.ConstructAppDomain(testMethodInfo, appDomainInfo);
        appDomain.SetData("Example-Data", true);
        return appDomain;
      }
    }
  }
}