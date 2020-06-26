[![NuGet](https://img.shields.io/nuget/v/NUnit.ApplicationDomain.svg)](https://www.nuget.org/packages/NUnit.ApplicationDomain)

````bash
PM> Install-Package NUnit.ApplicationDomain
````

*Now on [GitHub][] instead of [Bitbucket][]!*

[bitbucket]: https://bitbucket.org/zastrowm/nunit.applicationdomain_bitbucket
[github]: https://github.com/zastrowm/NUnit.ApplicationDomain
[nuget]: https://www.nuget.org/packages/NUnit.ApplicationDomain

# Introduction

Sometimes when creating a library, you need to have a static initializer, property, or field. Unfortunately, testing static things isn't easy - you can't easily control when the objects are created, and you can't tell them to "uncreate" themselves so you can test it again.

[NUnit.ApplicationDomain][nuget] attempts to solve this problem by running a test in a separate app domain, isolating the static initializer to only that domain.  At the end of the test, the app domain is destroyed and the next test can get a new app domain for a clean environment.

# How To

First, include the `NUnit.ApplicationDomain` nuget package.  Then decorate your test method with the `RunInApplicationDomainAttribute`.

** *Important Note* **

If you're not using the same version of NUnit as the library expects (3.7.0) you need to apply a binding redirect via `app.config` to your test library.  If you do not, test runners may report 0 test executions.

Example `app.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="nunit.framework"
                          publicKeyToken="2638cd05610744eb"/>
        <bindingRedirect oldVersion="3.7.0.0" newVersion="3.12.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## RunInApplicationDomainAttribute

The `RunInApplicationDomain` attribute runs that test method in its separate app domain.  Just put it on your test method:

````csharp
[Test, RunInApplicationDomain]
public void MyTest()
````

## AppDomainRunner.IsInTestAppDomain

Use the `AppDomainRunner.IsInTestAppDomain` method to detect if code with side-effects should be executed. 
This is especially useful in the setup and teardown methods, as those methods are invoked both in the "normal"
app domain and the "test" app domain:

````csharp
[SetUp]
public void Setup()
{
  // if we're not in the app domain, do not run the setup code
  if (!AppDomainRunner.IsInTestAppDomain)
    return;
    
  File.WriteAllText("fake.txt", "a file");
}
````

## AppDomainRunner.DataStore

Use `AppDomainRunner.DataStore` as a way of passing data back and forth between the "normal" app domain and the "test" app domain:


````csharp
internal class TestContextTests
{
  [SetUp]
  public void Setup()
  {
    // accessing TestContext.CurrentContext.TestDirectory from the app domain will throw an
    // exception but we need the test directory, so we pass it in via the data store. 
    if (!AppDomainRunner.IsInTestAppDomain)
    {
      AppDomainRunner.DataStore.Set("TestDirectory", TestContext.CurrentContext.TestDirectory);
    }
  }

  [Test, RunInApplicationDomain]
  public void VerifyItFails()
  {
    var testDirectory = AppDomainRunner.DataStore.Get<string>("TestDirectory");
    Console.WriteLine($"The test directory is: {testDirectory}");

    // we can also pass data back into the "normal" domain
    AppDomainRunner.DataStore.Set("ShouldBeSetFromAppDomain", testDirectory);
  }

  [TearDown]
  public void Teardown()
  {
    // okay, make sure everything worked properly now
    if (!AppDomainRunner.IsInTestAppDomain)
    {
      var testDirectory = AppDomainRunner.DataStore.Get<string>("ShouldBeSetFromAppDomain");

      // this should have been set by the test app-domain
      Assert.That(testDirectory, Is.EqualTo(TestContext.CurrentContext.TestDirectory));
    }
  }
}
````

This can be especially useful for verifying that something occurred in the app domain or for adding information that can't be accessed from the test app domain such as the members on `TestContext.CurrentContext`.

# Gotchas

There are a couple things that you should know about the way the tests run:

* The class containing the test method must be **public**
* Only the test method, the setup method, and the test method will be called.  Any extra NUnit parameters (such as ExpectedException, or RequiresSTA) will not be honored (if you want/needs support of this, create an issue).
* The setup and teardown methods are invoked **both normally and in the app domain**.  This results in the setup and teardown methods being called twice. It is advised to use `AppDomainRunner.IsInTestAppDomain` property to mitigate this problem.

## Tests Returning Tasks

`NUnit 3.0` introduced [better] support for async tests (tests that return a Task).  The way `NUnit.ApplicationDomain` is implemented requires the library to somehow block until the test is completed before running the TearDown.  It does this [by default] by invoking `.Wait()` on the returned task.

If you need to block via some other mechnism (for example using a `Dispatcher`, or pumping some sort of message thread) you can implement `IAsyncTestResultHandler` on your test class and then block using your own mechnism:

```csharp
[Test]
public async Task WaitsForDispatcherToContinue()
{
  // pretend that for some made-up reason, we need to be in the event loop 
  await Dispatcher.Yield();

  // and pretend that something later triggers that allows us to complete
  await Task.Delay(TimeSpan.FromSeconds(3));

  AppDomainRunner.DataStore.Set<bool>("ran", true);
}

[TearDown]
public void Teardown()
{
  Assert.That(AppDomainRunner.DataStore.Get<bool>("ran"), Is.True);
}

/// <inheritdoc />
void IAsyncTestResultHandler.Process(Task task)
{
  // if we just simply did task.Wait(), we would block indefinitely because no-one is message
  // pumping. 

  // instead, tell the dispatcher to run until the task has resolved
  var frame = new DispatcherFrame();
  task.ContinueWith(_1 => frame.Continue = false);
  Dispatcher.PushFrame(frame);

  // propagate any exceptions
  task.Wait();
}
```

Full test/example is [implemented as a test](test/NUnit.ApplicationDomain.Tests/AsyncTestWithDispatcherRunner.cs).
