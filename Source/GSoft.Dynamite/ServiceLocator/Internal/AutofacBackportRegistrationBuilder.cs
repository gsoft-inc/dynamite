using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace GSoft.Dynamite.ServiceLocator.Internal
{
    /// <summary>
    /// Internal <c>Autofac</c> copied here to back port an assembly scanning fix
    /// </summary>
    /// <typeparam name="TLimit">The limit</typeparam>
    /// <typeparam name="TActivatorData">The activator data</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style</typeparam>
    internal class RegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> : IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>, IHideObjectMembers
    {
        private readonly TActivatorData activatorData;
        private readonly TRegistrationStyle registrationStyle;
        private readonly RegistrationData registrationData;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="defaultService">The default service</param>
        /// <param name="activatorData">The activator data</param>
        /// <param name="style">The registration style</param>
        public RegistrationBuilder(Service defaultService, TActivatorData activatorData, TRegistrationStyle style)
        {
            if (defaultService == null)
            {
                throw new ArgumentNullException("defaultService");
            }

            if ((object)activatorData == null)
            {
                throw new ArgumentNullException("activatorData");
            }

            if ((object)style == null)
            {
                throw new ArgumentNullException("style");
            }

            this.activatorData = activatorData;
            this.registrationStyle = style;
            this.registrationData = new RegistrationData(defaultService);
        }

        /// <summary>
        /// The activator data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TActivatorData ActivatorData
        {
            get
            {
                return this.activatorData;
            }
        }

        /// <summary>
        /// The registration style.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TRegistrationStyle RegistrationStyle
        {
            get
            {
                return this.registrationStyle;
            }
        }

        /// <summary>
        /// The registration data.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RegistrationData RegistrationData
        {
            get
            {
                return this.registrationData;
            }
        }

        /// <summary>
        /// Configure the component so that instances are never disposed by the container.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ExternallyOwned()
        {
            this.RegistrationData.Ownership = InstanceOwnership.ExternallyOwned;
            return this;
        }

        /// <summary>
        /// Configure the component so that instances that support IDisposable are
        /// disposed by the container (default.)
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OwnedByLifetimeScope()
        {
            this.RegistrationData.Ownership = InstanceOwnership.OwnedByLifetimeScope;
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets a new, unique instance (default.)
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerDependency()
        {
            this.RegistrationData.Sharing = InstanceSharing.None;
            this.RegistrationData.Lifetime = new CurrentScopeLifetime();
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// gets the same, shared instance.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SingleInstance()
        {
            this.RegistrationData.Sharing = InstanceSharing.Shared;
            this.RegistrationData.Lifetime = new RootScopeLifetime();
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a single ILifetimeScope gets the same, shared instance. Dependent components in
        /// different lifetime scopes will get different instances.
        /// </summary>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerLifetimeScope()
        {
            this.RegistrationData.Sharing = InstanceSharing.Shared;
            this.RegistrationData.Lifetime = new CurrentScopeLifetime();
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope tagged with the provided tag value gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the tagged scope will
        /// share the parent's instance. If no appropriately tagged scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="lifetimeScopeTag">Tag applied to matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerMatchingLifetimeScope(object lifetimeScopeTag)
        {
            if (lifetimeScopeTag == null)
            {
                throw new ArgumentNullException("lifetimeScopeTag");
            }

            this.RegistrationData.Sharing = InstanceSharing.Shared;
            this.RegistrationData.Lifetime = new MatchingScopeLifetime(lifetimeScopeTag);
            return this;
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>()
        {
            return this.InstancePerOwned(typeof(TService));
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(Type serviceType)
        {
            return this.InstancePerMatchingLifetimeScope(new TypedService(serviceType));
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned<TService>(object serviceKey)
        {
            return this.InstancePerOwned(serviceKey, typeof(TService));
        }

        /// <summary>
        /// Configure the component so that every dependent component or call to Resolve()
        /// within a ILifetimeScope created by an owned instance gets the same, shared instance.
        /// Dependent components in lifetime scopes that are children of the owned instance scope will
        /// share the parent's instance. If no appropriate owned instance scope can be found in the
        /// hierarchy an <see cref="DependencyResolutionException"/> is thrown.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerOwned(object serviceKey, Type serviceType)
        {
            return this.InstancePerMatchingLifetimeScope(new KeyedService(serviceKey, serviceType));
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService>()
        {
            return this.As(new TypedService(typeof(TService)));
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService1">Service type number 1.</typeparam>
        /// <typeparam name="TService2">Service type number 2.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2>()
        {
            return this.As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)));
        }

        /// <summary>
        /// Configure the services that the component will provide. The generic parameter(s) to As()
        /// will be exposed as TypedService instances.
        /// </summary>
        /// <typeparam name="TService1">Service type number 1.</typeparam>
        /// <typeparam name="TService2">Service type number 2.</typeparam>
        /// <typeparam name="TService3">Service type number 3.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As<TService1, TService2, TService3>()
        {
            return this.As(new TypedService(typeof(TService1)), new TypedService(typeof(TService2)), new TypedService(typeof(TService3)));
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Service types to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Type[] services)
        {
            return this.As(services.Select(t => t.FullName != null ? new TypedService(t) : new TypedService(t.GetGenericTypeDefinition())).Cast<Service>().ToArray());
        }

        /// <summary>
        /// Configure the services that the component will provide.
        /// </summary>
        /// <param name="services">Services to expose.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> As(params Service[] services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            this.RegistrationData.AddServices(services);

            return this;
        }

        /// <summary>
        /// Provide a textual name that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named(string serviceName, Type serviceType)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException("serviceName");
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            return this.As(new KeyedService(serviceName, serviceType));
        }

        /// <summary>
        /// Provide a textual name that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceName">Named service to associate with the component.</param>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Named<TService>(string serviceName)
        {
            return this.Named(serviceName, typeof(TService));
        }

        /// <summary>
        /// Provide a key that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <param name="serviceType">The service type provided by the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed(object serviceKey, Type serviceType)
        {
            if (serviceKey == null)
            {
                throw new ArgumentNullException("serviceKey");
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            return this.As(new KeyedService(serviceKey, serviceType));
        }

        /// <summary>
        /// Provide a key that can be used to retrieve the component.
        /// </summary>
        /// <param name="serviceKey">Key to associate with the component.</param>
        /// <typeparam name="TService">The service type provided by the component.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Keyed<TService>(object serviceKey)
        {
            return this.Keyed(serviceKey, typeof(TService));
        }

        /// <summary>
        /// Add a handler for the Preparing event. This event allows manipulating of the parameters
        /// that will be provided to the component.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnPreparing(Action<PreparingEventArgs> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            this.RegistrationData.PreparingHandlers.Add((s, e) => handler(e));
            return this;
        }

        /// <summary>
        /// Add a handler for the Activating event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivating(Action<IActivatingEventArgs<TLimit>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            this.RegistrationData.ActivatingHandlers.Add((s, e) =>
            {
                var args = new ActivatingEventArgs<TLimit>(e.Context, e.Component, e.Parameters, (TLimit)e.Instance);
                handler(args);
                e.Instance = args.Instance;
            });
            return this;
        }

        /// <summary>
        /// Add a handler for the Activated event.
        /// </summary>
        /// <param name="handler">The event handler.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OnActivated(Action<IActivatedEventArgs<TLimit>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            this.RegistrationData.ActivatedHandlers.Add(
                (s, e) => handler(new ActivatedEventArgs<TLimit>(e.Context, e.Component, e.Parameters, (TLimit)e.Instance)));
            return this;
        }

        /// <summary>
        /// Configure the component so that any properties whose types are registered in the
        /// container will be wired to instances of the appropriate service.
        /// </summary>
        /// <param name="wiringFlags">Set wiring options such as circular dependency wiring support.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesAutowired(PropertyWiringFlags wiringFlags)
        {
            var injector = new AutowiringPropertyInjector();

            var allowCircularDependencies = 0 != (int)(wiringFlags & PropertyWiringFlags.AllowCircularDependencies);
            var preserveSetValues = 0 != (int)(wiringFlags & PropertyWiringFlags.PreserveSetValues);

            if (allowCircularDependencies)
            {
                this.RegistrationData.ActivatedHandlers.Add((s, e) => injector.InjectProperties(e.Context, e.Instance, !preserveSetValues));
            }
            else
            {
                this.RegistrationData.ActivatingHandlers.Add((s, e) => injector.InjectProperties(e.Context, e.Instance, !preserveSetValues));
            }

            return this;
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="key">Key by which the data can be located.</param>
        /// <param name="value">The data value.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            this.RegistrationData.Metadata.Add(key, value);

            return this;
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <param name="properties">The extended properties to associate with the component.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata(IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            foreach (var prop in properties)
            {
                this.WithMetadata(prop.Key, prop.Value);
            }

            return this;
        }

        /// <summary>
        /// Associates data with the component.
        /// </summary>
        /// <typeparam name="TMetadata">A type with properties whose names correspond to the property names to configure.</typeparam>
        /// <param name="configurationAction">The configuration action</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithMetadata<TMetadata>(Action<MetadataConfiguration<TMetadata>> configurationAction)
        {
            // Autofac backport FAIL: the MetadataConfiguration has an internal Properties dictionary needed for this to work 
            // and then it at that point it would become easier to re-build a patched old Autofac build 
            throw new NotImplementedException("Custom Dynamite Autofac backport does not support WithMetadata RegistrationBuilder method!!!");

            ////if (configurationAction == null) throw new ArgumentNullException("configurationAction");

            ////var epConfiguration = new MetadataConfiguration<TMetadata>();
            ////configurationAction(epConfiguration);
            ////return WithMetadata(epConfiguration.Properties);
        }
    }
}
