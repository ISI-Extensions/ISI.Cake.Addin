#region Copyright & License
/*
Copyright (c) 2025, Integrated Solutions, Inc.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

		* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
		* Neither the name of the Integrated Solutions, Inc. nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Cake.Addin.Extensions;
using ISI.Extensions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ISI.Cake.Addin.PackageComponents
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static PackageComponentsResponse PackageComponents(this global::Cake.Core.ICakeContext cakeContext, PackageComponentsRequest request)
		{
			ServiceProvider.Initialize();

			var response = new PackageComponentsResponse();

			if (string.IsNullOrWhiteSpace(request.PackageName))
			{
				request.PackageName = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileNameWithoutExtension(request.PackageFullName)));
			}

			var logger = new CakeContextLogger(cakeContext);
			var dateTimeStamper = new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper();

			var jsonSerializer = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.JsonSerialization.IJsonSerializer>();

			var nugetApi = new ISI.Extensions.Nuget.NugetApi(ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.Nuget.Configuration>(), logger, jsonSerializer);
			var packagerApi = new ISI.Extensions.VisualStudio.PackagerApi(logger, nugetApi, new ISI.Extensions.VisualStudio.MSBuildApi(logger, new ISI.Extensions.VisualStudio.VsWhereApi(ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.VisualStudio.Configuration>(), logger, dateTimeStamper, nugetApi)), new ISI.Extensions.VisualStudio.CodeGenerationApi(logger), new ISI.Extensions.VisualStudio.XmlTransformApi(logger));
			var sBomApi = new ISI.Extensions.Sbom.SbomApi(ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.Sbom.Configuration>(), logger, dateTimeStamper);

			ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.AfterBuildPackageComponentDelegate getAfterBuildPackageComponentDelegate(AfterBuildPackageComponentDelegate afterBuildPackageComponent)
			{
				if (request.SbomConfiguration != null)
				{
					return context =>
					{
						afterBuildPackageComponent?.Invoke(new AfterBuildPackageComponentContext()
						{
							ProjectFullName = context.ProjectFullName,
							PackageComponentDirectory = context.PackageComponentDirectory,
						});

						var packageName = System.IO.Path.GetFileNameWithoutExtension(context.ProjectFullName);
						var packageSourceDirectory = System.IO.Path.GetDirectoryName(context.ProjectFullName);

						var generateSBomUsingSettingsRequest = request.SbomConfiguration as PackageComponentsRequestSbomConfigurationUsingSettings;
						var generateSBomRequest = request.SbomConfiguration as PackageComponentsRequestSbomConfiguration;

						sBomApi.GeneratePackageSPDX(new()
						{
							PackageComponentDirectory = context.PackageComponentDirectory,
							PackageSourceDirectory = packageSourceDirectory,
							PackageName = packageName,
							PackageVersion = request.SbomConfiguration.PackageVersion,
							PackageAuthor = generateSBomRequest?.PackageAuthor ?? generateSBomUsingSettingsRequest?.Settings.SBom.Author ?? string.Empty,
							PackageNamespace = generateSBomRequest?.PackageNamespaceUri ?? new Uri(generateSBomUsingSettingsRequest?.Settings.SBom.Namespace),
						});
					};
				}

				return context => afterBuildPackageComponent?.Invoke(new AfterBuildPackageComponentContext()
				{
					ProjectFullName = context.ProjectFullName,
					PackageComponentDirectory = context.PackageComponentDirectory,
				});
			}

			packagerApi.PackageComponents(new ()
			{
				Configuration = request.Configuration,
				BuildVersion = request.BuildVersion.ToMSBuildVersion(),
				BuildPlatform = request.BuildPlatform.ToMSBuildPlatform(),
				PlatformTarget = request.PlatformTarget.ToBuildPlatformTarget(),
				BuildVerbosity = request.BuildVerbosity.ToMSBuildVerbosity(),
				Solution = request.Solution,
				SubDirectory = request.SubDirectory,
				PackageComponents = request.PackageComponents.ToNullCheckedArray(packageComponent =>
				{
					switch (packageComponent)
					{
						case PackageComponentConsoleApplication packageComponentConsoleApplication:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentConsoleApplication()
							{
								ProjectFullName = packageComponentConsoleApplication.ProjectFullName,
								IconFileName = packageComponentConsoleApplication.IconFileName,
								DoNotXmlTransformConfigs = packageComponentConsoleApplication.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentConsoleApplication.ExcludeFiles,
								AfterBuildPackageComponent = getAfterBuildPackageComponentDelegate(packageComponentConsoleApplication.AfterBuildPackageComponent),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;

						case PackageComponentWebSite packageComponentWebSite:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentWebSite()
							{
								ProjectFullName = packageComponentWebSite.ProjectFullName,
								IconFileName = packageComponentWebSite.IconFileName,
								DoNotXmlTransformConfigs = packageComponentWebSite.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentWebSite.ExcludeFiles,
								AfterBuildPackageComponent = getAfterBuildPackageComponentDelegate(packageComponentWebSite.AfterBuildPackageComponent),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;

						case PackageComponentWindowsApplication packageComponentWindowsApplication:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentWindowsApplication()
							{
								ProjectFullName = packageComponentWindowsApplication.ProjectFullName,
								IconFileName = packageComponentWindowsApplication.IconFileName,
								DoNotXmlTransformConfigs = packageComponentWindowsApplication.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentWindowsApplication.ExcludeFiles,
								AfterBuildPackageComponent = getAfterBuildPackageComponentDelegate(packageComponentWindowsApplication.AfterBuildPackageComponent),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;

						case PackageComponentWindowsService packageComponentWindowsService:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentWindowsService()
							{
								ProjectFullName = packageComponentWindowsService.ProjectFullName,
								IconFileName = packageComponentWindowsService.IconFileName,
								DoNotXmlTransformConfigs = packageComponentWindowsService.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentWindowsService.ExcludeFiles,
								AfterBuildPackageComponent = getAfterBuildPackageComponentDelegate(packageComponentWindowsService.AfterBuildPackageComponent),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;

						default:
							throw new ArgumentOutOfRangeException(nameof(packageComponent));
					}
				}),
				PackageFullName = request.PackageFullName,
				PackageName = request.PackageName,
				PackageBuildDateTimeStampVersion = request.PackageBuildDateTimeStampVersion,
				AssemblyVersionFiles = request.AssemblyVersionFiles,
				CleanupTempDirectories = request.CleanupTempDirectories,
				AddToLog = (logEntryLevel, description) => logger.LogInformation(description),
			});

			return response;
		}
	}
}