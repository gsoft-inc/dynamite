Dynamite for SharePoint 2013
============================

A C# toolkit, PowerShell cmdlets and a WSP solution package to help you build maintainable SharePoint 2013 farm solutions (full-trust, on-premise).


NuGet Feeds
===========

Subscribe to the stable Dynamite 2013 public [MyGet.org](http://www.myget.org) feed: 

* [NuGet v2 - VS 2012+](https://www.myget.org/F/dynamite-2013/api/v2)
* [NuGet v3 - VS 2015+](https://www.myget.org/F/dynamite-2013/api/v3/index.json)

Pre-release builds [are available from a separate feed](https://github.com/GSoft-SharePoint/Dynamite/wiki/Installing-the-Dynamite-NuGet-packages-from-our-MyGet.org-feeds).

Two main NuGet packages are available:

1. **GSoft.Dynamite**
    * C# library (DLL) with facilities for:
        * Dependency injection with Autofac
        * SharePoint object provisioning (fields, content types, lists)
        * Logging and globalization (i18n) 
        * SPListItem-to-entity mapping
        * etc.
    * Dependencies: 
        * Autofac, Newtonsoft.Json
    * Should be added to *every project* in Visual Studio 


2. **GSoft.Dynamite.SP**
    * Full-trust solution package (WSP) ready to deploy to your on-premise farm 
        * Provisions the DLL from the GSoft.Dynamite package (see 1. above) to the GAC
        * Deploy the WSP solution with `Add-SPSolution` and `Deploy-SPSolution` from a SharePoint Management Shell
    * PowerShell cmdlets module to help with provisioning
        * Will turn all your PowerShell shells into SharePoint Management Shell and register a Dynamite module of cmdlets
        * Install through `.\tools\Install-DSPModule.ps1`, review set of cmdlets with `Get-DSPCommand`
    * Should be installed *only once globally* in your Visual Studio solution
        * See [more detailed instructions for solution-wide package installation here](https://github.com/GSoft-SharePoint/Dynamite/wiki/Installing-the-Dynamite-NuGet-packages-from-our-MyGet.org-feeds#solution-wide-package-gsoftdynamitesp) 
        

Continuous Integration
======================

TeamCity builds are triggered on GSOFT's private build servers upon every commit to the repository. 

Commits to the `master` branch will generate new packages on the [stable MyGet feed](https://www.myget.org/F/dynamite-2013/api/v2) ([link to Master build on GSOFT's private Team City build environment](https://teamcity.gsoft.com/viewType.html?buildTypeId=bt71)).

Commits to `develop` will publish packages on the [pre-release feed](https://www.myget.org/F/dynamite-2013-dev/api/v2) instead ([link to Dev build on Team City](https://teamcity.gsoft.com/viewType.html?buildTypeId=bt69)).

The [full C# integration test suite](https://github.com/GSoft-SharePoint/Dynamite/tree/develop/Source/GSoft.Dynamite.IntegrationTests) runs on [a nightly build (GSOFT-only)](https://teamcity.gsoft.com/viewType.html?buildTypeId=Dynamite_Dynamite2013_Dynamite2013PublicCore_Dynamite2013DevelopNightly).


Target Audience & Philosophy
============================

Dynamite is meant exclusively for On-Premise, full-trust, server-side, custom SharePoint 2013 (.NET 4.5) solution development.

Its purpose is to encourage:

* A correct approach to **service location** (dependency injection, inversion of control) **using Autofac** as its core container framework within the (particularily hairy) context of GAC-deployed SharePoint WSP solution packages
* **Repeatable, idempotent** SharePoint artefact **provisioning** sequences (site columns, content types, lists)
* Less code repetition for typical **logging, internationalization** and SPListItem-to-business-entity mapping scenarios
* Loosely coupled, easily unit tested, **modular, extensible architectures**
* Environment-independent, **fully automated installation procedures** with PowerShell

Dynamite can be though of as an embodiment or spiritual successor to [Microsoft's patterns and practices team's famous SharePoint 2010 development guide](http://msdn.microsoft.com/en-us/library/ff770300.aspx).

Thus, the toolkit is firmly old-school in its **purely server-side/on-premise** approach. New development efforts outside of a full-trust 
context (e.g. Office 365, app model development, client-side, etc.) should probably look into alternatives such as the more recent [Office PnP](https://github.com/OfficeDev/PnP) 
project and its remote provisioning approach.

> You can think of Dynamite as a *batteries-included, SharePoint-aware, architecture-opinionated, intrastructure-level .NET & PowerShell toolkit* meant 
> as a building block for maintainable and automated SharePoint 2013 server-side, full-trust solutions


Quick Start Guide
=================

The Dynamite toolkit covers a lot of ground. Here are a few guidelines to get you up and running on these topics:

* [Dependency injection & service location](#dependency-injection--service-location)
   * Using Autofac correctly in a SharePoint server context
* [Using Dynamite's provisioning utilities](#using-dynamites-provisioning-utilities)
   * Creating fields, content types and lists in an idempotent way
* [Other utilities: logging and globalization](#)
* [The SharePoint entity binder: easy mappings from entities to SPListItems and back](#)


Dependency injection & service location
---------------------------------------

One main objective of the Dynamite toolkit is to guide you in the implementation of a dependency injection container - without having 
to worry too much about the particulars on how to do it "right" in a SharePoint on-premise, full-trust solution context. 

This container at the root of your application will serve as an intermediary when your components need to depend on other modules,
services and utilities. It is the "glue" that takes care of constructing concrete C# object instances while your own consumer code
only depends on interface contracts. This makes it easier to depend on other modules without having to worry about their own dependencies
and implementation details.


### Building your first Autofac container for service location

Access to Dynamite's C# utilities is enabled through service locator-style dependency injection.

Start by creating your own application container like so:

```
using Autofac
using GSoft.Dynamite.ServiceLocator

namespace Company.Project.ServiceLocator
{
    //
    // The Container that is used by UI-tier components for dependency
    // injection across all Company.Project.*.WSP projects (perhaps shared 
    // via a common Company.Project.ServiceLocator.DLL class library)
    //
    public static class ProjectContainer
    {
        //
        // The key that distinguishes your container from others in the same AppDomain
        // (i.e. within the same SharePoint web application AppDomain you can use more than
        // one root service locators configured with a different set of GAC-loaded dependencies)
        //
        private const string AppName = "Company.Project";

        //
        // The locator will scan the Global Assembly Cache and load the following Autofac 
        // registration modules:
        // - the core Dynamite utilities registration module
        // - all Autofac registration modules from assemblies with a filename that starts 
        //   with the AppName prefix "Company.Project"
        //
        private static ISharePointServiceLocator singletonLocatorInstance = new SharePointServiceLocator(AppName);

        //
        // Creates a new Autofac child injection scope from the current context, from which 
        // you can .Resolve<IFoo>() your dependencies.
        // Typically used from user control (.ASCX) or application page (.ASPX) code-behind 
        // in a HTTP-request context, which will allow you to resolve objects registered
        // as .InstancePerRequest (see below).
        //
        public static ILifetimeScope BeginLifetimeScope()
        {
            return singletonLocatorInstance.BeginLifetimeScope();
        }

        //
        // Creates a new Autofac child injection scope from the current Site or Web-scoped 
        // feature context.
        // Will not allow you to .Resolve objects registered as .InstancePerRequest (see below).
        //
        public static ILifetimeScope BeginLifetimeScope(SPFeature featureContext)
        {
            return singletonLocatorInstance.BeginLifetimeScope(featureContext);
        }
    }
}
```

Note how `SharePointServiceLocator` will scan the GAC for assembly file names that begin with the prefix `Company.Project*`.

You can specify your own alternate GAC-scanning logic if you want:

```
private static ISharePointServiceLocator singletonLocatorInstance = new SharePointServiceLocator(
    AppName,
    assemblyFileName =>
    {
        assemblyFileName.Contains(AppName) || assemblyFileName.Contains("My.Other.Dependecy.Namespace");
    });
```

All assemblies matching your condition will be loaded from the GAC_MSIL and scanned for [Autofac registration modules](http://docs.autofac.org/en/latest/configuration/modules.html)
such as the one in the example below.


###Registering your interface-to-implementation configuration as Autofac registration modules

Your Autofac service locator will scan the GAC for assemblies in search of **registration modules**.

One of your own registration modules may look like this:

```
using Autofac;
using GSoft.Dynamite.ServiceLocator.Lifetime;   // VERY IMPORTANT to import this instead of relying on the default Autofac.RegistrationExtensions

namespace Company.Project.SubProject
{
    public class MySubProjectDependecyRegistrations : Module
    {
        public override Load(ContainerBuilder builder)
        {
            //
            // A simple TRANSIENT lifetime registration
            // (a new object will be constructed upon every Resolve)
            //
            builder.Register<MySubProjectService>().As<IMyProjectService>();
            builder.RegisterType<MySiteCreator>().As<IMySiteCreator>()
            
            //
            // A transient, NAMED config repository registration 
            // (with an example of how to hook up a DECORATOR with Autofac, nifty!)
            //
            builder.RegisterType<ConfigRepository>().Named<IConfigRepository>("implementation");
            builder.RegisterDecorator<IConfigRepository>((c, inner) => new ElevatedSecurityConfigRepository(inner), fromKey: "implementation");

            //
            // An application-wide SINGLETON registration 
            // (available from the entire web application's AppDomain)
            //
            builder.RegisterType<MyGodClass>().As<IGod>().SingleInstance();

            //
            // Only one object instance of the following class will be created
            // by the container per SPSite instance. This allows you to isolate site
            // collection-specific behaviors through ONE-INSTANCE-PER-SPSITE semantics.
            //
            builder.RegisterType<MySiteCollectionSpecificCache>().As<ISiteCollectionSpecificCache>().InstancePerSite();

            //
            // Similarly, you can register SPWeb-scoped dependencies (i.e. ONE-INSTANCE-PER-SPWEB). 
            // SPWeb-bound lifetime scopes are children of their parent SPSite lifetime scopes 
            // (allowing you to depend on .InstancePerSite instances from your classes registered
            // with InstancePerWeb).
            //
            builder.RegisterType<MySubWebSpecificCache>().As<ISubWebSpecificCache().InstancePerWeb();

            //
            // Example of how to implement ONE-OBJECT-INSTANCE-PER-HTTP-REQUEST behavior.
            // Objects injected through an .IntancePerRequest configuration can depend on instances
            // from the current parent SPWeb (.InstancePerWeb) and SPSite (.InstancePerSite) scopes.
            //
            builder.RegisterType<MyHttpRequestCache>().As<IHttpRequestCache>().InstancePerRequest();
        }
    }
}
```

Note how **custom object lifetime behavior** can be configured to obtain *singleton-per-SPSite*, *singleton-per-SPWeb* and *per-HTTP-request* 
semantics through Dynamite's custom RegistrationExtensions. Please refer to the Dynamite wiki for 
[more detailed help on using service location and complex lifetime scope hierarchies](https://github.com/GSoft-SharePoint/Dynamite/wiki#1-a-modular-approach-to-building-sharepoint-farm-solutions-with-dynamite-and-autofac).

Make sure to brush up on the concept of [Lifetime Scopes](http://docs.autofac.org/en/v3.5.2/lifetime/index.html) if you haven't 
yet understood their power. Dynamite's `SharePointServiceLocator` and custom `InstancePerSite`, `InstancePerWeb` and `InstancePerRequest` lifetimes 
are meant to easily provide such fine-grained object scoping mechanics in a correct way within a full-trust SharePoint server context.

> To enable `InstancePerRequest` behavior, you need to configure a HttpModule in your server's `web.config`. 
>
> Do this by enabling the WebApplication-scope feature `GSoft.Dynamite.SP_Web Config Modifications` (ID: `2f59e5c1-448c-42ee-a782-4beac0a30370`) available from the `GSoft.Dynamite.wsp` solution package (from NuGet package GSoft.Dynamite.SP).
>
> Without the `GSoft.Dynamite.ServiceLocator.Lifetime.RequestLifetimeHttpModule` configured through this feature activation, objects will not be disposed properly at the end of each request.


### Dynamite's own registration module

The class `AutofacDynamiteRegistrationModule` holds all the interface-to-implementation configuration for the various utilities found in the toolkit.

See the [Dynamite registration code here](https://github.com/GSoft-SharePoint/Dynamite/blob/feature/readme_quick_start/Source/GSoft.Dynamite/ServiceLocator/AutofacDynamiteRegistrationModule.cs#L65) to take a look for yourself at the extent of available services and helpers.

This module of utilities is loaded in first position every time you initialize a `SharePointServiceLocator`. 

> Since Dynamite gives you this guarantee that your own Autofac registration modules will be scanned and loaded *after* Dynamite's own utilities,
> this means you can override the base registrations with your own to *replace* or *extend* Dynamite's own internal use of these utilities.
>
> For example, do `builder.RegisterType<MyCustomLogger>().As<ILogger>()` from within you own module in order to swap out Dynamite's default
> `TraceLogger` implementation (see [default logger code here](https://github.com/GSoft-SharePoint/Dynamite/blob/feature/readme_quick_start/Source/GSoft.Dynamite/Logging/TraceLogger.cs)).
> From then on, all logging will go through your `MyCustomLogger`, even the logging made by Dynamite's other utilities (which themselves only
> depend loosely on the contract interface `ILogger`).



### Resolving Dynamite's utilities and your own registered dependencies

At last, we reach the point where we can *actually use* the above-registered components.

In a SharePoint farm solution, your typical code entry points are the following (i.e. the UI-level parts of your application):

1. An ASP.NET page lifecycle code-behind event such as `Page_Load`
2. A SharePoint event receiver such as `FeatureActivated`

You should aim to keep the logic in such entry points to a minimum, since they are coupled to the ASP.NET and SharePoint SPRequest pipelines. 
All heavy-lifting and business logic components should be encapsulated within your own more-easily-unit-tested utilities.

For example, the code-behind of a SharePoint menu WebPart's user control could look like this:

```
public partial class MainMenu : UserControl
{
    private const string MenuAscxPath = @"~/_CONTROLTEMPLATES/15/DSF/MainMenuPanel.ascx";

    protected void Page_Load(object sender, EventArgs e)
    {
        using (var injectionScope = ProjectContainer.BeginLifetimeScope())
        {
            var aPerRequestInstance = injectionScope.Resolve<IHttpRequestCache>();
            var dynamiteLogger = injectionScope.Resolve<ILogger>();
            
            // do UI rendering behavior...
        }
    }
}

```

While a site-scoped feature's event receiver would look like:

```
public override void FeatureActivated(SPFeatureReceiverProperties properties)
{
    var site = properties.Feature.Parent as SPSite;

    using (var siteCollectionLevelScope = ProjectContainer.BeginLifetimeScope(properties.Feature))
    {
        var logger = siteCollectionLevelScope.Resolve<ILogger>();
        var mySiteCreator = siteCollectionLevelScope.Resolve<IMySiteCreator>();

        // do site provisioning...
        mySiteCreator.DoComplexStuffHere(site);
    }
}
```

Note how **a `using` block should always be used** to surround the code which injects some dependencies to ensure 
proper disposal behavior of all resources through the disposal of the child lifetime scope returned by `BeginLifetimeScope`.

Beyond such UI-level entry points, all further dependencies down the call stack should be constructor-injected like so:

```
//
// Your own custom site provisioning utility:
// Registered with containerBuilder.RegisterType<MySiteCreator>().As<IMySiteCreator>()
// in a Autofac module which is loaded through the SharePointServiceLocator
//
public class MySiteCreator : IMySiteCreator
{
    //
    // Dependencies on GSoft.Dynamite utilities injected through constructor
    // 
    private ILogger logger;
    private IContentTypeHelper contentTypeHelper;

    //
    // Dependency on your own internal service/utility module, also provided
    // through constructor injection
    //
    private IMyConfigUtility config;    

    //
    // Outside of a unit testing context, you will never call this constructor yourself.
    // When you resolve IMySiteCreator from one of your UI-level projects (see example above), 
    // Autofac will take care of resolving the following dependencies for you and inject 
    // them through this constructor.
    //
    public MySiteCreator(ILogger logger, IContentTypeHelper contentTypeHelper, IMyConfigUtility config)
    {
        this.logger = logger;
        this.contentTypeHelper = contentTypeHelper;
        this.config = config;
    }

    public void DoComplexStuffHere(SPSite site)
    {
        this.logger.Info("Starting provisionning stuff for site collection " + site.Name);
        var configElement = this.config.GetFromPropertyBag(site, "config-key");
        // etc.
    }
}
```

Thus, dependencies injected in the `MySiteCreator` constructor are easily mockable if you want to unit test your components.

Note how a method parameter is used to pass the context's `SPSite` instance down the call stack. 

> **A good tip:** make sure you call `SPContext.Current.Web` and `SPContext.Current.Site` only from the UI-level (e.g. `.ascx` 
> code-behind code) but never from your own business-level class. From the UI entry-point code, pass the current `SPWeb` or `SPSite`
>  as a method parameter down to your heavy-lifting utility classes. 
>
> This allows your utilities to be reused outside of a `HttpRequest` context (perhaps from a command-line application or when calling 
> feature activation code from PowerShell - where **any** dependency on `SPContext` would be a deal breaker).

### More to read about Dynamite's service locator

Head to the wiki [for more about building modular SharePoint farm solution](https://github.com/GSoft-SharePoint/Dynamite/wiki#1-a-modular-approach-to-building-sharepoint-farm-solutions-with-dynamite-and-autofac).


Automating your deployments with PowerShell
-------------------------------------------

Large SharePoint deployments require a high level of automation to ensure repeatability across environments.

The trick is to depend on PowerShell scripts to automate your deployments, even on local development environments. You depend
less on Visual Studio magic to deploy everything and instead you use PowerShell scripts to:

1. Retract and re-deploy your WSP full trust solutions
    * See `Deploy-DSPSolution`: define a series of WSPs to deploy in a XML file and they will be retracted beforehand if required
2. Create your test/staging/production SharePoint site collection(s) if they are not provisioned already
    * See `New-DSPStructure` to create a site collection and subwebs hierarchy based on a XML file
3. Following a sequence of feature (re)activation steps to provision your site's structural components (site columns, content types, lists, pages, etc.)
    * See `Initialize-DSPFeature` to quickly deactivate then re-activate any SPFeature
    * Maintaining a feature activation sequence on top of a basic site defintion (such as Team Site) like this is easier and more flexible than than trying to bundle your own custom site definition
4. Configure dependencies on farm-level services such as Managed Metadata (taxonomy) or SharePoint Search
    * We recommend using [Gary Lapointes's cmdlets `Export-SPTerms` and `Import-SPTerms`](https://github.com/GSoft-SharePoint/PowerShell-SPCmdlets)) to help you with term store exports/imports

Dynamite provides you with the Dynamite PowerShell Toolkit ("DSP" for short), a module of cmdlets meant to help you build your own set of PowerShell deployment scripts.

Please, read more on [how to install and use the DSP cmdlets module in the wiki](https://github.com/GSoft-SharePoint/Dynamite/wiki#2-automate-your-deployments-and-upgrades-end-to-end-with-the-dynamite-powershell-toolkit).


Using Dynamite's provisioning utilities
---------------------------------------

What makes SharePoint special is that it comes out-of-the-box with high-level concepts such as Site collections, Site, Site Column, Content Types, Lists and so on.

While building applications based on SharePoint, your first order of business is typically to follow a sequence resembling this one:

1. Create a site collection
2. Initialize your term store
3. Configure some site columns (with taxonomy mappings to term store)
4. Add some content types
5. Create a few lists and document libraries
6. Create a few page instances in Pages library and add some web parts

### Create a site collection