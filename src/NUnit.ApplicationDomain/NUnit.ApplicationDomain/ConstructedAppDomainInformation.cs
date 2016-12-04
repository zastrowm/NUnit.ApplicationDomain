using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.ApplicationDomain
{
  /// <summary>
  ///  Information about an app-domain constructed from an <see cref="IAppDomainFactory"/>.
  /// </summary>
  public class ConstructedAppDomainInformation
  {
    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <param name="owner"> The factory that constructed this instance. </param>
    /// <param name="appDomain"> The app domain to use for the test context. </param>
    public ConstructedAppDomainInformation(IAppDomainFactory owner, AppDomain appDomain)
    {
      if (owner == null)
        throw new ArgumentNullException(nameof(owner));

      Owner = owner;
      AppDomain = appDomain;
    }

    /// <summary> The factory that constructed this instance. </summary>
    public IAppDomainFactory Owner { get; }

    /// <summary> The app domain to use for the app-domain context. </summary>
    public AppDomain AppDomain { get; }
  }
}