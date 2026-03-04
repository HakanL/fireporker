# .NET Framework 4.6 to .NET 8 Migration - Complete

## Migration Summary

The FirePorker planning poker application has been successfully migrated from .NET Framework 4.6 to .NET 8.

### What Changed

#### 1. Project File (.csproj)
- **Before**: Old-style XML project file with package references, content includes, and compile items
- **After**: Modern SDK-style project file with minimal configuration
  ```xml
  <Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
      <TargetFramework>net8.0</TargetFramework>
      <Nullable>enable</Nullable>
      <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>
    <ItemGroup>
      <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
    </ItemGroup>
  </Project>
  ```

#### 2. Application Startup
- **Before**: Global.asax, Startup.cs with OWIN, App_Start folder with route/bundle/filter configs
- **After**: Program.cs with minimal hosting model
  ```csharp
  var builder = WebApplication.CreateBuilder(args);
  builder.Services.AddControllersWithViews();
  builder.Services.AddSignalR();
  builder.Services.AddMemoryCache();
  
  var app = builder.Build();
  app.UseStaticFiles();
  app.UseRouting();
  app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
  app.MapHub<PokerHub>("/signalr/hubs");
  app.Run();
  ```

#### 3. SignalR Migration (Major Change)
**ASP.NET SignalR 2.x → ASP.NET Core SignalR**

**Server Side:**
- Changed from `Hub` (Microsoft.AspNet.SignalR) to `Hub` (Microsoft.AspNetCore.SignalR)
- Methods now return `Task` and use `async/await`
- `Clients.Group(id).method(args)` → `await Clients.Group(id).SendAsync("method", args)`
- `Groups.Add(connectionId, group)` → `await Groups.AddToGroupAsync(connectionId, group)`

**Client Side (JavaScript):**
```javascript
// Before (SignalR 2.x):
var hub = $.connection.pokerHub;
hub.server.playerVoted(gameId, playerId, points);
hub.client.playerVoted = function(playerId, points) { ... };
$.connection.hub.start().done(function() { ... });

// After (ASP.NET Core SignalR):
var connection = new signalR.HubConnectionBuilder()
  .withUrl("/signalr/hubs")
  .withAutomaticReconnect()
  .build();
connection.invoke("PlayerVoted", gameId, playerId, points);
connection.on("playerVoted", function(playerId, points) { ... });
connection.start().then(function() { ... });
```

#### 4. Controllers
- **Before**: `System.Web.Mvc.Controller`, `ActionResult`, `FormCollection`
- **After**: `Microsoft.AspNetCore.Mvc.Controller`, `IActionResult`, explicit parameter binding
- Cookie handling changed from `HttpCookie` to `CookieOptions`
- Dependency injection for `IHubContext<PokerHub>`

#### 5. Models
- Updated `GameManager` from `System.Runtime.Caching.MemoryCache` to `Microsoft.Extensions.Caching.Memory.IMemoryCache`
- Added nullable reference type annotations
- File-scoped namespace declarations

#### 6. Views
- Added `_ViewImports.cshtml` for tag helpers
- Updated `_Layout.cshtml` to use tag helpers (`asp-controller`, `asp-action`)
- Removed Web.Optimization bundles
- Direct script/style references from wwwroot
- JSON serialization uses `System.Text.Json`

#### 7. Static Files
- Moved from root to `wwwroot/` directory
  - `Content/` → `wwwroot/Content/`
  - `Scripts/` → `wwwroot/Scripts/`
  - `fonts/` → `wwwroot/fonts/`
- Added ASP.NET Core SignalR client library

#### 8. Configuration
- **Before**: `Web.config` with appSettings and system.web sections
- **After**: `appsettings.json` with structured configuration

### Files Removed
- `Global.asax` / `Global.asax.cs`
- `Startup.cs` (OWIN)
- `App_Start/` folder (BundleConfig, RouteConfig, FilterConfig)
- `packages.config`
- `Web.config` / `Web.Debug.config` / `Web.Release.config`
- `Properties/AssemblyInfo.cs`
- `Views/Web.config`

### Files Added
- `Program.cs` - Application entry point
- `appsettings.json` / `appsettings.Development.json`
- `Views/_ViewImports.cshtml`
- `Dockerfile` and `.dockerignore`
- `wwwroot/lib/signalr/signalr.min.js`

### Testing the Migration

#### Local with .NET SDK
```bash
cd PlanningPoker
dotnet restore
dotnet build
dotnet run
```

Access at: https://localhost:5001

#### Docker
```bash
docker build -t fireporker .
docker run -p 8080:8080 fireporker
```

Access at: http://localhost:8080

### Functionality Preserved
✅ Real-time planning poker voting  
✅ Player join/leave notifications  
✅ Story creation and estimation  
✅ Vote reveal when all players voted  
✅ Host tools (clear votes, accept estimate)  
✅ Cookie-based session tracking  
✅ In-memory game state management  
✅ Responsive UI with Knockout.js  

### Known Differences
1. **Case sensitivity**: JSON serialization in .NET 8 is case-sensitive by default. Updated view bindings from PascalCase to camelCase where needed.
2. **Cookie format**: Changed from multi-value cookie to pipe-delimited string for simplicity with new cookie API.
3. **SignalR reconnection**: ASP.NET Core SignalR handles reconnection slightly differently but more robustly with `.withAutomaticReconnect()`.

### Next Steps (Optional Enhancements)
- Migrate from in-memory cache to distributed cache (Redis) for multi-instance deployment
- Add health checks and metrics
- Implement authentication/authorization
- Add database persistence for game history
- Update JavaScript libraries (jQuery, Knockout) to modern alternatives
- Add unit and integration tests
- Configure HTTPS properly for production

### Resources
- [ASP.NET Core SignalR Migration Guide](https://learn.microsoft.com/en-us/aspnet/core/signalr/migrate)
- [Migrate from ASP.NET to ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/)
- [.NET 8 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)

---
**Migration completed**: March 4, 2026  
**Target Framework**: .NET 8.0  
**Branch**: feature/dotnet8-migration
