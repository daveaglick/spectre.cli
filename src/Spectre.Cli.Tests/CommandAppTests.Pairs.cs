using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Shouldly;
using Spectre.Cli.Exceptions;
using Spectre.Cli.Testing;
using Spectre.Cli.Testing.Data.Commands;
using Spectre.Cli.Testing.Data.Converters;
using Spectre.Cli.Testing.Fakes;
using Xunit;

namespace Spectre.Cli.Tests
{
    public sealed partial class CommandAppTests
    {
        public sealed class Pairs
        {
            public sealed class AmbiguousSettings : CommandSettings
            {
                [CommandOption("--var <VALUE>")]
                [PairDeconstructor(typeof(StringIntDeconstructor))]
                [TypeConverter(typeof(CatAgilityConverter))]
                public ILookup<string, string> Values { get; set; }
            }

            public sealed class NotDeconstructableSettings : CommandSettings
            {
                [CommandOption("--var <VALUE>")]
                [PairDeconstructor(typeof(StringIntDeconstructor))]
                public string Values { get; set; }
            }

            public sealed class DefaultPairDeconstructorSettings : CommandSettings
            {
                [CommandOption("--var <VALUE>")]
                public IDictionary<string, int> Values { get; set; }
            }

            public sealed class LookupSettings : CommandSettings
            {
                [CommandOption("--var <VALUE>")]
                [PairDeconstructor(typeof(StringIntDeconstructor))]
                public ILookup<string, string> Values { get; set; }
            }

            public sealed class DictionarySettings : CommandSettings
            {
                [CommandOption("--var <VALUE>")]
                [PairDeconstructor(typeof(StringIntDeconstructor))]
                public IDictionary<string, string> Values { get; set; }
            }

            public sealed class ReadOnlyDictionarySettings : CommandSettings
            {
                [CommandOption("--var <VALUE>")]
                [PairDeconstructor(typeof(StringIntDeconstructor))]
                public IReadOnlyDictionary<string, string> Values { get; set; }
            }

            public sealed class StringIntDeconstructor : PairDeconstuctor<string, string>
            {
                protected override (string Key, string Value) Deconstruct(string value)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    var parts = value.Split(new[] { '=' });
                    if (parts.Length != 2)
                    {
                        throw new InvalidOperationException("Could not parse pair");
                    }

                    return (parts[0], parts[1]);
                }
            }

            [Fact]
            public void Should_Throw_If_Option_Has_Pair_Deconstructor_And_Type_Converter()
            {
                // Given
                var app = new CommandApp<GenericCommand<AmbiguousSettings>>();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = Record.Exception(() => app.Run(new[]
                {
                    "--var", "foo=bar",
                    "--var", "foo=qux",
                }));

                // Then
                result.ShouldBeOfType<ConfigurationException>().And(ex =>
                {
                    ex.Message.ShouldBe("The option 'var' is both marked as pair deconstructable and convertable.");
                });
            }

            [Fact]
            public void Should_Throw_If_Option_Has_Pair_Deconstructor_But_Type_Is_Not_Deconstructable()
            {
                // Given
                var app = new CommandApp<GenericCommand<NotDeconstructableSettings>>();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = Record.Exception(() => app.Run(new[]
                {
                    "--var", "foo=bar",
                    "--var", "foo=qux",
                }));

                // Then
                result.ShouldBeOfType<ConfigurationException>().And(ex =>
                {
                    ex.Message.ShouldBe("The option 'var' is marked as pair deconstructable, but the underlying type does not support that.");
                });
            }

            [Fact]
            public void Should_Map_Pairs_To_Pair_Deconstructable_Collection_Using_Default_Deconstructort()
            {
                // Given
                var resolver = new FakeTypeResolver();
                var settings = new DefaultPairDeconstructorSettings();
                resolver.Register(settings);

                var app = new CommandApp<GenericCommand<DefaultPairDeconstructorSettings>>(new FakeTypeRegistrar(resolver));
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = app.Run(new[]
                {
                    "--var", "foo=1",
                    "--var", "foo=3",
                    "--var", "bar=4",
                });

                // Then
                result.ShouldBe(0);
                settings.Values.ShouldNotBeNull();
                settings.Values.Count.ShouldBe(2);
                settings.Values["foo"].ShouldBe(3);
                settings.Values["bar"].ShouldBe(4);
            }

            [Fact]
            public void Should_Map_Lookup_Values()
            {
                // Given
                var resolver = new FakeTypeResolver();
                var settings = new LookupSettings();
                resolver.Register(settings);

                var app = new CommandApp<GenericCommand<LookupSettings>>(new FakeTypeRegistrar(resolver));
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = app.Run(new[]
                {
                    "--var", "foo=bar",
                    "--var", "foo=qux",
                });

                // Then
                result.ShouldBe(0);
                settings.Values.ShouldNotBeNull();
                settings.Values.Count.ShouldBe(1);
                settings.Values["foo"].ToList().Count.ShouldBe(2);
            }

            [Fact]
            public void Should_Map_Dictionary_Values()
            {
                // Given
                var resolver = new FakeTypeResolver();
                var settings = new DictionarySettings();
                resolver.Register(settings);

                var app = new CommandApp<GenericCommand<DictionarySettings>>(new FakeTypeRegistrar(resolver));
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = app.Run(new[]
                {
                    "--var", "foo=bar",
                    "--var", "baz=qux",
                });

                // Then
                result.ShouldBe(0);
                settings.Values.ShouldNotBeNull();
                settings.Values.Count.ShouldBe(2);
                settings.Values["foo"].ShouldBe("bar");
                settings.Values["baz"].ShouldBe("qux");
            }

            [Fact]
            public void Should_Map_Latest_Value_Of_Same_Key_When_Mapping_To_Dictionary()
            {
                // Given
                var resolver = new FakeTypeResolver();
                var settings = new DictionarySettings();
                resolver.Register(settings);

                var app = new CommandApp<GenericCommand<DictionarySettings>>(new FakeTypeRegistrar(resolver));
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = app.Run(new[]
                {
                    "--var", "foo=bar",
                    "--var", "foo=qux",
                });

                // Then
                result.ShouldBe(0);
                settings.Values.ShouldNotBeNull();
                settings.Values.Count.ShouldBe(1);
                settings.Values["foo"].ShouldBe("qux");
            }

            [Fact]
            public void Should_Map_ReadOnly_Dictionary_Values()
            {
                // Given
                var resolver = new FakeTypeResolver();
                var settings = new ReadOnlyDictionarySettings();
                resolver.Register(settings);

                var app = new CommandApp<GenericCommand<ReadOnlyDictionarySettings>>(new FakeTypeRegistrar(resolver));
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                });

                // When
                var result = app.Run(new[]
                {
                    "--var", "foo=bar",
                    "--var", "baz=qux",
                });

                // Then
                result.ShouldBe(0);
                settings.Values.ShouldNotBeNull();
                settings.Values.Count.ShouldBe(2);
                settings.Values["foo"].ShouldBe("bar");
                settings.Values["baz"].ShouldBe("qux");
            }
        }
    }
}
