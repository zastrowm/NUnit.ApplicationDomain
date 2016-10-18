using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary> Runs a TestMethodInformation in a child app domain. </summary>
  internal static class ParentAppDomainRunner
  {
    /// <summary> The setup/teardown methods that have been cached for each type thus far. </summary>
    private static readonly ConcurrentDictionary<Type, SetupAndTeardownMethods> CachedInfo;

    /// <summary> Static constructor. </summary>
    static ParentAppDomainRunner()
    {
      CachedInfo = new ConcurrentDictionary<Type, SetupAndTeardownMethods>();
    }

    /// <summary> Runs the given test for the given type under a new, clean app domain. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <param name="typeUnderTest"> The type that is currently under test. </param>
    /// <param name="testMethod"> The test method to invoke as the test. </param>
    /// <param name="fixtureArguments"> The arguments to use when constructing the test fixture. </param>
    /// <returns>
    ///  The exception that occurred while executing the test, or null if no exception was generated.
    /// </returns>
    public static Exception Run(Type typeUnderTest, MethodInfo testMethod, object[] fixtureArguments)
    {
      if (testMethod == null)
        throw new ArgumentNullException(nameof(testMethod));
      if (typeUnderTest == null)
        throw new ArgumentNullException(nameof(typeUnderTest));

      var setupAndTeardown = GetSetupTeardownMethods(typeUnderTest);

      var methodData = new TestMethodInformation(typeUnderTest,
                                                 testMethod,
                                                 setupAndTeardown,
                                                 AppDomainRunner.DataStore,
                                                 fixtureArguments);

      var info = new AppDomainSetup
                 {
                   // At a minimum, we want the ability to load the types defined in this assembly
                   ApplicationBase = GetDirectoryToMyDll()
                 };

      if (!String.IsNullOrEmpty(methodData.AppConfigFile))
      {
        info.ConfigurationFile = methodData.AppConfigFile;
      }

      AppDomain domain = AppDomain.CreateDomain(AppDomainRunner.TestAppDomainName,
                                                null,
                                                info,
                                                GetPermissionSet());

      // Add an assembly resolver for resolving any assemblies not known by the test application domain.
      var assemblyResolver = new InDomainAssemblyResolver(new ResolveHelper());
      domain.AssemblyResolve += assemblyResolver.ResolveEventHandler;

      domain.Load(methodData.TypeUnderTest.Assembly.GetName());

      var inDomainRunner = domain.CreateInstanceAndUnwrap<InDomainTestMethodRunner>();

      // Store any resulting exception from executing the test method
      var possibleException = inDomainRunner.Execute(methodData);

      // if we don't unload, it's possible that execution continues in the AppDomain, consuming CPU/
      // memory.  See more info @ https://bitbucket.org/zastrowm/nunit.applicationdomain/pull-requests/1/ 
      AppDomain.Unload(domain);

      return possibleException;
    }

    private static string GetDirectoryToMyDll()
    {
      return Path.GetDirectoryName(new Uri(typeof(InDomainTestMethodRunner).Assembly.EscapedCodeBase).LocalPath);
    }

    /// <summary> Gets the setup and teardown methods for the given type. </summary>
    /// <param name="typeUnderTest"> The type under test. </param>
    /// <returns>
    ///  The setup teardown methods, loaded from the cache if it already existed, otherwise queried
    ///  via reflection.
    /// </returns>
    private static SetupAndTeardownMethods GetSetupTeardownMethods(Type typeUnderTest)
    {
      SetupAndTeardownMethods setupAndTeardown;

      if (CachedInfo.TryGetValue(typeUnderTest, out setupAndTeardown))
        return setupAndTeardown;

      // get all of the setup methods in the type
      var setupMethods = typeUnderTest.GetMethodsWithAttribute<OneTimeSetUpAttribute>();
      setupMethods.AddRange(typeUnderTest.GetMethodsWithAttribute<SetUpAttribute>());

      // we want most-derived last
      setupMethods.Reverse();

      // get all of the teardown methods in the type (it is already the way we want it).
      var teardownMethods = typeUnderTest.GetMethodsWithAttribute<OneTimeTearDownAttribute>();
      teardownMethods.AddRange(typeUnderTest.GetMethodsWithAttribute<TearDownAttribute>());

      setupAndTeardown = new SetupAndTeardownMethods(setupMethods, teardownMethods);
      CachedInfo.TryAdd(typeUnderTest, setupAndTeardown);

      return setupAndTeardown;
    }

    /// <summary>
    /// create a permission set
    /// </summary>
    private static PermissionSet GetPermissionSet()
    {
      return new PermissionSet(PermissionState.Unrestricted);
    }
  }
}