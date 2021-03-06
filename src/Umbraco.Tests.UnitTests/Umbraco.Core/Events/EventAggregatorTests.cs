// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Events;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Events
{
    public class EventAggregatorTests
    {
        private const int A = 3;
        private const int B = 5;
        private const int C = 7;
        private IUmbracoBuilder _builder;

        [SetUp]
        public void Setup()
        {
            IServiceCollection register = TestHelper.GetServiceCollection();
            _builder = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());
        }

        [Test]
        public async Task CanPublishEvents()
        {
            _builder.Services.AddScoped<Adder>();
            _builder.AddNotificationHandler<Notification, NotificationHandlerA>();
            _builder.AddNotificationHandler<Notification, NotificationHandlerB>();
            _builder.AddNotificationHandler<Notification, NotificationHandlerC>();
            ServiceProvider provider = _builder.Services.BuildServiceProvider();

            var notification = new Notification();
            IEventAggregator aggregator = provider.GetService<IEventAggregator>();
            await aggregator.PublishAsync(notification);

            Assert.AreEqual(A + B + C, notification.SubscriberCount);
        }

        public class Notification : INotification
        {
            public int SubscriberCount { get; set; }
        }

        public class NotificationHandlerA : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
            {
                notification.SubscriberCount += A;
                return Task.CompletedTask;
            }
        }

        public class NotificationHandlerB : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
            {
                notification.SubscriberCount += B;
                return Task.CompletedTask;
            }
        }

        public class NotificationHandlerC : INotificationHandler<Notification>
        {
            private readonly Adder _adder;

            public NotificationHandlerC(Adder adder) => _adder = adder;

            public Task HandleAsync(Notification notification, CancellationToken cancellationToken)
            {
                notification.SubscriberCount = _adder.Add(notification.SubscriberCount, C);
                return Task.CompletedTask;
            }
        }

        public class Adder
        {
            public int Add(int a, int b) => a + b;
        }
    }
}
