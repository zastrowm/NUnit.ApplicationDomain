[Nuget Package][nuget]

````
#!bash
PM> Install-Package NUnit.ApplicationDomain
````

[nuget]: https://nuget.org/packages/NUnit.ApplicationDomain

# Introduction

Sometimes when creating a library, you need to have a static initializer, property, or field.  Unfortunately, testing static things isn't easy - you can't easily control when the objects are created, and you can't tell them to "uncreate" themselves so you can test it again.

[NUnit.ApplicationDomain][nuget] attempts to solve this problem by running a test in a separate app domain, isolating the static initializer to only that domain.  At the end of the test, the app domain is destroyed and the next test can get a new app domain for a clean environment.

# How To

First, include the `NUnit.ApplicationDomain` nuget package.  Then either decorate your test method with the `RunInApplicationDomainAttribute` (preferred) or derive your test class from `AppDomainTestRunnerBase` (discouraged).

## RunInApplicationDomainAttribute

The `RunInApplicationDomain` attribute runs that test method in its separate app domain.  Just put it on your test method:

````
#!csharp
[Test, RunInApplicationDomain]
public void MyTest()
````

  If you want to specify the name of the application domain, you can do so by passing the name to the constructor:

````
#!csharp
[Test, RunInApplicationDomainAttribute("TestApplication")]
public void MyTest()
````

And that's all you have to do!

## AppDomainTestRunnerBase

Alternatively, if you don't like using attributes, you can derive your test class from `AppDomainTestRunnerBase`:

````
#!csharp
public class AllTheTests : AppDomainTestRunnerBase
````

Then in your test methods, call `RunInDomain`:

````
#!csharp
[Test]
public void TestMethod()
{
  RunInAppDomain()

  // this code runs in the app domain
}
````

Code before the call to `RunInDomain` runs in the main application domain and in the test application domain.  Only the code after the call to `RunInDomain` runs in the application domain.

# Gotchas

There are a couple things that you should know about the way the tests run:

* A setup method will be called if present in the class containing the test method
* The class containing the test method must be **public**
* The class containing the test method must have a parameterless constuctor
* Only the method and the setup method will be called.  Any extra NUnit parameters (such as ExpectedException, or RequiresSTA) will not be honored.
