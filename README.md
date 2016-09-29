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
    * C# library (DLL) with utilies for dependency injection, SharePoint object provisionning (fields, content types, lists), logging, SPListItem-to-entity mapping, etc.
    * Dependencies: Autofac, Newtonsoft.Json
    * Should be added to every Visual Studio project


2. **GSoft.Dynamite.SP**
    * Full-trust solution package (WSP) ready to deploy to your on-premise farm (provisions the DLL from the GSoft.Dynamite package to the GAC)
    * PowerShell cmdlets module to help with provisionning
    * Should be installed only once globally in your Visual Studio solution


Continuous Integration
======================

TeamCity builds are triggered on GSOFT's private build servers upon every commit to the repository. 

Commits to the `master` branch will generate new packages on the [stable MyGet feed](https://www.myget.org/F/dynamite-2013/api/v2) ([link to Master build](https://teamcity.gsoft.com/viewType.html?buildTypeId=bt71)).

Commits to `develop` will publish packages on the [pre-release feed](https://www.myget.org/F/dynamite-2013-dev/api/v2) instead ([link to Dev build](https://teamcity.gsoft.com/viewType.html?buildTypeId=bt69)).

The C# integration test suite runs on [a nightly build](https://teamcity.gsoft.com/viewType.html?buildTypeId=Dynamite_Dynamite2013_Dynamite2013PublicCore_Dynamite2013DevelopNightly).


Target Audience & Philosophy
============================

Dynamite is meant exclusively for On-Premise, full-trust, server-side, custom SharePoint 2013 (.NET 4.5) solution development.

Its purpose is to encourage:

* A correct approach to service location (dependency injection, inversion of control) using Autofac as its core container framework within the (particularily hairy) context of GAC-deployed SharePoint WSP solution packages
* Repeatable, idempotent SharePoint artefact provisionning sequences (site columns, content types, lists)
* Less code repetition for typical logging, internationalization and SPListItem-to-business-entity mapping scenarios
* Loosely coupled, easily unit tested, modular, extensible architectures
* Environment-independent, fully automated PowerShell installation procedures

Dynamite can be though of as an embodiment or spiritual successor to [Microsoft's patterns and practices team's famous SharePoint 2010 development guide](http://msdn.microsoft.com/en-us/library/ff770300.aspx).

Thus, the toolkit is firmly old-school in its purely server-side approach. New development efforts outside of a full-trust context (e.g. Office 365, app model development, client-side, etc.) should probably look into alternatives such as the more recent [Office PnP](https://github.com/OfficeDev/PnP) project and its remote provisionning approach.


Quick Start Guide
=================

Building your first Autofac container for service location
----------------------------------------------------------

Access to Dynamite's C# utilities is enabled through service locator-style dependency injection.

Start by creating your own application container like so:

```
using Autofac
using GSoft.Dynamite.ServiceLocator

// The Container that is used by WSPs for dependency injection across 
// all Company.Project.*.WSP projects (perhaps shared via a common
// Company.Project.ServiceLocator.DLL class library)
public static class ProjectContainer
{
    // The key that distinguishes your container from others in the same AppDomain
    private const string AppName = "Company.Project";

    // The locator will scan the Global Assembly Cache and load the following Autofac registration modules:
    // - the core Dynamite utilities registration module
    // - all Autofac registration modules from assemblies with a filename that starts with "Company.Project"
    private static ISharePointServiceLocator singletonLocatorInstance = new SharePointServiceLocator(AppName);

    // Creates a new Autofac child injection scope from the current context, from which you can .Resolve your dependencies.
    // Typically used from user control (.ASCX) or application page (.ASPX) code-behind in a HTTP request context.
    public static ILifetimeScope BeginLifetimeScope()
    {
        return singletonLocatorInstance.BeginLifetimeScope();
    }

    // Creates a new Autofac child injection scope from the current Site or Web-scoped feature context.
    // Will not allow you to .Resolve objects registered as .InstancePerRequeste (see below).
    public static ILifetimeScope BeginLifetimeScope(SPFeature featureContext)
    {
        return singletonLocatorInstance.BeginLifetimeScope(featureContext);
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

All assemblies matching your condition will be loaded from the GAC_MSIL and scanned for [Autofac registration modules](http://docs.autofac.org/en/latest/configuration/modules.html) such as, for example:

```
using Autofac;
using GSoft.Dynamite.ServiceLocator.Lifetime;   // VERY IMPORTANT to import this instead of relying on the default Autofac.RegistrationExtensions

namespace Company.Project.SubProject
{
    public class MySubProjectDependecyRegistrations : Module
    {
        public override Load(ContainerBuilder builder)
        {
            // A simple transient lifetime registration
            // (new object constructed upon every Resolve)
            builder.Register<MySubProjectService>().As<IMyProjectService>();
            
            // A transient, named config repository registration 
            // (with an example of how to hook up decorators with Autofac, nifty!)
            builder.RegisterType<ConfigRepository>().Named<IConfigRepository>("implementation");
            builder.RegisterDecorator<IConfigRepository>((c, inner) => new ElevatedSecurityConfigRepository(inner), fromKey: "implementation");

            // An application-wide singleton registration
            builder.RegisterType<MyGodClass>().As<IGod>().SingleInstance();

            // Only one object instance of the following class will be created
            // by the container per SPSite instance. This allows you to isolate site
            // collection-specific behaviors.
            builder.RegisterType<MySiteCollectionSpecificCache>().As<ISiteCollectionSpecificCache>().InstancePerSite();

            // Similarly, you can register SPWeb-scoped dependecies. SPWeb lifetime scopes
            // are children of SPSite lifetime scopes (allowing you to depend on .InstancePerSite
            // instances from your .InstancePerWeb-registered classes).
            builder.RegisterType<MySubWebSpecificCache>().As<ISubWebSpecificCache().InstancePerWeb();

            // Example of how to implement one-object-instance-per-HTTP-request behavior.
            // Objects injected through an .IntancePerRequest configuration can depend on instances
            // from the current parent SPWeb (.InstancePerWeb) and SPSite (.InstancePerSite) scopes.
            builder.RegisterType<MyHttpRequestCache>().As<IHttpRequestCache>().InstancePerRequest();
        }
    }
}
```

Note how object lifetime behavior can be configured to following singleton-per-SPSite, singleton-per-SPWeb and per-HTTP-request semantics through Dynamite's custom RegistrationExtensions. Please refer to the Dynamite wiki for [more detailed help on using service location and complex lifetime scope hierarchies](https://github.com/GSoft-SharePoint/Dynamite/wiki#1-a-modular-approach-to-building-sharepoint-farm-solutions-with-dynamite-and-autofac).

Resolving Dynamite's utilities and your own registered dependencies
-------------------------------------------------------------------

In a SharePoint farm solution, your typical code entry points are the following:

1. An ASP.NET page lifecycle code-behind event such as `Page_Load`
2. A SharePoint event receiver such as `FeatureActivated`

You should aim to keep the logic in such entry points to a minimum, since they are coupled to the ASP.NET and SharePoint SPRequest pipelines. All heavy-lifting should be encapsulated within you own more-easily-tested utilities.

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

    using (var siteCollectionLevelScope = WebsiteContainer.BeginLifetimeScope(properties.Feature))
    {
        var logger = siteCollectionLevelScope.Resolve<ILogger>();
        var mySiteCreator = siteCollectionLevelScope.Resolve<IMySiteCollectionProvisionningHelper>();

        // do site provisionning...
        mySiteCreator.DoComplexStuffHere(site);
    }
}
```

Note how a `using` block should always be used to surround the code which injects some dependencies to ensure proper disposal behavior of all resources through the disposal of the child lifetime scope returned by `BeginLifetimeScope`.

Beyond such UI-level entry points, all further dependencies down the call stack should be constructor-injected like so:

```
public class MySiteCreator : IMySiteCollectionProvisionningHelper
{
    private ILogger logger;
    private IContentTypeHelper contentTypeHelper;
    private IMyConfigUtility config;    

    public MySiteCreator(ILogger logger, IContentTypeHelper contentTypeHelper, IMyConfigUtility config)
    {
        this.logger = logger;
        this.contentTypeHelper = contentTypeHelper;
        this.config = config;
    }

    public void DoComplexStuffHere(SPSite site)
    {
        this.logger.Info("Starting provisionning stuff for site collection " + site.Name);

        // etc.
    }
}
```

Thus, dependencies injected in the `MySiteCreator` constructor are easily mockable if you want to unit test your components.

Note how a method parameter is used to pass the context's `SPSite` instance down the call stack. 

> A good tip: make sure you call `SPContext.Current.Web` and `SPContext.Current.Site` only from the UI level (e.g. `.ascx` code-behind code) and then pass the current `SPWeb` or `SPSite` as a method parameter down to your heavy-lifting utility classes. 
>
> This allows your utilities to be reused outside of a `HttpRequest` context (perhaps from a command-line application or when calling feature activation code from PowerShell - where any dependency on `SPContext` would be a deal breaker).





