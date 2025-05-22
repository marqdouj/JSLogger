# JSLogger

## This library provides a simple way to log messages to the JavaScript console from Blazor, using JS Interop.
- It is not an implementation of ILogger, but rather mimics most methods of ILogger as async ValueTask.

### NuGet Package is [here](https://www.nuget.org/packages/Marqdouj.JSLogger/)

### Usage
See the 'Sandbox' demo in the 'src' folder for a working example.

### Release
- 8.5.0
  - Fixed issues with Visual Studio packaging.
  - The Module (jslogger.js) or Global (jslogger-bundled.js) script can be configured as a service that gets injected into your razor components. 
  See `App.Razor`, `Program.cs`,  and `LoggerDemo.razor` in the demo app (`Sandbox`) for configuration, etc.
- All previous versions have issues with the packaging; they will be delisted.
