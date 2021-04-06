#region Copyright & License
/*
Copyright (c) 2021, Integrated Solutions, Inc.
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

namespace ISI.Cake.Addin.DeploymentManager
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static DeployArtifactResponse DeployArtifact(this global::Cake.Core.ICakeContext cakeContext, DeployArtifactRequest request)
		{
			var response = new DeployArtifactResponse();
			
			request.WarmUpWebService(cakeContext.Log);

			var deploymentManagerApi = new ISI.Extensions.Scm.DeploymentManagerApi(new CakeContextLogger(cakeContext));

			response.Success = deploymentManagerApi.DeployArtifact(new ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.DeployArtifactRequest()
			{
				ServicesManagerUrl = request.ServicesManagerUrl,
				Password = request.Password,
				BuildArtifactManagementUrl = request.BuildArtifactManagementUrl,
				AuthenticationToken = request.AuthenticationToken,
				ArtifactName = request.ArtifactName,
				ArtifactDateTimeStampVersionUrl = request.ArtifactDateTimeStampVersionUrl,
				ArtifactDownloadUrl = request.ArtifactDownloadUrl,
				ToDateTimeStamp = request.ToDateTimeStamp,
				FromEnvironment = request.FromEnvironment,
				ToEnvironment = request.ToEnvironment,
				ConfigurationKey = request.ConfigurationKey,
				Components = request.Components.ToNullCheckedArray(component =>
				{
					switch (component)
					{
						case DeployComponent deployComponent:
							return new ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.DeployComponent()
							{
								ComponentType = deployComponent.ComponentType,
								PackageFolder = deployComponent.PackageFolder,
								DeployToSubfolder = deployComponent.DeployToSubfolder,
								ApplicationExe = deployComponent.ApplicationExe,
								ExcludeFiles = deployComponent.ExcludeFiles,
							} as ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.IDeployComponent;

						case DeployComponentConsoleApplication deployComponentConsoleApplication:
							return new ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.DeployComponentConsoleApplication()
							{
								PackageFolder = deployComponentConsoleApplication.PackageFolder,
								DeployToSubfolder = deployComponentConsoleApplication.DeployToSubfolder,
								ConsoleApplicationExe = deployComponentConsoleApplication.ConsoleApplicationExe,
								ExcludeFiles = deployComponentConsoleApplication.ExcludeFiles,
								ExecuteConsoleApplicationAfterInstall = deployComponentConsoleApplication.ExecuteConsoleApplicationAfterInstall,
								ExecuteConsoleApplicationAfterInstallArguments = deployComponentConsoleApplication.ExecuteConsoleApplicationAfterInstallArguments,
							} as ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.IDeployComponent;

						case DeployComponentWebSite deployComponentWebSite:
							return new ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.DeployComponentConsoleApplication()
							{
								PackageFolder = deployComponentWebSite.PackageFolder,
								DeployToSubfolder = deployComponentWebSite.DeployToSubfolder,
								ExcludeFiles = deployComponentWebSite.ExcludeFiles,
							} as ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.IDeployComponent;

						case DeployComponentWindowsService deployComponentWindowsService:
							return new ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.DeployComponentWindowsService()
							{
								PackageFolder = deployComponentWindowsService.PackageFolder,
								DeployToSubfolder = deployComponentWindowsService.DeployToSubfolder,
								WindowsServiceExe = deployComponentWindowsService.WindowsServiceExe,
								ExcludeFiles = deployComponentWindowsService.ExcludeFiles,
								UninstallIfInstalled = deployComponentWindowsService.UninstallIfInstalled,
							} as ISI.Extensions.Scm.DataTransferObjects.DeploymentManagerApi.IDeployComponent;

						default:
							throw new ArgumentOutOfRangeException(nameof(component));
					}
				}),
				SetDeployedVersion = request.SetDeployedVersion,
				RunAsync = request.RunAsync,
			}).Success;

			return response;
		}
	}
}