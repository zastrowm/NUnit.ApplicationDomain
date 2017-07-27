using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary> Runs a TestMethodInformation in a child app domain. </summary>
  internal static class ParentAppDomainRunner
  {
    /// <summary> The setup/teardown methods that have been cached for each type thus far. </summary>
    private static readonly ConcurrentDictionary<Type, SetupAndTeardownMethods> CachedInfo
      = new ConcurrentDictionary<Type, SetupAndTeardownMethods>();

    private static readonly PerTestAppDomainFactory DefaultFactory
      = new PerTestAppDomainFactory();

    /// <summary> Runs the given test for the given type under a new, clean app domain. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
    ///  illegal values. </exception>
    /// <param name="test"> The test that should be run in another app-domain. </param>
    /// <param name="appDomainFactoryType"> The type of factory to use to construct app-domains. </param>
    /// <returns>
    ///  The exception that occurred while executing the test, or null if no exception was generated.
    /// </returns>
    public static Exception Run(ITest test, Type appDomainFactoryType)
    {
      if (test == null)
        throw new ArgumentNullException(nameof(test));

      var appDomainFactory = ConstructFactory(appDomainFactoryType);

      var typeInfo = test.Fixture != null
        ? test.TypeInfo
        : test.Method.TypeInfo;

      if (typeInfo == null)
        throw new ArgumentException("Cannot determine the type that the test belongs to");

      var setupAndTeardown = GetSetupTeardownMethods(typeInfo.Type);

      var testArguments = CurrentArgumentsRetriever.GetTestArguments(test);
      var testFixtureArguments = CurrentArgumentsRetriever.GetTestFixtureArguments(test);

      var methodData = new TestMethodInformation(typeInfo.Type,
                                                 test.Method.MethodInfo,
                                                 setupAndTeardown,
                                                 AppDomainRunner.DataStore,
                                                 testArguments,
                                                 testFixtureArguments);

      var domainInfo = appDomainFactory.GetAppDomainFor(methodData);
      var domain = domainInfo.AppDomain;

      // Add an assembly resolver for resolving any assemblies not known by the test application domain.
      var assemblyResolver = new InDomainAssemblyResolver(new ResolveHelper());
      domain.AssemblyResolve += assemblyResolver.ResolveEventHandler;

      domain.Load(methodData.TypeUnderTest.Assembly.GetName());

      var inDomainRunner = domain.CreateInstanceAndUnwrap<InDomainTestMethodRunner>();

      // Store any resulting exception from executing the test method
      var possibleException = inDomainRunner.Execute(methodData);

      domainInfo.Owner.MarkFinished(domainInfo);

      return possibleException;
    }

    /// <summary>
    ///  Construct the <see cref="IAppDomainFactory"/> from the given type, throwing out if the type
    ///  is not an instance of
    ///  <see cref="IAppDomainFactory"/>.
    /// </summary>
    private static IAppDomainFactory ConstructFactory(Type typeToConstruct)
    {
      if (typeToConstruct == null)
        return DefaultFactory;

      var instance = Activator.CreateInstance(typeToConstruct);
      var factory = instance as IAppDomainFactory;
      if (factory == null)
        throw new InvalidOperationException(
                $"Cannot specify an AppDomainFactory that is not an instance of ${nameof(IAppDomainFactory)}");

      return factory;
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