using System;
using System.Collections.Generic;
using Shouldly;
using Spectre.Cli.Internal;
using Spectre.Cli.Internal.Configuration;
using Spectre.Cli.Internal.Modelling;
using Spectre.Cli.Internal.Parsing;
using Spectre.Cli.Tests.Data;
using Spectre.Cli.Tests.Data.Settings;
using Xunit;

namespace Spectre.Cli.Tests.Unit.Internal
{
    public sealed class CommandTreeBinderTests
    {
        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-1
        /// </summary>
        [Fact]
        public void Should_Bind_Parameters_Correctly_For_Case_1()
        {
            // Given, When
            var settings = Fixture.Bind<DogSettings>(
                new[] { "animal", "--alive", "mammal", "--name", "Rufus", "dog", "12", "--good-boy" },
                config =>
                {
                    config.AddBranch<AnimalSettings>("animal", animal =>
                    {
                        animal.AddBranch<MammalSettings>("mammal", mammal =>
                        {
                            mammal.AddCommand<DogCommand>("dog");
                            mammal.AddCommand<HorseCommand>("horse");
                        });
                    });
                });

            // Then
            settings.Age.ShouldBe(12);
            settings.GoodBoy.ShouldBe(true);
            settings.Name.ShouldBe("Rufus");
            settings.IsAlive.ShouldBe(true);
        }

        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-2
        /// </summary>
        [Fact]
        public void Should_Bind_Parameters_Correctly_For_Case_2()
        {
            // Given, When
            var settings = Fixture.Bind<DogSettings>(
                new[] { "dog", "12", "4", "--good-boy", "--name", "Rufus", "--alive" },
                config =>
                {
                    config.AddCommand<DogCommand>("dog");
                });

            // Then
            settings.Legs.ShouldBe(12);
            settings.Age.ShouldBe(4);
            settings.GoodBoy.ShouldBe(true);
            settings.Name.ShouldBe("Rufus");
            settings.IsAlive.ShouldBe(true);
        }

        /// <summary>
        /// https://github.com/spectresystems/spectre.cli/wiki/Test-cases#test-case-3
        /// </summary>
        [Fact]
        public void Should_Bind_Parameters_Correctly_For_Case_3()
        {
            // Given, When
            var settings = Fixture.Bind<DogSettings>(
                new[] { "animal", "dog", "12", "--good-boy", "--name", "Rufus" },
                config =>
                {
                    config.AddBranch<AnimalSettings>("animal", animal =>
                    {
                        animal.AddCommand<DogCommand>("dog");
                        animal.AddCommand<HorseCommand>("horse");
                    });
                });

            // Then
            settings.Age.ShouldBe(12);
            settings.GoodBoy.ShouldBe(true);
            settings.Name.ShouldBe("Rufus");
            settings.IsAlive.ShouldBe(false);
        }

        [Fact]
        public void Should_Bind_Using_Custom_Type_Converter_If_Specified()
        {
            // Given, When
            var settings = Fixture.Bind<CatSettings>(
                new[] { "cat", "--name", "Tiger", "--agility", "FOOBAR" },
                config =>
                {
                    config.AddCommand<CatCommand>("cat");
                });

            // Then
            settings.Agility.ShouldBe(6);
        }

        internal static class Fixture
        {
            public static T Bind<T>(IEnumerable<string> args, Action<Configurator> action)
                where T : CommandSettings, new()
            {
                // Configure
                var configurator = new Configurator(null);
                action(configurator);

                // Parse command tree.
                var parser = new CommandTreeParser(CommandModelBuilder.Build(configurator));
                var result = parser.Parse(args);

                // Bind the settings to the tree.
                CommandSettings settings = new T();
                CommandBinder.Bind(result.Tree, ref settings, new TypeResolverAdapter(null));

                // Return the settings.
                return (T)settings;
            }
        }
    }
}
