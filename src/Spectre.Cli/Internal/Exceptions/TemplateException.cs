﻿using Spectre.Cli.Internal.Configuration;
using Spectre.Cli.Internal.Rendering;

namespace Spectre.Cli.Internal.Exceptions
{
    internal sealed class TemplateException : ConfigurationException
    {
        public string Template { get; }
        public override bool AlwaysPropagateWhenDebugging => true;

        public TemplateException(string message, string template, IRenderable pretty)
            : base(message, pretty)
        {
            Template = template;
        }

        public static TemplateException UnexpectedCharacter(string template, int position, char character)
        {
            return TemplateExceptionFactory.Create(template,
                new TemplateToken(TemplateToken.Kind.Unknown, position, $"{character}", $"{character}"),
                $"Encountered unexpected character '{character}'.",
                "Unexpected character.");
        }

        public static TemplateException UnterminatedValueName(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                $"Encountered unterminated value name '{token.Value}'.",
                "Unterminated value name.");
        }

        public static TemplateException ArgumentCannotContainOptions(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Arguments can not contain options.",
                "Not permitted.");
        }

        public static TemplateException MultipleValuesAreNotSupported(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Multiple values are not supported.",
                "Too many values.");
        }

        public static TemplateException ValuesMustHaveName(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Values without name are not allowed.",
                "Missing value name.");
        }

        public static TemplateException OptionsMustHaveName(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Options without name are not allowed.",
                "Missing option name.");
        }

        public static TemplateException OptionNamesCannotStartWithDigit(string template, TemplateToken token)
        {
            // Rewrite the token to point to the option name instead of the whole string.
            token = new TemplateToken(token.TokenKind,
                token.TokenKind == TemplateToken.Kind.ShortName ? token.Position + 1 : token.Position + 2,
                token.Value, token.Value);

            return TemplateExceptionFactory.Create(template, token,
                "Option names cannot start with a digit.",
                "Invalid option name.");
        }

        public static TemplateException InvalidCharacterInOptionName(string template, TemplateToken token, char character)
        {
            // Rewrite the token to point to the invalid character instead of the whole value.
            token = new TemplateToken(token.TokenKind,
                (token.TokenKind == TemplateToken.Kind.ShortName ? token.Position + 1 : token.Position + 2)
                    + token.Value.IndexOf(character),
                token.Value, character.ToString());

            return TemplateExceptionFactory.Create(template, token,
                $"Encountered invalid character '{character}' in option name.",
                "Invalid character.");
        }

        public static TemplateException LongOptionMustHaveMoreThanOneCharacter(string template, TemplateToken token)
        {
            // Rewrite the token to point to the option name instead of the whole option.
            token = new TemplateToken(token.TokenKind, token.Position + 2, token.Value, token.Value);

            return TemplateExceptionFactory.Create(template, token,
                "Long option names must consist of more than one character.",
                "Invalid option name.");
        }

        public static TemplateException MultipleShortOptionNamesNotAllowed(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Multiple short option names are not supported.",
                "Too many short options.");
        }

        public static TemplateException ShortOptionMustOnlyBeOneCharacter(string template, TemplateToken token)
        {
            // Rewrite the token to point to the option name instead of the whole option.
            token = new TemplateToken(token.TokenKind, token.Position + 1, token.Value, token.Value);

            return TemplateExceptionFactory.Create(template, token,
                "Short option names can not be longer than one character.",
                "Invalid option name.");
        }

        public static TemplateException MultipleOptionValuesAreNotSupported(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Multiple option values are not supported.",
                "Too many option values.");
        }

        public static TemplateException OptionValueCannotBeOptional(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "Option values cannot be optional.",
                "Must be required.");
        }

        public static TemplateException InvalidCharacterInValueName(string template, TemplateToken token, char character)
        {
            // Rewrite the token to point to the invalid character instead of the whole value.
            token = new TemplateToken(token.TokenKind,
                token.Position + 1 + token.Value.IndexOf(character),
                token.Value, character.ToString());

            return TemplateExceptionFactory.Create(template, token,
                $"Encountered invalid character '{character}' in value name.",
                "Invalid character.");
        }

        public static TemplateException MissingLongAndShortName(string template, TemplateToken token)
        {
            return TemplateExceptionFactory.Create(template, token,
                "No long or short name for option has been specified.",
                "Missing option. Was this meant to be an argument?");
        }
    }
}
