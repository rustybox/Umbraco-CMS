﻿using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    public class AdvancedMigrationTests : UmbracoIntegrationTest
    {
        private ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

        private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();

        [Test]
        public void CreateTableOfTDto()
        {
            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(CreateTableOfTDtoMigration))
                        throw new NotSupportedException();
                    return new CreateTableOfTDtoMigration(c);
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);

                var helper = new DatabaseSchemaCreator(scope.Database, LoggerFactory.CreateLogger<DatabaseSchemaCreator>(), LoggerFactory, UmbracoVersion);
                var exists = helper.TableExists("umbracoUser");
                Assert.IsTrue(exists);

                scope.Complete();
            }
        }

        [Test]
        public void DeleteKeysAndIndexesOfTDto()
        {
            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "DeleteKeysAndIndexesMigration":
                            return new DeleteKeysAndIndexesMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<DeleteKeysAndIndexesMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexesOfTDto()
        {
            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "DeleteKeysAndIndexesMigration":
                            return new DeleteKeysAndIndexesMigration(c);
                        case "CreateKeysAndIndexesOfTDtoMigration":
                            return new CreateKeysAndIndexesOfTDtoMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<DeleteKeysAndIndexesMigration>("b")
                        .To<CreateKeysAndIndexesOfTDtoMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        [Test]
        public void CreateKeysAndIndexes()
        {
            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "DeleteKeysAndIndexesMigration":
                            return new DeleteKeysAndIndexesMigration(c);
                        case "CreateKeysAndIndexesMigration":
                            return new CreateKeysAndIndexesMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<DeleteKeysAndIndexesMigration>("b")
                        .To<CreateKeysAndIndexesMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        [Test]
        public void CreateColumn()
        {
            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "CreateTableOfTDtoMigration":
                            return new CreateTableOfTDtoMigration(c);
                        case "CreateColumnMigration":
                            return new CreateColumnMigration(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            using (var scope = ScopeProvider.CreateScope())
            {
                var upgrader = new Upgrader(
                    new MigrationPlan("test")
                        .From(string.Empty)
                        .To<CreateTableOfTDtoMigration>("a")
                        .To<CreateColumnMigration>("done"));

                upgrader.Execute(ScopeProvider, builder, Mock.Of<IKeyValueService>(), _loggerFactory.CreateLogger<Upgrader>(), _loggerFactory);
                scope.Complete();
            }
        }

        public class CreateTableOfTDtoMigration : MigrationBase
        {
            public CreateTableOfTDtoMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // creates User table with keys, indexes, etc
                Create.Table<UserDto>().Do();
            }
        }

        public class DeleteKeysAndIndexesMigration : MigrationBase
        {
            public DeleteKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // drops User table keys and indexes
                //Execute.DropKeysAndIndexes("umbracoUser");

                // drops *all* tables keys and indexes
                var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToList();
                foreach (var table in tables)
                    Delete.KeysAndIndexes(table, false, true).Do();
                foreach (var table in tables)
                    Delete.KeysAndIndexes(table, true, false).Do();
            }
        }

        public class CreateKeysAndIndexesOfTDtoMigration : MigrationBase
        {
            public CreateKeysAndIndexesOfTDtoMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // creates Node table keys and indexes
                Create.KeysAndIndexes<UserDto>().Do();
            }
        }

        public class CreateKeysAndIndexesMigration : MigrationBase
        {
            public CreateKeysAndIndexesMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // creates *all* tables keys and indexes
                foreach (var x in DatabaseSchemaCreator.OrderedTables)
                {
                    // ok - for tests, restrict to Node
                    if (x != typeof(UserDto)) continue;

                    Create.KeysAndIndexes(x).Do();
                }
            }
        }

        public class CreateColumnMigration : MigrationBase
        {
            public CreateColumnMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // cannot delete the column without this, of course
                Delete.KeysAndIndexes("umbracoUser").Do();

                Delete.Column("id").FromTable("umbracoUser").Do();

                var table = DefinitionFactory.GetTableDefinition(typeof(UserDto), SqlSyntax);
                var column = table.Columns.First(x => x.Name == "id");
                var create = SqlSyntax.Format(column); // returns [id] INTEGER NOT NULL IDENTITY(1060,1)
                Database.Execute($"ALTER TABLE {SqlSyntax.GetQuotedTableName("umbracoUser")} ADD " + create);
            }
        }
    }
}
