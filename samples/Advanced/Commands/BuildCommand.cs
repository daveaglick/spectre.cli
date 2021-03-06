﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Cli;

namespace Sample.Commands
{
    [Description("Builds a project and all of its dependencies.")]
    public sealed class BuildCommand : AsyncCommand<BuildSettings>
    {
        private readonly IFileSystem _fileSystem;

        public BuildCommand(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        // For validation that requires more logic and/or dependencies.
        public override ValidationResult Validate(CommandContext context, BuildSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.Project))
            {
                if (!_fileSystem.FileExist(settings.Project))
                {
                    return ValidationResult.Error("The specified project do not exist.");
                }
            }
            return base.Validate(context, settings);
        }

        public override async Task<int> Execute(CommandContext context, BuildSettings settings)
        {
            if (!settings.NoRestore)
            {
                // Pretend we're restoring packages.
                Console.WriteLine("Restoring packages...");
            }
            
            // Get the project name.
            var project = settings.Project;
            if (string.IsNullOrWhiteSpace(project))
            {
                project = "random.csproj";
            }
            
            // Pretend we're building.
            Console.WriteLine($"Building {project}...");
            await Task.Delay(1000);
            Console.WriteLine("Build completed!");

            // Return success.
            return 0;
        }
    }
}