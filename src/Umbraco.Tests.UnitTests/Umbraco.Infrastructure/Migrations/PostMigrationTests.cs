﻿using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Migrations
{
    [TestFixture]
    public class PostMigrationTests
    {
        private static readonly ILoggerFactory s_loggerFactory = NullLoggerFactory.Instance;

        [Test]
        public void ExecutesPlanPostMigration()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case nameof(NoopMigration):
                            return new NoopMigration();
                        case nameof(TestPostMigration):
                            return new TestPostMigration();
                        default:
                            throw new NotSupportedException();
                    }
                });

            var database = new TestDatabase();
            IScope scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var sqlContext = new SqlContext(
                new SqlServerSyntaxProvider(),
                DatabaseType.SQLCe,
                Mock.Of<IPocoDataFactory>());
            var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            MigrationPlan plan = new MigrationPlan("Test")
                .From(string.Empty).To("done");

            plan.AddPostMigration<TestPostMigration>();
            TestPostMigration.MigrateCount = 0;

            var upgrader = new Upgrader(plan);
            upgrader.Execute(
                scopeProvider,
                builder,
                Mock.Of<IKeyValueService>(),
                s_loggerFactory.CreateLogger<Upgrader>(),
                s_loggerFactory);

            Assert.AreEqual(1, TestPostMigration.MigrateCount);
        }

        [Test]
        public void MigrationCanAddPostMigration()
        {
            IMigrationBuilder builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case nameof(NoopMigration):
                            return new NoopMigration();
                        case nameof(TestMigration):
                            return new TestMigration(c);
                        case nameof(TestPostMigration):
                            return new TestPostMigration();
                        default:
                            throw new NotSupportedException();
                    }
                });

            var database = new TestDatabase();
            IScope scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var sqlContext = new SqlContext(
                new SqlServerSyntaxProvider(),
                DatabaseType.SQLCe,
                Mock.Of<IPocoDataFactory>());
            var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            MigrationPlan plan = new MigrationPlan("Test")
                .From(string.Empty).To<TestMigration>("done");

            TestMigration.MigrateCount = 0;
            TestPostMigration.MigrateCount = 0;

            new MigrationContext(database, s_loggerFactory.CreateLogger<MigrationContext>());

            var upgrader = new Upgrader(plan);
            upgrader.Execute(
                scopeProvider,
                builder,
                Mock.Of<IKeyValueService>(),
                s_loggerFactory.CreateLogger<Upgrader>(),
                s_loggerFactory);

            Assert.AreEqual(1, TestMigration.MigrateCount);
            Assert.AreEqual(1, TestPostMigration.MigrateCount);
        }

        public class TestMigration : MigrationBase
        {
            public TestMigration(IMigrationContext context)
                : base(context)
            {
            }

            public static int MigrateCount { get; set; }

            public override void Migrate()
            {
                MigrateCount++;

                Context.AddPostMigration<TestPostMigration>();
            }
        }

        public class TestPostMigration : IMigration
        {
            public static int MigrateCount { get; set; }

            public void Migrate() => MigrateCount++;
        }
    }
}
