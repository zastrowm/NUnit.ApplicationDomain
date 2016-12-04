using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.ApplicationDomain
{
  /// <summary> Allows the app-domain for given tests to be customized. </summary>
  public interface IAppDomainFactory
  {
    /// <summary> Retrieves the app domain to use for the given test. </summary>
    /// <param name="testMethodInfo"> Information about the method under test for which the appdomain
    ///  is being constructed. </param>
    /// <returns> The application domain that should be used for the given test. </returns>
    ConstructedAppDomainInformation GetAppDomainFor(TestMethodInformation testMethodInfo);

    /// <summary> Marks the app-domain as no longer used for the given context. </summary>
    /// <param name="constructedInfo"> The originally constructed app-domain. </param>
    void MarkFinished(ConstructedAppDomainInformation constructedInfo);
  }
}