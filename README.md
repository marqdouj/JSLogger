# JSLogger

## This library provides a simple way to log messages to the JavaScript console from Blazor, using JS Interop.
- It is not an implementation of ILogger, but rather mimics most methods of ILogger as async ValueTask.

### Usage
See the 'Sandbox' demo in the 'src' folder for a working example.

### Release
- **Under Investigation**
  - The "DiscoverPrecompressedAssets" issue was fixed, however this broke
  the JSLogger. For now, just configure the JSLoggerService and not JSLogger
- 8.3.0
  - Fixed issue with "DiscoverPrecompressedAssets" task failed unexpectedly error
  - Added methods to provide both the Module and Global script as a service 
  that can be injected into your razor components. See `App.Razor`, `Program.cs`,
  and `LoggerDemo.razor` in the demo app (`Sandbox`) for configuration, etc.
- 8.2.0
  - Resolved issue with `Dependencies` reference to `NPM` being shown in Visual Studio
  when adding package to a project.
- 8.1.1
  - Added `LogRaw` method to log raw (not formatted with template) messages with optional style.
    - i.e. `LogRaw("Hello World", "color: red; font-size: 20px;")`
- 8.1.0 
  - **Breaking Changes** 
    - Renamed `Logger` to `JSLogger` and `LoggerConfig` to `JSLoggerConfig`.
  - Added JSLoggerService that uses IJSRuntime directly via loaded script
rather than the IJSObjectReference module.
    - see `App.razor` for the script reference and `Program.cs` for the service registration.
    - NOTE: The loaded script could also be used directly in your own *.js file.
- 8.0.0 - Initial release for .NET 8

