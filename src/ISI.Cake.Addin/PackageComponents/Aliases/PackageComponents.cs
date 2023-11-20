#region Copyright & License
/*
Copyright (c) 2023, Integrated Solutions, Inc.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

		* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
		* Neither the name of the Integrated Solutions, Inc. nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
 
using Cake.Common.IO;
using ISI.Cake.Addin.Extensions;
using ISI.Extensions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

			var jsonSerializer = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.JsonSerialization.IJsonSerializer>();

			var nugetApi = new ISI.Extensions.Nuget.NugetApi(new ISI.Extensions.Nuget.Configuration(), logger, jsonSerializer);
			var packagerApi = new ISI.Extensions.VisualStudio.PackagerApi(logger, nugetApi, new ISI.Extensions.VisualStudio.MSBuildApi(logger, new ISI.Extensions.VisualStudio.VsWhereApi(logger, nugetApi)), new ISI.Extensions.VisualStudio.CodeGenerationApi(logger), new ISI.Extensions.VisualStudio.XmlTransformApi(logger));

			packagerApi.PackageComponents(new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentsRequest()
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
								AfterBuildPackageComponent = packageComponentDirectory => packageComponentConsoleApplication.AfterBuildPackageComponent?.Invoke(packageComponentDirectory),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;
						case PackageComponentWebSite packageComponentWebSite:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentWebSite()
							{
								ProjectFullName = packageComponentWebSite.ProjectFullName,
								IconFileName = packageComponentWebSite.IconFileName,
								DoNotXmlTransformConfigs = packageComponentWebSite.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentWebSite.ExcludeFiles,
								AfterBuildPackageComponent = packageComponentDirectory => packageComponentWebSite.AfterBuildPackageComponent?.Invoke(packageComponentDirectory),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;
						case PackageComponentWindowsApplication packageComponentWindowsApplication:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentWindowsApplication()
							{
								ProjectFullName = packageComponentWindowsApplication.ProjectFullName,
								IconFileName = packageComponentWindowsApplication.IconFileName,
								DoNotXmlTransformConfigs = packageComponentWindowsApplication.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentWindowsApplication.ExcludeFiles,
								AfterBuildPackageComponent = packageComponentDirectory => packageComponentWindowsApplication.AfterBuildPackageComponent?.Invoke(packageComponentDirectory),
							} as ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.IPackageComponent;
						case PackageComponentWindowsService packageComponentWindowsService:
							return new ISI.Extensions.VisualStudio.DataTransferObjects.PackagerApi.PackageComponentWindowsService()
							{
								ProjectFullName = packageComponentWindowsService.ProjectFullName,
								IconFileName = packageComponentWindowsService.IconFileName,
								DoNotXmlTransformConfigs = packageComponentWindowsService.DoNotXmlTransformConfigs,
								ExcludeFiles = packageComponentWindowsService.ExcludeFiles,
								AfterBuildPackageComponent = packageComponentDirectory => packageComponentWindowsService.AfterBuildPackageComponent?.Invoke(packageComponentDirectory),
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
				AddToLog = description => logger.LogInformation(description),
			});

			return response;
		}
	}
}