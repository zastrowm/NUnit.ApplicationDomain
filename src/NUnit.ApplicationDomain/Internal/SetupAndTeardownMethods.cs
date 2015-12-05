using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NUnit.ApplicationDomain.Internal
{
  /// <summary> The setup and teardown methods to invoke before running a test. </summary>
  internal class SetupAndTeardownMethods : MarshalByRefObject
  {
    /// <summary> Constructor. </summary>
    /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
    /// <param name="setupMethods"> The setup methods for the current test. </param>
    /// <param name="teardownMethods"> The teardown methods for the current test. </param>
    public SetupAndTeardownMethods(IEnumerable<MethodBase> setupMethods, IEnumerable<MethodBase> teardownMethods)
    {
      if (setupMethods == null)
        throw new ArgumentNullException(nameof(setupMethods));
      if (teardownMethods == null)
        throw new ArgumentNullException(nameof(teardownMethods));

      SetupMethods = setupMethods;
      TeardownMethods = teardownMethods;
    }

    /// <summary> The setup methods for the current test. </summary>
    public IEnumerable<MethodBase> SetupMethods { get; }

    /// <summary> The teardown methods for the current test. </summary>
    public IEnumerable<MethodBase> TeardownMethods { get; }
  }
}