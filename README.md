# Xunit.Runner.Http
Enables you to run your tests via http requests.

## Usage
In order to use this library you'll need to turn your test project into `Microsoft.NET.Sdk.Web` project and add a `Program.cs` file that creates a web app. Then you'll need to:

Register the test runner with dependency injection:

`services.AddXunitHttpTestRunner(typoef(MyTestAssembly).Assembly);`

Tell the application to use the test runner:

`app.UseXunitHttpTestRunner();`

Now when you run the project you should get a swagger with all of your test methods listed as Get methods.