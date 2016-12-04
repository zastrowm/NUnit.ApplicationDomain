using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using NUnit.ApplicationDomain.Internal;
using NUnit.Framework;

namespace NUnit.ApplicationDomain
{
  /// <summary>
  ///  A default app-domain factory that simply constructs a new app-domain before the test and
  ///  tears it down after every test.
  /// </summary>
  public class DefaultAppDomainFactory : IAppDomainFactory
  {
    /// <inheritdoc />
    public virtual ConstructedAppDomainInformation GetAppDomainFor(TestMethodInformation testMethodInfo)
    {
      var appDomainInfo = new AppDomainSetup
                          {
                            // At a minimum, we want the ability to load the types defined in this assembly
                            ApplicationBase = GetDirectoryToMyDll()
                          };

      if (!String.IsNullOrEmpty(testMethodInfo.AppConfigFile))
      {
        appDomainInfo.ConfigurationFile = testMethodInfo.AppConfigFile;
      }

      ConfigureAppDomain(appDomainInfo);

      var appDomain = AppDomain.CreateDomain(AppDomainRunner.TestAppDomainName,
                                             null,
                                             appDomainInfo,
                                             GetPermissionSet(testMethodInfo));

      return new ConstructedAppDomainInformation(this, appDomain);
    }

    /// <inheritdoc />
    public virtual void MarkFinished(ConstructedAppDomainInformation constructedInfo)
    {
      // if we don't unload, it's possible that execution continues in the AppDomain, consuming CPU/
      // memory.  See more info @ https://bitbucket.org/zastrowm/nunit.applicationdomain/pull-requests/1/ 
      AppDomain.Unload(constructedInfo.AppDomain);
    }

    /// <summary> Method that allows sub-classes to configure how the app-domain is setup. </summary>
    /// <param name="appDomainSetup"> The AppDomainSetup that will be used to construct the instance. </param>
    protected virtual void ConfigureAppDomain(AppDomainSetup appDomainSetup)
    {
    }

    /// <summary> Gets the permission set for the constructed app-domains. </summary>
    /// <returns> The permission set to use for constructed app-domains. </returns>
    protected virtual PermissionSet GetPermissionSet(TestMethodInformation context)
    {
      return new PermissionSet(PermissionState.Unrestricted);
    }

    /// <summary> Gets the directory where the dlls for NUnit.Application.Domain can be found. </summary>
    private static string GetDirectoryToMyDll()
    {
      return Path.GetDirectoryName(new Uri(typeof(InDomainTestMethodRunner).Assembly.EscapedCodeBase).LocalPath);
    }
  }
}