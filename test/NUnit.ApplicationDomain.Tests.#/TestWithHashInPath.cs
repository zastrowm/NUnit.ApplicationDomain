using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HashInNameTests
{
  public class TestWithHashInPath
  {
    [Test, RunInApplicationDomain]
    [Description("Run in application domain")]
    public void Run_in_application_domain()
    {
      Console.WriteLine("Success");
    }
  }
}