### :lock: This repository is no longer maintained :lock:
---

Dynamite for SharePoint 2013
============================

A C# toolkit, PowerShell cmdlets and a WSP solution package to help you build maintainable SharePoint 2013 farm solutions (full-trust, on-premise).

* [NuGet Feeds](#nuget-feeds)
* [Continuous Integration](#continuous-integration)
* [Target Audience & Philosophy](#target-audience--philosophy)
* [Quick Start Guide](#quick-start-guide)

> **New to SharePoint development?** Try to get through [this SharePoint 101 course content](https://github.com/GSoft-SharePoint/SharePoint-101/wiki) to hit the ground running.

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

> **In summary**, Dynamite as a *batteries-included, SharePoint-aware, architecture-opinionated, intrastructure-level .NET & PowerShell toolkit* that
> acts as a building block for maintainable and automatically-provisioned SharePoint 2013 server-side, full-trust solutions.


Quick Start Guide
=================

The Dynamite toolkit covers a lot of ground. Here are a few guidelines and examples to get you up and running on these topics:

* [A) Dependency injection & service location](#a-dependency-injection--service-location)
   * Using Autofac correctly in a SharePoint server context
* [B) Automating your deployments with PowerShell](#b-automating-your-deployments-with-powershell)
* [C) Using Dynamite's provisioning utilities](#c-using-dynamites-provisioning-utilities)
   * Creating fields, content types and lists in an idempotent way
* [D) Other utilities: logging and globalization](#d-other-utilities-logging-and-globalization)
* [E) The SharePoint entity binder: easy mappings from entities to SPListItems and back](#e-the-sharepoint-entity-binder-easy-mappings-from-entities-to-splistitems-and-back)

Then take a look at the [Dynamite project wiki](https://github.com/GSoft-SharePoint/Dynamite/wiki) for complementary articles & documentation.


A) Dependency injection & service location
---------------------------------------

One main objective of the Dynamite toolkit is to guide you in the implementation of a dependency injection container - without having 
to worry too much about the particulars on how to do it "right" in a SharePoint on-premise, full-trust solution context. 

This container at the root of your application will serve as an intermediary when your components need to depend on other modules,
services and utilities. It is the "glue" that takes care of constructing concrete C# object instances while your own consumer code
only depends on interface contracts. This loose coupling strategy makes it easier to depend on other modules without having to worry 
about their own dependencies and implementation details.

In this section:
* [A.1) Building your first Autofac container for service location](#a1-building-your-first-autofac-container-for-service-location)
* [A.2) Registering your interface-to-implementation configuration as Autofac registration modules](#a2-registering-your-interface-to-implementation-configuration-as-autofac-registration-modules)
* [A.3) Dynamite's own registration module](#a3-dynamites-own-registration-module)
* [A.4) Resolving Dynamite's utilities and your own registered dependencies](#a4-resolving-dynamites-utilities-and-your-own-registered-dependencies)
* [A.5) More to read about Dynamite's service locator](#a5-more-to-read-about-dynamites-service-locator)
* [A.6) How to troubleshoot container registration problems](#a6-how-to-troubleshoot-container-registration-problems)

### A.1) Building your first Autofac container for service location

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


### A.2) Registering your interface-to-implementation configuration as Autofac registration modules

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
            // (with an example of how to hook up a DECORATOR with Autofac - nifty!)
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
yet understood their purpose and their power. Dynamite's `SharePointServiceLocator` and custom `InstancePerSite`, `InstancePerWeb` and `InstancePerRequest` lifetimes 
are meant to easily provide such fine-grained object scoping mechanics in a correct way within a full-trust SharePoint server context.

> #### Instance-per-SPRequest means web.config changes
> 
> To enable `InstancePerRequest` behavior, you need to configure a HttpModule in your server's `web.config`. 
>
> Do this by enabling the WebApplication-scope feature `GSoft.Dynamite.SP_Web Config Modifications` (ID: `2f59e5c1-448c-42ee-a782-4beac0a30370`) available from the `GSoft.Dynamite.wsp` solution package (from NuGet package GSoft.Dynamite.SP).
>
> Without the `GSoft.Dynamite.ServiceLocator.Lifetime.RequestLifetimeHttpModule` configured through this feature activation, objects will not be disposed properly at the end of each request.


### A.3) Dynamite's own registration module

The class `AutofacDynamiteRegistrationModule` holds all the interface-to-implementation configuration for the various utilities found in the Dynamite C# toolkit.

See the [Dynamite registration code in `AutofacDynamiteRegistrationModule.cs` here](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/ServiceLocator/AutofacDynamiteRegistrationModule.cs#L65) to take a look for yourself at the extent of available services and helpers.

This module of utilities is loaded in first position every time you initialize a `SharePointServiceLocator`. 

> #### An easy replace-and-extend pattern
> 
> Since Dynamite gives you this guarantee that your own Autofac registration modules will be scanned and loaded *after* Dynamite's own utilities,
> this means you can override the base registrations with your own to *replace* or *extend* Dynamite's own internal use of these utilities.
>
> For example, do `builder.RegisterType<MyCustomLogger>().As<ILogger>()` from within you own module in order to swap out Dynamite's default
> `TraceLogger` implementation (see [default logger code here](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/Logging/TraceLogger.cs)).
> From then on, all logging will go through your `MyCustomLogger`, even the logging made by Dynamite's other utilities (which themselves only
> depend loosely on the contract interface `ILogger`).



### A.4) Resolving Dynamite's utilities and your own registered dependencies

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

Beyond such UI-level entry points, all further dependencies down the call stack should be **constructor-injected** like so:

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

> #### Depending on SPContext is evil 
>
> **A good tip:** make sure you call `SPContext.Current.Web` and `SPContext.Current.Site` only from the UI-level (e.g. `.ascx` 
> code-behind code) but never from your own business-level class. From the UI entry-point code, pass the current `SPWeb` or `SPSite`
>  as a method parameter down to your heavy-lifting utility classes. 
>
> This allows your code to be reused outside of a `HttpRequest` context (perhaps from a command-line application or when calling 
> feature activation code from PowerShell - where **any** dependency on `SPContext` would be a deal breaker).

### A.5) More to read about Dynamite's service locator

Head to the wiki [for more about building modular SharePoint farm solution](https://github.com/GSoft-SharePoint/Dynamite/wiki#1-a-modular-approach-to-building-sharepoint-farm-solutions-with-dynamite-and-autofac).

Learn about advanced usage of complex SharePoint provisioning framework-building patterns by using [the Dynamite-Components project](https://github.com/GSoft-SharePoint/Dynamite-Components) 
as an example of how to use the [`AddOnProvidedServiceLocator`](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/ServiceLocator/AddOn/AddOnProvidedServiceLocator.cs) 
as the foundational building block for a reusable plugins-based architecture.

### A.6) How to troubleshoot container registration problems

Dependency injection registrations are loaded upon the first call to the `SharePointServiceLocator`. Thus, it typically occurs upon application 
startup or when you first load a page with a user control that depends on the container/service locator.

1. Install [ULSViewer.exe](https://www.microsoft.com/en-ca/download/details.aspx?id=44020) on your SharePoint server to gain access to the Unified Logging Service output (i.e. your SharePoint logs).
    * You will need to add the users running your app pools to the Local Users and Groups under the groups "Performance Log Users" and "Performance Monitor Users".
    * After that, launching ULSViewer.exe as administrator and using the shortcut `Ctrl+Shift+U` should give you a live view of your local SharePoint server logs.
2. If you are debugging a website component, make sure you `iisreset` or at least recycle your app pool.
    * If you are running a PowerShell script, you will need to restart a new PowerShell process instead to ensure full Autofac container reinitialization.
3. With ULSViewer rolling, refresh your web page or run the bit of PowerShell (perhaps a feature activation).
    * This will trigger the use of the `SharePointServiceLocator` instance within your C# code.
4. You can filter your ULSViewer output on the Category "GSoft.Dynamite" and/or the keyword "Autofac" in the Message field to help filter out the noise.
5. You should see at least one log entry summarizing the registrations of the Autofac modules that were loaded.

Below is an example of a log trace that you will find and will help you do the inventory. Note that the registrations at the end of the list will supercede any earlier registration done for the same interface.

```
10/27/2016 14:50:59.24	w3wp.exe (0x0AF8)	0x191C	Unknown	GSoft.Dynamite	00000	Medium	GSoft.Dynamite - Autofac component registry details for container GSoft.Dynamite: [Autofac.ILifetimeScope->Autofac.Core.Lifetime.LifetimeScope], [Autofac.IComponentContext->Autofac.Core.Lifetime.LifetimeScope], [GSoft.Dynamite.Logging.ILogger->GSoft.Dynamite.Logging.TraceLogger], [GSoft.Dynamite.Monitoring.IAggregateTimeTracker->GSoft.Dynamite.Monitoring.AggregateTimeTracker], [GSoft.Dynamite.Binding.SharePointDataRowEntitySchema->GSoft.Dynamite.Binding.SharePointDataRowEntitySchema], [GSoft.Dynamite.Binding.IEntitySchemaBuilder->GSoft.Dynamite.Binding.CachedSchemaBuilder], [GSoft.Dynamite.Binding.Converters.TaxonomyValueDataRowConverter->GSoft.Dynamite.Binding.Converters.TaxonomyValueDataRowConverter], [GSoft.Dynamite.Binding.Converters.TaxonomyValueCollectionDataRowConverter->GSoft.Dynamite.Binding.Converters.TaxonomyValueCollectionDataRowConverter], [GSoft.Dynamite.Binding.Converters.TaxonomyValueConverter->GSoft.Dynamite.Binding.Converters.TaxonomyValueConverter], [GSoft.Dynamite.Binding.Converters.TaxonomyValueCollectionConverter->GSoft.Dynamite.Binding.Converters.TaxonomyValueCollectionConverter], [GSoft.Dynamite.Binding.ISharePointEntityBinder->GSoft.Dynamite.Binding.SharePointEntityBinder], [GSoft.Dynamite.Cache.ICacheHelper->GSoft.Dynamite.Cache.CacheHelper], [GSoft.Dynamite.Caching.IAppCacheHelper->GSoft.Dynamite.Caching.AppCacheHelper], [GSoft.Dynamite.Caching.ISessionCacheHelper->GSoft.Dynamite.Caching.SessionCacheHelper], [GSoft.Dynamite.Configuration.IPropertyBagHelper->GSoft.Dynamite.Configuration.PropertyBagHelper], [GSoft.Dynamite.Configuration.IConfiguration->GSoft.Dynamite.Configuration.PropertyBagConfiguration], [GSoft.Dynamite.Definitions.IContentTypeBuilder->GSoft.Dynamite.Definitions.ContentTypeBuilder], [GSoft.Dynamite.Definitions.IFieldHelper->GSoft.Dynamite.Definitions.FieldHelper], [GSoft.Dynamite.Exceptions.ICatchAllExceptionHandler->GSoft.Dynamite.Exceptions.CatchAllExceptionHandler], [GSoft.Dynamite.Globalization.IResourceLocator->GSoft.Dynamite.Globalization.ResourceLocator], [GSoft.Dynamite.Globalization.IResourceLocatorConfig->GSoft.Dynamite.ServiceLocator.DefaultResourceLocatorConfig], [GSoft.Dynamite.Globalization.IMuiHelper->GSoft.Dynamite.Globalization.MuiHelper], [GSoft.Dynamite.Globalization.IDateHelper->GSoft.Dynamite.Globalization.DateHelper], [GSoft.Dynamite.Globalization.IRegionalSettingsHelper->GSoft.Dynamite.Globalization.RegionalSettingsHelper], [GSoft.Dynamite.Globalization.Variations.IVariationDirector->GSoft.Dynamite.Globalization.Variations.DefaultVariationDirector], [GSoft.Dynamite.Globalization.Variations.IVariationBuilder->GSoft.Dynamite.Globalization.Variations.CanadianEnglishAndFrenchVariationBuilder], [GSoft.Dynamite.Globalization.Variations.IVariationExpert->GSoft.Dynamite.Globalization.Variations.VariationExpert], [GSoft.Dynamite.Globalization.Variations.IVariationHelper->GSoft.Dynamite.Globalization.Variations.VariationHelper], [GSoft.Dynamite.Lists.IListHelper->GSoft.Dynamite.Lists.ListHelper], [GSoft.Dynamite.Lists.IListLocator->GSoft.Dynamite.Lists.ListLocator], [GSoft.Dynamite.Lists.IListSecurityHelper->GSoft.Dynamite.Lists.ListSecurityHelper], [GSoft.Dynamite.Catalogs.ICatalogBuilder->GSoft.Dynamite.Catalogs.CatalogBuilder], [GSoft.Dynamite.MasterPages.IMasterPageHelper->GSoft.Dynamite.MasterPages.MasterPageHelper], [GSoft.Dynamite.MasterPages.IExtraMasterPageBodyCssClasses->GSoft.Dynamite.MasterPages.ExtraMasterPageBodyCssClasses], [GSoft.Dynamite.Navigation.INavigationService->GSoft.Dynamite.Navigation.NavigationService], [GSoft.Dynamite.Navigation.INavigationNode->GSoft.Dynamite.Navigation.NavigationNode], [GSoft.Dynamite.Navigation.NavigationManagedProperties->GSoft.Dynamite.Navigation.NavigationManagedProperties], [GSoft.Dynamite.Repositories.IFolderRepository->GSoft.Dynamite.Repositories.FolderRepository], [GSoft.Dynamite.Repositories.IQueryHelper->GSoft.Dynamite.Repositories.QueryHelper], [GSoft.Dynamite.Repositories.IItemLocator->GSoft.Dynamite.Repositories.ItemLocator], [GSoft.Dynamite.Security.ISecurityHelper->GSoft.Dynamite.Security.SecurityHelper], [GSoft.Dynamite.Security.IUserHelper->GSoft.Dynamite.Security.UserHelper], [GSoft.Dynamite.Serializers.IXmlHelper->GSoft.Dynamite.Serializers.XmlHelper], [GSoft.Dynamite.Serializers.ISerializer->GSoft.Dynamite.Serializers.JsonNetSerializer], [GSoft.Dynamite.Setup.IFieldValueInfo->GSoft.Dynamite.Setup.FieldValueInfo], [GSoft.Dynamite.Setup.IFolderInfo->GSoft.Dynamite.Setup.FolderInfo], [GSoft.Dynamite.Setup.IPageInfo->GSoft.Dynamite.Setup.PageInfo], [GSoft.Dynamite.Setup.ITaxonomyInfo->GSoft.Dynamite.Setup.TaxonomyInfo], [GSoft.Dynamite.Setup.ITaxonomyMultiInfo->GSoft.Dynamite.Setup.TaxonomyMultiInfo], [GSoft.Dynamite.Setup.IFolderMaker->GSoft.Dynamite.Setup.FolderMaker], [GSoft.Dynamite.Setup.IPageCreator->GSoft.Dynamite.Setup.PageCreator], [GSoft.Dynamite.Taxonomy.ISiteTaxonomyCacheManager->GSoft.Dynamite.Taxonomy.PerRequestSiteTaxonomyCacheManager], [GSoft.Dynamite.Taxonomy.ITaxonomyService->GSoft.Dynamite.Taxonomy.TaxonomyService], [GSoft.Dynamite.Taxonomy.ITaxonomyHelper->GSoft.Dynamite.Taxonomy.TaxonomyHelper], [GSoft.Dynamite.TimerJobs.ITimerJobExpert->GSoft.Dynamite.TimerJobs.TimerJobExpert], [GSoft.Dynamite.Utils.IEventReceiverHelper->GSoft.Dynamite.Utils.EventReceiverHelper], [GSoft.Dynamite.Utils.ISearchHelper->GSoft.Dynamite.Utils.SearchHelper], [GSoft.Dynamite.Utils.ICustomActionHelper->GSoft.Dynamite.Utils.CustomActionHelper], [GSoft.Dynamite.Utils.IContentOrganizerHelper->GSoft.Dynamite.Utils.ContentOrganizerHelper], [GSoft.Dynamite.Utils.INavigationHelper->GSoft.Dynamite.Utils.NavigationHelper], [GSoft.Dynamite.Navigation.ICatalogNavigation->GSoft.Dynamite.Navigation.CatalogNavigation], [GSoft.Dynamite.Repositories.IComposedLookRepository->GSoft.Dynamite.Repositories.ComposedLookRepository], [GSoft.Dynamite.Branding.IDisplayTemplateHelper->GSoft.Dynamite.Branding.DisplayTemplateHelper], [GSoft.Dynamite.Branding.IImageRenditionHelper->GSoft.Dynamite.Branding.ImageRenditionHelper], [GSoft.Dynamite.Caml.ICamlBuilder->GSoft.Dynamite.Caml.CamlBuilder], [GSoft.Dynamite.Caml.ICamlUtils->GSoft.Dynamite.Caml.CamlUtils], [GSoft.Dynamite.WebConfig.IWebConfigModificationHelper->GSoft.Dynamite.WebConfig.WebConfigModificationHelper], [GSoft.Dynamite.WebParts.IWebPartHelper->GSoft.Dynamite.WebParts.WebPartHelper], [GSoft.Dynamite.ReusableContent.Contracts.Repositories.IReusableContentRepository->GSoft.Dynamite.ReusableContent.Core.Repositories.ReusableContentRepository], [GSoft.Dynamite.ReusableContent.Contracts.Entities.ReusableHtmlContent->GSoft.Dynamite.ReusableContent.Contracts.Entities.ReusableHtmlContent], [GSoft.Dynamite.ReusableContent.Contracts.Services.IReusableContentService->GSoft.Dynamite.ReusableContent.Core.Services.ReusableContentService], [GSoft.Dynamite.ReusableContent.Contracts.WebParts.IReusableContentWebPart->GSoft.Dynamite.ReusableContent.ReusableContentWebPart.ReusableContentWebPart], [GSoft.Dynamite.Globalization.IResourceLocatorConfig->GSoft.Dynamite.PowerShell.ServiceLocator.PowerShellResourceLocationConfig], [GSoft.Dynamite.Utils.ICatalogHelper->GSoft.Dynamite.Utils.CatalogHelper], [GSoft.Dynamite.Portal.Contracts.WebParts.IContentBySearchSchedule->GSoft.Dynamite.Portal.SP.Publishing.WebParts.ContentBySearchSchedule.ContentBySearchSchedule], [GSoft.Dynamite.Portal.Contracts.WebParts.IResultScriptSchedule->GSoft.Dynamite.Portal.SP.Publishing.WebParts.ResultScriptSchedule.ResultScriptSchedule], [GSoft.Dynamite.Portal.Contracts.WebParts.IContextualNavigation->GSoft.Dynamite.Portal.SP.Publishing.WebParts.ContextualNavigation.ContextualNavigation], [GSoft.Dynamite.Portal.Contracts.WebParts.IChildNodes->GSoft.Dynamite.Portal.SP.Publishing.WebParts.ChildNodes.ChildNodes], [GSoft.Dynamite.Portal.Contracts.Factories.IContentTypeFactory->GSoft.Dynamite.Portal.Core.Factories.ContentTypeFactory], [GSoft.Dynamite.Portal.Contracts.Utils.ISchedulingControl->GSoft.Dynamite.Portal.Core.Utils.SchedulingControl], [GSoft.Dynamite.Portal.Contracts.Utils.IContentAssociation->GSoft.Dynamite.Portal.Core.Utils.ContentAssociation], [GSoft.Dynamite.Globalization.IResourceLocatorConfig->GSoft.Dynamite.Portal.Core.Resources.PortalResourceLocatorConfig], [GSoft.Dynamite.Portal.Contracts.Factories.IListViewFactory->GSoft.Dynamite.Portal.Core.Factories.ListViewFactory], [GSoft.Dynamite.Portal.Contracts.Factories.IWebPartFactory->GSoft.Dynamite.Portal.Core.Factories.WebPartFactory], [GSoft.Dynamite.Portal.Contracts.Utils.INavigationBuilder->GSoft.Dynamite.Portal.Core.Utils.NavigationBuilder], [GSoft.Dynamite.Portal.Contracts.Services.INavigationService->GSoft.Dynamite.Portal.Core.Services.NavigationService],	7b9ab19d-394a-70f2-d900-4704378eeb9b
```

Slightly earlier in the logs, you should also find a log entry summarizing which DLLs were scanned and loaded from the GAC. Make sure your own assemblies are loaded. 
If they aren't, adjust the filename-scanning-and-filtering predicate you feed as second parameter to [the `SharePointServiceLocator` constructor](#a1-building-your-first-autofac-container-for-service-location).

> #### Logs to die for
> 
> Being skilled at scanning and filtering the ULS logs is an essential skill for SharePoint on-premise developers. It is your last and best line of 
> investigation when troubleshooting SharePoint and custom solution errors.
>
> Tip: When all else fails, try turning on the Verbose level logs in *Central Administration > Monitoring > Diagnostics logging* across all categories.
> However, be prepared to be overwhelmed by the volume of logs produced, so you should launch ULSViewer only moments before you do the action that causes
> the error you are trying to diagnose.


B) Automating your deployments with PowerShell
-------------------------------------------

A large SharePoint deployment requires a high level of automation to **ensure repeatability** across environments (dev/testing/staging/production).

The trick is to depend on PowerShell scripts to automate your deployments, even on local development environments. You depend
less on Visual Studio magic to deploy everything and instead you use PowerShell scripts to:

1. Publish a folder/zip of deployment artifacts, containing all scripts and WSP solutions that need to be run and deploy
    * If you don't have a SharePoint build server, you can still package/publish your WSP solution with (gasp!) Visual Studio 
        * Make sure you retract everything from the GAC before you attempt a clean build + publish, or suffer the shameful consequences DLL hell inherent to GAC deployments
    * Then use `Copy-DSPFiles` to copy your PowerShell scripts, modules and input XML and `Copy-DSPSolutions` to bring all packaged WSPs to the deployment folder
        * See [an example `Publish-DeploymentFolder.ps1` example](https://github.com/GSoft-SharePoint/Dynamite-Components/blob/develop/Source/GSoft.Dynamite.Models.StandardPublishingCMS/Publish-DeploymentFolder.ps1) and build your own!
2. Retract and re-deploy your WSP full trust solutions
    * See `Deploy-DSPSolution`: define a series of WSPs to deploy in a XML file and they will be retracted beforehand if required
    * Make sure you configure all your SharePoint features to have no automatic activation behavior upon WSP deployment: the only bad side effect of a WSP retract+deploy run should be an IIS application pool recycle (not rogue feature activations).
5. Configure dependencies on farm-level services such as Managed Metadata (taxonomy) and SharePoint search
    * We recommend using [Gary Lapointes's cmdlets `Export-SPTerms` and `Import-SPTerms`](https://github.com/GSoft-SharePoint/PowerShell-SPCmdlets)) to help you with term store exports/imports
3. Create your test/staging/production SharePoint site collection(s) if they are not provisioned already
    * See `New-DSPStructure` to create a site collection and subwebs hierarchy based on a XML file
4. Following a sequence of feature (re)activation steps to provision your site's structural components (site columns, content types, lists, pages, etc.)
    * See `Initialize-DSPFeature` to quickly deactivate then re-activate any SPFeature
    * Maintaining a feature activation sequence on top of a basic site defintion (such as Team Site) like this is easier and more flexible than than trying to bundle your own custom site definition
5. Configure some more dependencies on farm-level services such as SharePoint Search
6. *Last but not least*: Run some final integration Pester tests on your deployment to make sure provisioning completed successfully
    * [Pester](https://github.com/pester/Pester) is a great tool for BDD testing and you should already be using it! What are you waiting for?

Dynamite provides you with the Dynamite PowerShell Toolkit ("DSP" for short), a module of cmdlets meant to help you build your own set of PowerShell deployment scripts.

Please, read more on [how to install and use the DSP cmdlets module in the wiki](https://github.com/GSoft-SharePoint/Dynamite/wiki#2-automate-your-deployments-and-upgrades-end-to-end-with-the-dynamite-powershell-toolkit).


C) Using Dynamite's provisioning utilities
---------------------------------------

What makes SharePoint special is that it comes out-of-the-box with high-level concepts such as Site Collections, Site, Site Column, Content Types, Lists and so on.

While building applications based on SharePoint, your first order of business is typically to follow a sequence resembling this one:

* [C.1) Create a site collection](#c1-create-a-site-collection)
* [C.2) Initialize your term store](#c2-initialize-your-term-store)
* [C.3) Configure some site columns(with taxonomy mappings to term store)](#c3-configure-some-site-columns-with-taxonomy-mappings-to-term-store)
* [C.4) Add some content types](#c4-add-some-content-types)
* [C.5) Create a few lists and document libraries](#c5-create-a-few-lists-and-document-libraries)
* [C.6) Create a few page instances in the Pages library and add some web parts](#c6-create-a-few-page-instances-in-the-pages-library-and-add-some-web-parts)

From then on, SharePoint takes a role similar to that of a database in regular application development. Your SharePoint site structure acts as a back-end to your (hopefully) 
isolated business logic

### C.1) Create a site collection

We recommend using the Dynamite PowerShell cmdlets to create your hierarchy of site collection and webs.

Start with a configuration file `Tokens.YOUR-MACHINE-HOSTNAME.ps1` and add a few variables to it:

```
# Configuration for publishing site provisioning sequence

# Site collection admin user
$DSP_SiteCollectionAdmin = "DOMAIN\myuser"

# Web app URL
$DSP_PortalWebApplicationUrl = "http://my-web-application.example.com"

# Hostname site collection URL + LCID for publishing site
$DSP_SiteCollectionHostHeader = "http://my-publishing-intranet.example.com"
$DSP_PubSiteLanguage = 1036

# Content DB to use to store site collection content (will be created if it doesn't exist)
$DSP_ContentDatabase = "SP_Content_MyPublishingSite"
```

Create the following `Sites.template.xml` definition file for a simple publishing site collection with two webs/subsites:

```
<?xml version="1.0" encoding="utf-8"?>
<WebApplication Url="[[DSP_PortalWebApplicationUrl]]">
  <Site Name="My Publishing Site" HostNamePath="[[DSP_SiteCollectionHostHeader]]" IsAnonymous="True" OwnerAlias="[[DSP_SiteCollectionAdmin]]" Language="[[DSP_PubSiteLanguage]]" Template="BLANKINTERNET#0" ContentDatabase="[[DSP_PubDatabaseName]]">
    <Webs>
       <Web Name="My HR News Site" Path="rh" Template="BLANKINTERNET#0" Language="[[DSP_PubSiteLanguage]]">
       </Web>
       <Web Name="My Communications" Path="com" Template="BLANKINTERNET#0">
       </Web>
    </Webs>
  </Site>
</WebApplication>
```

You can use [any site definition/template ID](http://www.funwithsharepoint.com/sharepoint-2013-site-templates-codes-for-powershell/) supported by SharePoint, such as `STS#0` for Team Sites, etc.

Then,

1. Run `Update-DSPTokens` to instantiate the `Sites.xml` file 
    * Tokens matching `[[DSP_*]]` in the `*.template.xml` are replaced by the variables matching `$DSP_*` from the `Tokens.{MY-MACHINE-NAME}.ps1` file.

2. Run `New-DSPStructure .\Sites.xml` to start provisioning
    * The content database will be created if need be
    * The site collection and subsites will be created and any missing subwebs
    * If you want to remove any dev site you already have in place to test your full provisioning sequence (early in development this is usually the case), you can run `Remove-DSPStructure .\Sites.xml` beforehand.

### C.2) Initialize your term store

As mentioned above, install the set of cmdlets [from Gary Lapointe](https://github.com/GSoft-SharePoint/PowerShell-SPCmdlets) to help with term store exports and imports.

1. Log onto your term store interface and manually click-create your taxonomy hierarchy (term sets and terms with their multilingual labels)
2. Run `Export-SPTerms` to obtain `MyTermGroup.xml`
3. Modify the XML file to replace all usernames with the string `[[DSP_SiteCollectionAdmin]]` and rename the file to `MyTermGroup.template.xml`
4. Once you have this template file, you can run `Update-DSPTokens` to generate an environment-specific XML ready to import
5. Delete everything from your term store
5. Run `Import-SPTerms` to initialize your term store from stratch with your XML definition

Recommendations:
* Keep the same term set and term Guids between environments to simplify mappings between taxonomy site columns and the term store
* Be extra careful with term reuses/pinned terms, since their misuse will lead to nasty orphaned terms problems
* Use [GSoft-SharePoint's fork of Gary Lapointe's cmdlets](https://github.com/GSoft-SharePoint/PowerShell-SPCmdlets) if you want to ensure your term local custom properties are exported and imported properly
* Maintain a static class to document your term set and term group "constants" programmatically, as shown below:

```
public static class MyTermStoreDefinitions
{
    public static TermGroupInfo MyTermGroup
    {
        get
        {
            // If you don't specify a parent TermStoreInfo, it is assumed that your term group is part of 
            // the default term store of your SharePoint farm. Multiple term stores can exist: in this case
            // you should be explicit in initializing your TermGroupInfo's parent TermStoreInfo object.
            return new TermGroupInfo(
                new Guid("{7F9BADDB-A943-4423-A073-EA7B98554E53}"), 
                "MyTermGroupName");
        }
    }

    public static TermSetInfo MyFirstTermSet
    {
        get 
        {
            return new TermSetInfo(
                new Guid("{4CCF8615-2BC5-4116-9714-4BC940066499}"), 
                "MyFirstTermSetName", 
                MyTermGroup);
        }
    }

    public static TermSetInfo MySecondTermSet
    {
        get 
        {
            return new TermSetInfo(
                new Guid("{4194B4E3-C4DA-4617-AFF7-1D0971FD6CFB}"), 
                "MySecondTermSetName", 
                MyTermGroup);
        }
    }

    public static TermInfo MySpecialSnowflakeTerm
    {
        get
        {
            return new TermInfo(
                new Guid("{BF656F1B-6055-4695-A597-423DEA9BDA78}"), 
                "Default label of a term with special meaning in my app", 
                MyFirstTermSet);
        }
    }
}
```

> #### Each TermInfo gives you a full parent context
> 
> Note how these classes in the `GSoft.Dynamite.Taxonomy` namespace are organized in a hierarchical fashion: 
>
> * From a `TermInfo` you can navigate "up" to its parent term group (i.e. you can navigate `TermInfo -> TermSetInfo ->`TermGroupInfo -> TermStoreInfo`). 
>    * however, you can't navigate "down" from a `TermSetInfo` to its children to avoid cycles, keeping these `*Info` data structures easy to serialize
> * A null `TermStoreInfo` instance going up the hierarchy indicates the default Farm term store should be used for term lookups. 
> 
> * A null `TermGroupInfo` indicates the the special "Default Site Collection Term Group" is where the specified Term Set lies (i.e. the term set
> created automatically when spawing a new publishing-type site collection and visible only from that site collection Term Store Management screen).

### C.3) Configure some site columns (with taxonomy mappings to term store)

Site columns are the field types that will be re-used across all content types in your information infrastructure.

Instead of using good-old XML to define your fields (as is tradition), we recommend defining them as part of your C# code.

> #### Sprinkling a little DRY on site columns
>
> We want to avoid repeating ourselves. When you define your column once in XML and then refer to them through code, you end up
> duplicating information (the field Guids, their internal names) and this is one of the most common sources of error in SharePoint
> development.

The Dynamite C# library includes many classes - all deriving from `BaseFieldInfo` - than can be used to define site columns. For example:

```
public static class MyFieldDefinitions
{
    public const string MyTextFieldInternalName = "MyTextField";
    public const string MyHiddenBooleanFieldInternalName = "MyBooleanField";
    public const string MyDateOnlyFieldInternalName = "MyDateField";
    public const string MyTaxonomyFieldInternalName = "MyTaxonomyField";
    public const string MyTaxonomyMultiFieldInternalName = "MyTaxonomyMultiField";

    public static TextFieldInfo MyTextField
    {
        get
        {
            return new TextFieldInfo(
                MyTextFieldInternalName,
                new Guid("{B785DE83-EF92-4B36-96FD-7390B5523099}"),
                "Field_MyTextField_Title",
                "Field_MyTextField_Description",
                "My_ContentGroup");
        }
    }

    public static BooleanFieldInfo MyHiddenBooleanField
    {
        get
        {
            return new BooleanFieldInfo(
                MyHiddenBooleanFieldInternalName,
                new Guid("{51CA8736-6ABB-4DFD-BFAE-06FED2C873F8}"),
                "Field_MyBooleanField_Title",
                "Field_MyBooleanField_Description",
                "My_ContentGroup")
                {
                    IsHidden = true
                };
        }
    }
    
    public static DateTimeFieldInfo MyDateOnlyField
    {
        get
        {
            return new DateTimeFieldInfo(
                MyDateOnlyFieldInternalName,
                new Guid("{A96A8D8E-5C18-4B05-99D4-DE722CD76B6E}"),
                "Field_MyDateField_Title",
                "Field_MyDateField_Description",
                "My_ContentGroup")
                {
                    DefaultFormula = "=[Today]",
                    Format = DateTimeFieldFormat.DateOnly
                };
        }
    }

    public static TaxonomyFieldInfo MyTaxonomyField
    {
        get
        {
            return new TaxonomyFieldInfo(
                MyTaxonomyFieldInternalName,
                new Guid("{9661323A-1C6F-4DD1-8508-EF3FB49E29B6}"),
                "Field_MyTaxonomyField_Title",
                "Field_MyTaxonomyField_Description",
                "My_ContentGroup")
                {
                    TermStoreMapping = new TaxonomyContext(MyTermStoreDefinitions.MyFirstTermSet),
                    IsPathRendered = true
                };
        }
    }
        
    public static TaxonomyMultiFieldInfo MyTaxonomyMultiField
    {
        get
        {
            return new TaxonomyMultiFieldInfo(
                MyTaxonomyFieldInternalName,
                new Guid("{2DC8FDBB-0FC1-4251-8FAC-349E5CDE41EF}"),
                "Field_MyTaxonomyMultiField_Title",
                "Field_MyTaxonomyMultiField_Description",
                "My_ContentGroup")
                {
                    TermStoreMapping = new TaxonomyContext(MyTermStoreDefinitions.MySecondTermSet),
                    CreateValuesInEditForm = true
                };
        }
    }
}
```

Note how we define the taxonomy fields' mappings to the term store using the `TermSetInfo` constants defined in [section C.2)](#c2-initialize-your-term-store) above.

See [the `GSoft.Dynamite.Field.Types` namespace here](https://github.com/GSoft-SharePoint/Dynamite/tree/develop/Source/GSoft.Dynamite/Fields/Types) 
for a full list of supported field types.

> #### FieldType <--> ValueType
>
> Note how all `*FieldInfo` types are defined by specifying through generics what the "associated value type" of each column is.
>
> For example:
> * `public class BooleanFieldInfo : BaseFieldInfoWithValueType<bool?>`
> * `public class DateTimeFieldInfo : BaseFieldInfoWithValueType<DateTime?>`
> * `TaxonomyFieldInfo : BaseFieldInfoWithValueType<TaxonomyValue>`
> * `TaxonomyMultiFieldInfo : BaseFieldInfoWithValueType<TaxonomyValueCollection>`
>
> Deriving from the generic `BaseFieldInfoWithValueType<T>` gives you access to the property `.AssocatedValueType`.
>
> This introspective quality to `*FieldInfo` definitions and the strongly-typed relationship between **[Field Types](https://github.com/GSoft-SharePoint/Dynamite/tree/develop/Source/GSoft.Dynamite/Fields/Types)** 
> and **[Value Types](https://github.com/GSoft-SharePoint/Dynamite/tree/develop/Source/GSoft.Dynamite/ValueTypes)** 
> forms the bridge between site column provisioning through `IFieldHelper` and SharePoint-SPListItem-to-entity binding made possible through
> [`ISharePointEntityBinder`](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/Binding/ISharePointEntityBinder.cs) (introduced below in section C.6)

Once your field definitions are in place, you can use the `IFieldHelper` utility to provision your site column definitions in your new site collections,
typically during a SharePoint feature activation. For example:

```
public override void FeatureActivated(SPFeatureReceiverProperties properties)
{
    var site = properties.Feature.Parent as SPSite;
    using (var injectionScope = ProjectContainer.BeginLifetimeScope(properties.Feature))
    {
        var fieldHelper = injectionScope.Resolve<IFieldHelper>();

        var fieldsToProvision = new List<BaseFieldInfo>() {
            MyFieldDefinitions.MyTextField,
            MyFieldDefinitions.MyHiddenBooleanField,
            MyFieldDefinitions.MyDateOnlyField,
            MyFieldDefinitions.MyTaxonomyField,
            MyFieldDefinitions.MyTaxonomyMultiField
        };

        IEnumerable<SPField> provisionedSiteColumns = fieldHelper.EnsureField(site.RootWeb.Fields, fieldsToProvision);
    }
}
```

Deploy your WSP solution to the GAC, activate your feature and the fields should appear in the root web's available site columns. 
From then on, you can use those fields in your content type definitions. 

Note how the `FieldHelper` knows: 
* How to support **idempotent** provisioning (probably the **key reason** why you would want to use Dynamite's provisioning utilities): 
    * If you deactivate and re-activate the above feature multiple times, nothing bad will happen
    * If you add more fields to the definition, re-deploy and re-activate the feature, your new fields will be provisioned
    * If you update the definition of fields, re-deploy and re-activate the feature, (as long as you keep the same field ID and Internal Name) your changes will be pushed to the already-deployed site column
        * You should still be careful when defining site columns: updating a field definition can have unintended effects and should be tested carefully before rollout. Sometimes, when already in production, the best idea is to create a brand new column, migrate existing data to it and hide the previous field.
* How to link up taxonomy fields to their term set automatically (thanks to the `.TaxonomyConext` property), making your life less complicated.
* How to use the `IResourceLocator` to initialize your site columns in a fully localized fashion using the resource keys you defined in your `*FieldInfo` constants.
    * For example, the string `"Field_MyTaxonomyField_Title"` is a key to a localized resource found in file `Company.Project.AppModule.en-US.resx` (maybe deployed as a Global Assembly Resource or to `$SharePointRoot\Resources`, both are looked up).
    * The resource file is looked up thanks to your registration of a custom implmentation of the `IResourceLocatorConfig` interface.
    * See section D) below for more on how to use Dynamite's `IResourceLocator` for internationalization

If you need to, you can use the returned `SPField` object collection to further tweak your site column definitions (as long as you don't forget to call `SPField.Update()` 
to persist your further enhancements).

> #### Field definitions belong at the root
> 
> Make sure you provision site columns on the Root Web of your site collections first. Defining a field within a sub-web
> or on a list directly tends to limit you options. Dynamite's `FieldHelper` tries to be smart and will always attempt to 
> provision your fields at the topmost level in your site collection (even if you pass it a SPFieldCollection from a sub-web
> or a list) to make sure site columns are always provisioned before list columns.

### C.4) Add some content types

Compose your field definitions with out-of-the-box columns to express your own list and document content types.

For example, you could declare the following content types definitions: 

```
public static class MyContentTypeDefinitions
{
    public static ContentTypeInfo MyDocument
    {
        get
        {
            var titleAndName = new List<BaseFieldInfo>()
            {
                BuiltInFields.FileLeafRef,  // File name
                BuiltInFields.Title
            };

            var myDocFields = new List<BaseFieldInfo>()
            {
                MyFieldDefinitions.MyDateOnlyField,
                MyFieldDefinitions.MyTaxonomyField
            };

            var docIdFields = new List<BaseFieldInfo>()
            {
                new MinimalFieldInfo<UrlValue>("_dlc_DocIdUrl", new Guid("{3b63724f-3418-461f-868b-7706f69b029c}")),
                new MinimalFieldInfo<string>("dlc_DocId", new Guid("{ae3e2a36-125d-45d3-9051-744b513536a6}"))
            };

            var allFields = titleAndName.Concat(myDocFields).Concat(docIdFields);

            return new ContentTypeInfo(
                ContentTypeIdBuilder.CreateChild(BuiltInContentTypes.Document, new Guid("{CC651266-E8C2-4075-BC6D-333FE1F0C2A9}")),
                "CT_MyDocument_Title",
                "CT_MyDocument_Description",
                "My_ContentGroup")
                {
                    Fields = allFields.ToList()
                };
        }
    }

    public static ContentTypeInfo MyListItem
    {
        get
        {
            return new ContentTypeInfo(
                ContentTypeIdBuilder.CreateChild(BuiltInContentTypes.Item, new Guid("{98D58929-52D7-4CB3-BBD3-E91D4B6E8478}")),
                "CT_MyListItem_Title",
                "CT_MyListItem_Description",
                "My_ContentGroup")
                {
                    Fields = new List<BaseFieldInfo>()
                    {
                        // Title field is already added implicitly since we derive from OOTB Item CT
                        MyFieldDefinitions.MyTextField
                        MyFieldDefinitions.MyHiddenBooleanField,
                        MyFieldDefinitions.MyDateOnlyField,
                        MyFieldDefinitions.MyTaxonomyField,
                        MyFieldDefinitions.MyTaxonomyMultiField
                    }
                };
        }
    }
}

```

Then use the `IContentTypeHelper` from a feature event receiver to provision these new site content types in an idempotent fashion:

```
public override void FeatureActivated(SPFeatureReceiverProperties properties)
{
    var site = properties.Feature.Parent as SPSite;
    using (var injectionScope = ProjectContainer.BeginLifetimeScope(properties.Feature))
    {
        var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

        IEnumerable<SPContentType> provisionedCTs = contentTypeHelper.EnsureContentType(
            site.RootWeb.ContentTypes, 
            new List<ContentTypeInfo>()
            {
                MyContentTypeDefinitions.MyDocument,
                MyContentTypeDefinitions.MyListItem
            });
    }
}
```

> #### A pattern emerges
>
> Most of Dynamite's provisioning utils follow this pattern:
>
> 1. Use a **declarative style** for **`FooInfo`** object definitions that have a parallel with a SharePoint artefact type
>
>     * `BaseFieldInfo` <--> `SPField`, `ContentTypeInfo` <--> `SPContentType`, `ListInfo` <--> `SPList`, etc.
>
>     * What makes Dynamite's `FooInfo` objects special is that they are **easy to serialize**
>
> 2. Use a **`IFooHelper` utility to provision** your `FooInfo` definitions as SharePoint artefacts
>
>     * All provisioning helpers use "ensure" semantics to create-or-update in an **idempotent** way


### C.5) Create a few lists and document libraries

Fields and content types are great, but it's all boilerplate until you get to creating lists and document libraries.

You can use the `IListHelper` to adjust existing lists or create new ones:

```
public override void FeatureActivated(SPFeatureReceiverProperties properties)
{
    var site = properties.Feature.Parent as SPSite;
    using (var injectionScope = ProjectContainer.BeginLifetimeScope(properties.Feature))
    {
        var listHelper = scope.Resolve<IListHelper>();
        var viewFields = new[] 
        {
            BuiltInFields.TitleLink,
            MyFieldDefinitions.MyDateOnlyField,
            MyFieldDefinitions.MyTaxonomyField
            BuiltInFields.Modified,
            BuiltInFields.ModifiedBy,
            new MinimalFieldInfo<UrlValue>("_dlc_DocIdUrl", new Guid("{3b63724f-3418-461f-868b-7706f69b029c}"))
        };

        // Change available content types on default general purpose doc lib to list only our own
        var ootbDocLibWithAdjustedCTs = new ListInfo("Documents", "Core_Documents", "Core_Documents")
        {
            ContentTypes = new[] 
            {
                MyContentTypeDefinitions.MyDocument
            },
            DefaultViewFields = viewFields
        };

        SPList defaultDocLibUpdatedToUseMyCT = listHelper.EnsureList(site.RootWeb, ootbDocLibWithAdjustedCTs);

        // A brand new document library definition
        var superDocLibInfo = new ListInfo("SuperDocLib", "DocLib_SuperTitle", "DocLib_SuperDescription")
        {
            ContentTypes = new[] 
            {
                DocContentTypes.MyDocument
            },
            ListTemplateInfo = BuiltInListTemplates.DocumentLibrary,
            DefaultViewFields = viewFields
        };

        SPList superProvisionedList = listHelper.EnsureList(site.RootWeb, superDocLibInfo);
    }

```

### C.6) Create a few page instances in the Pages library and add some web parts

Let's keep the ball rolling and create a custom search results page with an extra web part at the bottom of the Center web part zone:

```
using (var injectionScope = ProjectContainer.BeginLifetimeScope(properties.Feature))
{
    var pageHelper = scope.Resolve<IPageHelper>();
    var pagesLibrary = site.RootWeb.GetPagesLibrary();

    var welcomePageContentTypeId = "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4";
    var searchPageLayout = new PageLayoutInfo("SearchResults.aspx", welcomePageContentTypeId);

    var registreResultsPageInfo = new PageInfo()
    {
        FileName = "MySearchResultsPage",
        Title = "My Search",
        IsPublished = true,
        PageLayout = searchPageLayout,
        WebParts = new[] { 
            new WebPartInfo("Center", new MyCustomWebPart(), 500)
        }
    };

    pageHelper.EnsurePage(pagesLibrary, pagesLibrary.RootFolder, registreResultsPageInfo);
}
```


## D) Other utilities: logging and globalization

[As shown above](#a4-resolving-dynamites-utilities-and-your-own-registered-dependencies), logging to the SharePoint ULS is a piece of cake with Dynamite's [`TraceLogger`](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/Logging/TraceLogger.cs):

```
using(var scope = ProjectContainer.BeginLifetimeScope())
{
    var logger = scope.Resolve<ILogger>();
    logger.Info("Formatted log trace at Level={1} and Category={2}", "Medium", "Company.Project");
    logger.Error("Unexpected-level event!");    
}
```

Don't hesitate to [register your own `ILogger` implementation](#an-easy-replace-and-extend-pattern) to enhance the basic implementation's behavior!

Dynamite will also help with the internationalization of your solution. The `IResourceLocator` serves as a central utility to find resource strings that come from **both**:

1. Global Assemble Resources (typically used in code-behind and user controls)
2. `$SharePointRoot\Resources`-deployed resources (typically used in SharePoint Element.XML feature module definitions)

All you have to do is deploy your resource files through your WSP package and then register a class that implements [`IResourceLocatorConfig`](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/Globalization/IResourceLocatorConfig.cs) and return the resource file prefixes you want resolved through the global `ResourceLocator`:

```
public class MyResourceLocatorConfig : IResourceLocatorConfig
{
    public ICollection<string> ResourceFileKeys
    {
        get
        {
            return new[]
            {
                // all files like "Company.Project.en-US.resx" and
                // "Company.Project.fr-FR.resx" will be searched
                // by the Dynamite ResourceLocator
                "Company.Project"
            }
        }
    }

}
```

Use the `IResourceLocator` like so:

```
using (var scope = ProjectContainer.BeginLifetimeScope())
{
    var resourceLocator = scope.Resolve<IResourceLocator>();

    // Fetch by key from all RESX files configured through 
    // your IResourceLocatorConfig registrations (using CurrentUILanguage)
    var myLocalizedString = resourceLocator.Find("CT_MyDocumentTitle");

    // Specify a resource file name (helpful in case of resource key 
    // conflicts across many files)
    var myOtherLocalizedString = resourceLocator.Find("Specific.ResourceFile.Prefix", "Some_Label_Name");
}
```

No need to worry if you created the resource file as Global Assembly Resources or as content deployed to the SharePoint Root resource folder: the `ResourceLocator` will look in both places for you. 

## E) The SharePoint entity binder: easy mappings from entities to SPListItems and back

Suppose we have some very complex business logic to implement as part of my application. In an ideal world, we don't want to mix my business logic with data access code that interacts with SharePoint.

Instead of manipulating objects of type `SPListItem` - which are great as "dictionaries-of-values" -, we would prefer to map them to some business entities which are easier to reason with.

For example, lets configure a list that uses our `MyListItem` content type [we intialized above](#c4-add-some-content-types):

```
// Somewhere in a FeatureActivated event
using(var scope = ProjectContainer.BeginLifetimeScope(properties.Feature))
{
    var listHelper = scope.Resolve<IListHelper>();
    var listDefinition = new ListInfo("MyWebRelativeUrl", "List_MyListTitle", "List_MyListDescription");
    {
        ContentTypes = new[]
        {
            MyContentTypeDefinitions.MyListItem
        }
    }        

    var myCustomList = listHeper.EnsureList(currentWeb, listDefinition);    
}
```

Now, let's create a model class that represents the business-level C# entity corresponding to our content type:

```
public class MyEntity : BaseEntity
{
    [Property(MyFieldDefinitions.MyTextFieldInternalName)]
    public string MyTextField { get; set; }        

    [Property(MyFieldDefinitions.MyHiddenBooleanFieldInternalName)]
    public bool MyHiddenBooleanField { get; set }

    // Automatically mapped if property name and internal column name match
    [Property]
    public DateTime MyDateField { get; set; }

    [Property]
    public TaxonomyValue MyTaxonomyField { get; set; }

    [Property]
    public TaxonomyValueCollection MyTaxonomyMultiField { get; set; }
}
```

Properties in our not-quite-POCOs are decorated with the `GSoft.Dynamite.Binding.Property` attribute, which effectively maps the object properties to site column internal names. Object properties that do not have the `Property` attribute will be ignored by the `SharePointEntityBinder` during mapping.

> Note how Dynamite provides the [`BaseEntity`](https://github.com/GSoft-SharePoint/Dynamite/blob/develop/Source/GSoft.Dynamite/BaseEntity.cs) class that already has common list item properties like `ID`, `Title` and the read-only properties `Modified` and `Created` (use `BindingType.ReadOnly` in such cases, as shown in `BaseEntity`).

#### Mapping from SPListItem to Entity

Now we're ready to rock and whip out the `ISharePointEntityBinder`. Let's fetch some list items (with some help from the `IListLocator` and 
`ICamlBuilder`) and convert them into entities:

```
// Somewhere in a user control...
using (var scope = ProjectContainer.BeginLifetimeScope())
{
    var listLocator = scope.Resolve<IListLocator>();
    var caml = scope.Resolve<ICamlBuilder>();
    var mapper = scope.Resolve<ISharePointEntityBinder>();

    // Using the web-relative URL instead of the list name's 
    // resource key would also work here, thanks IListLocator!
    var list = listLocator.TryGetList(SPContext.Current.Web, "List_MyListTitle");

    // Define a simple CAML query with the ICamlBuilder to
    // avoid string manipulation errors while building the 
    // query markup
    var query = new SPQuery();
    query.Query = caml.Where(caml.BeginsWith(caml.FieldRef(BuiltInFields.Title), caml.Value("MySpecialTitlePrefix")));

    // The method ViewFieldsForEntityType is very handy to 
    // define the set of fields you want returned by the SPQuery
    query.ViewFields = caml.ViewFieldsForEntityType(typeof(MyEntity));

    SPListItemCollection results = query.GetItems(list);

    // The pièce-de-résistance: one-liner to map from SPListItemCollection
    // to a list of MyEntity objects.
    IList<MyEntity> myResultsConvertedToEntities = mapper.Get<MyEntity>(results);

    // Now we can use our easy-to-serialize entity with its 
    // easy-to-navigate ValueType properties
    var firstEntity = myResultsConvertedToEntities.First();

    if (firstEntity.MyTaxonomyValue.Term.Id == MyTermStoreDefinitions.MySpecialSnowflakeTerm.Id)
    {
        // etc.
    }
}
```

To ensure the best mapping performance possible (i.e. to minimize calls to the SharePoint database), make sure you use the method `ICamlBuilder.ViewFieldsForEntityType` to properly define the SELECT component of your `SPQuery`.

> Not initializing your `SPQuery.ViewFields` can lead to one database call being generated for each SPField being accessed later on - how terrible!

Whenever possible, you should also use the `ISharePointEntityBinder.Get<T>` method overload which accepts a `SPListItemCollection`. Behind the scenes, the implementation takes care of applying `ToDataTable` to ensure all items are fetched with a single database call. 

> Looping over a collection of list items can have the unintended and unfortunate consequence of generating one (or more - see comment about `ViewFields` above) database call for each `SPListItem` in the collection.

#### Mapping from Entity to SPListItem

The `ISharePointEntityBinder` can also map in the reverse direction to help you persist your entities as list items. For example:

```
// Somewhere in a user control...
using (var scope = ProjectContainer.BeginLifetimeScope())
{
    var listLocator = scope.Resolve<IListLocator>();
    var mapper = scope.Resolve<ISharePointEntityBinder>();

    var list = listLocator.TryGetList(SPContext.Current.Web, "MyWebRelativeUrl");

    var newItem = list.AddItem();
    var newEntity = new MyEntity()
    {
        MyDateOnlyField = DateTime.Now,
        MyTaxonomyField = new TaxonomyValue(MyTermStoreDefinitions.MySpecialSnowflakeTerm)
    };

    // Map from entity to list item
    mapper.FromEntity<MyEntity>(newEntity, newItem);
    newItem.Update();
}
```

Thanks for reading this guide! Hopefully Dynamite will help you build SharePoint full-trust solutions that are easier to maintain.

Please don't hesitate to leave your comments and questions in the project Issues.
