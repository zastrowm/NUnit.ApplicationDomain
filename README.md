[Nuget Package][nuget]

````
#!bash
PM> Install-Package NUnit.ApplicationDomain
````

[nuget]: https://nuget.org/packages/NUnit.ApplicationDomain

# Introduction

Sometimes when creating a library, you need to have a static initializer, property, or field.
Unfortunately, testing static things isn't easy - you can't easily control when the objects
are created, and you can't tell them to "uncreate" themselves so you can test it again.

[NUnit.ApplicationDomain][nuget] attempts to solve this problem by running a test in a
separate app domain, isolating the static initializer to only that domain.  At the end of 
the test, the app domain is destroyed and the next test can get a new app domain for a clean environment.

# How To

First, include the `NUnit.ApplicationDomain` nuget package.  Then decorate your test method with the `RunInApplicationDomainAttribute`.

## RunInApplicationDomainAttribute

The `RunInApplicationDomain` attribute runs that test method in its separate app domain.  Just put it on your test method:

````
#!csharp
[Test, RunInApplicationDomain]
public void MyTest()
````

## AppDomainRunner.IsInTestAppDomain

Use the `AppDomainRunner.IsInTestAppDomain` method to detect if code with side-effects should be executed. 
This is especially useful in the setup and teardown methods, as those methods are invoked both in the "normal"
appdomain and the "test" app domain:

````
#!csharp
[SetUp]
public void Setup()
{
  // if we're not in the app domain, do not run the setup code
  if (!AppDomainRunner.IsInTestAppDomain)
    return;
    
  File.WriteAllText("fake.txt", "a file");
}
````

# Gotchas

There are a couple things that you should know about the way the tests run:

* The class containing the test method must be **public**
* The class containing the test method must have a **parameterless constuctor**
* Only the test method, the setup method, and the test method will be called.  Any extra NUnit parameters (such as
  ExpectedException, or RequiresSTA) will not be honored (if you want/needs support of this, create an issue).
* The setup and teardown methods are invoked **both normally and in the app domain**.  This results in the setup and
  teardown methods being called twice. It is advised to use `AppDomainRunner.IsInTestAppDomain` property to mitigate
  this problem.

# (Obsolete) AppDomainTestRunnerBase

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

