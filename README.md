ASP.NET Core MVC
===

AppVeyor: [![AppVeyor](https://ci.appveyor.com/api/projects/status/969jbosi0qwc1awg/branch/dev?svg=true)](https://ci.appveyor.com/project/aspnetci/mvc/branch/dev)

Travis:   [![Travis](https://travis-ci.org/aspnet/Mvc.svg?branch=dev)](https://travis-ci.org/aspnet/Mvc)

ASP.NET Core MVC gives you a powerful, patterns-based way to build dynamic websites that enables a clean separation of concerns and gives you full control over markup for enjoyable, agile development. ASP.NET Core MVC includes many features that enable fast, TDD-friendly development for creating sophisticated applications that use the latest web standards.

ASP.NET Core MVC in ASP.NET Core includes support for building web pages and HTTP services in a single aligned framework that can be hosted in IIS or self-hosted in your own process.

Related community projects:
* [AspNet.Mvc.TypedRouting](https://github.com/ivaylokenov/AspNet.Mvc.TypedRouting): A collection of extension methods providing strongly typed routing and link generation for ASP.NET Core MVC projects.
* [ASP.NET MVC Boilerplate](https://visualstudiogallery.msdn.microsoft.com/6cf50a48-fc1e-4eaf-9e82-0b2a6705ca7d): Rich templates for ASP.NET Core MVC.
* [MyTested.AspNetCore.Mvc](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc): Powerful fluent testing framework for ASP.NET Core MVC.
* [MvcDeviceDetector](https://github.com/laskoviymishka/MvcDeviceDetector): Device detection mechanism to create mobile web applications.
* [XmlResult](https://github.com/Wallsmedia/XmlResult): XML formatter extensions to allow defining the XML serializer type.
* [AspNetCoreImageTagHelper](https://github.com/ignatandrei/AspNetCoreImageTagHelper): Tag helper for rendering images as inline base64 data.

This project is part of ASP.NET Core. You can find samples, documentation and getting started instructions for ASP.NET Core at the [Home](https://github.com/aspnet/home) repo.

## Building from source
 
1. Clone or fork this repository
2. Before opening this project in Visual Studio or VS Code, execute `build.cmd /t:Restore` (Windows) or `./build.sh /t:Restore` (Linux/macOS). This will execute only the part of the build script that downloads and initializes a few required build tools and packages. To run a complete build on command line only, execute `build.cmd` or `build.sh` without arguments.
3. You can then open the project in Visual Studio, Visual Studio Code, etc.

See [building from source](https://github.com/aspnet/Home/wiki/Building-from-source) for more information on building from source. If you would like to contribute, see [contributing information](https://github.com/aspnet/Home/blob/dev/CONTRIBUTING.md).
