﻿using System.Collections.Generic;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace BenchmarkDotNet.Toolchains.NativeAot
{
    public class NativeAotToolchain : Toolchain
    {
        /// <summary>
        /// compiled as net6.0, targets experimental 6.0.0-* NativeAOT build from the https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json
        /// </summary>
        public static readonly IToolchain Net60 = CreateBuilder()
            .UseNuGet("6.0.0-*", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json")
            .TargetFrameworkMoniker("net6.0")
            .ToToolchain();

        /// <summary>
        /// compiled as net7.0, targets latest NativeAOT build from the NuGet.org feed
        /// </summary>
        public static readonly IToolchain Net70 = CreateBuilder()
            .UseNuGet("", "https://api.nuget.org/v3/index.json")
            .TargetFrameworkMoniker("net7.0")
            .ToToolchain();

        /// <summary>
        /// compiled as net8.0, targets latest NativeAOT build from the NuGet.org feed: "https://api.nuget.org/v3/index.json"
        /// </summary>
        public static readonly IToolchain Net80 = CreateBuilder()
            .UseNuGet("", "https://api.nuget.org/v3/index.json")
            .TargetFrameworkMoniker("net8.0")
            .ToToolchain();

        /// <summary>
        /// compiled as net9.0, targets latest NativeAOT build from the .NET 9 feed: https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json
        /// </summary>
        public static readonly IToolchain Net90 = CreateBuilder()
            .UseNuGet("", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json")
            .TargetFrameworkMoniker("net9.0")
            .ToToolchain();

        /// <summary>
        /// compiled as net10.0, targets latest NativeAOT build from the .NET 10 feed: https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet10/nuget/v3/index.json
        /// </summary>
        public static readonly IToolchain Net10_0 = CreateBuilder()
            .UseNuGet("", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet10/nuget/v3/index.json")
            .TargetFrameworkMoniker("net10.0")
            .ToToolchain();

        internal NativeAotToolchain(string displayName,
            string ilCompilerVersion,
            string runtimeFrameworkVersion, string targetFrameworkMoniker, string runtimeIdentifier,
            string customDotNetCliPath, string packagesRestorePath,
            Dictionary<string, string> feeds, bool useNuGetClearTag, bool useTempFolderForRestore,
            bool rootAllApplicationAssemblies, bool ilcGenerateCompleteTypeMetadata, bool ilcGenerateStackTraceData,
            string ilcOptimizationPreference, string ilcInstructionSet)
            : base(displayName,
                new Generator(ilCompilerVersion, runtimeFrameworkVersion, targetFrameworkMoniker, customDotNetCliPath,
                    runtimeIdentifier, feeds, useNuGetClearTag, useTempFolderForRestore, packagesRestorePath,
                    rootAllApplicationAssemblies, ilcGenerateCompleteTypeMetadata, ilcGenerateStackTraceData,
                    ilcOptimizationPreference, ilcInstructionSet),
                new DotNetCliPublisher(customDotNetCliPath, GetExtraArguments(runtimeIdentifier)),
                new Executor())
        {
            CustomDotNetCliPath = customDotNetCliPath;
        }

        internal string CustomDotNetCliPath { get; }

        public static NativeAotToolchainBuilder CreateBuilder() => NativeAotToolchainBuilder.Create();

        public static string GetExtraArguments(string runtimeIdentifier) => $"-r {runtimeIdentifier}";

        public override IEnumerable<ValidationError> Validate(BenchmarkCase benchmarkCase, IResolver resolver)
        {
            foreach (var error in base.Validate(benchmarkCase, resolver))
            {
                yield return error;
            }

            foreach (var validationError in DotNetSdkValidator.ValidateCoreSdks(CustomDotNetCliPath, benchmarkCase))
            {
                yield return validationError;
            }
        }
    }
}
