﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.Collections;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;

namespace Volo.Abp.EventBus.Distributed
{
    [Dependency(TryRegister = true)]
    [ExposeServices(typeof(IDistributedEventBus), typeof(LocalDistributedEventBus))]
    public class LocalDistributedEventBus : IDistributedEventBus, ITransientDependency
    {
        private readonly ILocalEventBus _localEventBus;
        protected IHybridServiceScopeFactory ServiceScopeFactory { get; }
        protected DistributedEventBusOptions DistributedEventBusOptions { get; }

        public LocalDistributedEventBus(
            ILocalEventBus localEventBus,
            IHybridServiceScopeFactory serviceScopeFactory,
            IOptions<DistributedEventBusOptions> distributedEventBusOptions)
        {
            _localEventBus = localEventBus;
            ServiceScopeFactory = serviceScopeFactory;
            DistributedEventBusOptions = distributedEventBusOptions.Value;
            Subscribe(distributedEventBusOptions.Value.Handlers);
        }

        public virtual void Subscribe(ITypeList<IEventHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(@interface))
                    {
                        continue;
                    }

                    var genericArgs = @interface.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        Subscribe(genericArgs[0], new IocEventHandlerFactory(ServiceScopeFactory, handler));
                    }
                }
            }
        }

        public IDisposable Subscribe<TEvent>(Func<TEvent, Task> action) where TEvent : class
        {
            return _localEventBus.Subscribe(action);
        }

        public IDisposable Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class
        {
            return _localEventBus.Subscribe(handler);
        }

        public IDisposable Subscribe<TEvent, THandler>() where TEvent : class where THandler : IEventHandler, new()
        {
            return _localEventBus.Subscribe<TEvent, THandler>();
        }

        public IDisposable Subscribe(Type eventType, IEventHandler handler)
        {
            return _localEventBus.Subscribe(eventType, handler);
        }

        public IDisposable Subscribe<TEvent>(IEventHandlerFactory factory) where TEvent : class
        {
            return _localEventBus.Subscribe<TEvent>(factory);
        }

        public IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
        {
            return _localEventBus.Subscribe(eventType, factory);
        }

        public void Unsubscribe<TEvent>(Func<TEvent, Task> action) where TEvent : class
        {
            _localEventBus.Unsubscribe(action);
        }

        public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class
        {
            _localEventBus.Unsubscribe(handler);
        }

        public void Unsubscribe(Type eventType, IEventHandler handler)
        {
            _localEventBus.Unsubscribe(eventType, handler);
        }

        public void Unsubscribe<TEvent>(IEventHandlerFactory factory) where TEvent : class
        {
            _localEventBus.Unsubscribe<TEvent>(factory);
        }

        public void Unsubscribe(Type eventType, IEventHandlerFactory factory)
        {
            _localEventBus.Unsubscribe(eventType, factory);
        }

        public void UnsubscribeAll<TEvent>() where TEvent : class
        {
            _localEventBus.UnsubscribeAll<TEvent>();
        }

        public void UnsubscribeAll(Type eventType)
        {
            _localEventBus.UnsubscribeAll(eventType);
        }

        public Task PublishAsync<TEvent>(TEvent eventData)
            where TEvent : class
        {
            return _localEventBus.PublishAsync(eventData);
        }

        public Task PublishAsync(Type eventType, object eventData)
        {
            return _localEventBus.PublishAsync(eventType, eventData);
        }
    }
}