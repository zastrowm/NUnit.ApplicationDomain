using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace NUnit.ApplicationDomain.Tests
{
  public class STAThreadVerificationTest
  {
    [Test, Apartment(ApartmentState.STA)]
    [RunInApplicationDomain]
    public void VerifySTA()
    {
      Assert.That(Thread.CurrentThread.GetApartmentState(), Is.EqualTo(ApartmentState.STA));
    }
  }
}