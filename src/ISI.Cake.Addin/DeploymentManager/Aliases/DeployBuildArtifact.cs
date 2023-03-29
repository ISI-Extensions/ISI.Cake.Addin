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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Cake.Addin.Extensions;
using ISI.Extensions.Extensions;
using WindowsDeploymentApiDTOs = ISI.Services.WindowsDeploymentAgent.DataTransferObjects.WindowsDeploymentApi;

namespace ISI.Cake.Addin.DeploymentManager
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static DeployBuildArtifactResponse DeployBuildArtifact(this global::Cake.Core.ICakeContext cakeContext, DeployBuildArtifactRequest request)
		{
			var response = new DeployBuildArtifactResponse();

			request.WarmUpWebService(cakeContext.Log);

			var buildArtifactsApi = new ISI.Services.SCM.BuildArtifacts.Rest.BuildArtifactsApi(null, new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

			var applicationIsInUse = false;
			var wouldNotStart = false;

			var versionIsAlreadyDeployed = false;
			var toVersion = string.Empty;

			if (!string.IsNullOrWhiteSpace(request.BuildArtifactDateTimeStampVersionUrl))
			{
				toVersion = new ISI.Extensions.Scm.DateTimeStampVersion(request.ToDateTimeStampVersion?.ToString())?.Version?.ToString() ?? string.Empty;

				if (string.IsNullOrWhiteSpace(toVersion))
				{
					toVersion = buildArtifactsApi.GetBuildArtifactEnvironmentDateTimeStampVersion(new ISI.Services.SCM.BuildArtifacts.Rest.DataTransferObjects.BuildArtifactsApi.GetBuildArtifactEnvironmentDateTimeStampVersionRequest()
					{
						BuildArtifactsApiUri = request.BuildArtifactsApiUri,
						BuildArtifactsApiKey = request.BuildArtifactsApiKey,
						BuildArtifactName = request.BuildArtifactName,
						Environment = request.FromEnvironment,
					})?.DateTimeStampVersion?.Version?.ToString() ?? string.Empty;
				}

				var currentVersion = buildArtifactsApi.GetBuildArtifactEnvironmentDateTimeStampVersion(new ISI.Services.SCM.BuildArtifacts.Rest.DataTransferObjects.BuildArtifactsApi.GetBuildArtifactEnvironmentDateTimeStampVersionRequest()
				{
					BuildArtifactsApiUri = request.BuildArtifactsApiUri,
					BuildArtifactsApiKey = request.BuildArtifactsApiKey,
					BuildArtifactName = request.BuildArtifactName,
					Environment = request.ToEnvironment,
				})?.DateTimeStampVersion?.Version?.ToString() ?? string.Empty;

				versionIsAlreadyDeployed = !string.IsNullOrWhiteSpace(toVersion) && !string.IsNullOrWhiteSpace(currentVersion) && string.Equals(toVersion, currentVersion, StringComparison.InvariantCultureIgnoreCase);
			}

			if (versionIsAlreadyDeployed)
			{
				cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "Version: {0} already deployed", toVersion);
			}
			else
			{
				var deploymentManagerApi = new ISI.Services.WindowsDeploymentAgent.Rest.WindowsDeploymentApi(null, new CakeContextLogger(cakeContext));

				var deployArtifactResponse = deploymentManagerApi.DeployArtifact(new()
				{
					WindowsDeploymentApiUri = request.WindowsDeploymentApiUri,
					WindowsDeploymentApiKey = request.WindowsDeploymentApiKey,
					BuildArtifactsApiUri = request.BuildArtifactsApiUri,
					BuildArtifactsApiKey = request.BuildArtifactsApiKey,
					BuildArtifactName = request.BuildArtifactName,
					BuildArtifactDateTimeStampVersionUrl = request.BuildArtifactDateTimeStampVersionUrl,
					BuildArtifactDownloadUrl = request.BuildArtifactDownloadUrl,
					ToDateTimeStampVersion = request.ToDateTimeStampVersion,
					ToEnvironment = request.ToEnvironment,
					ConfigurationKey = request.ConfigurationKey,
					Components = request.Components.ToNullCheckedArray(component =>
					{
						switch (component)
						{
							case DeployComponentConsoleApplication deployComponentConsoleApplication:
								return new WindowsDeploymentApiDTOs.DeployComponentConsoleApplication()
								{
									PauseComponentUrl = deployComponentConsoleApplication.PauseComponentUrl,
									CheckComponentCanDeployStatusUrl = deployComponentConsoleApplication.CheckComponentCanDeployStatusUrl,
									CheckComponentCanDeployStatusInterval = deployComponentConsoleApplication.CheckComponentCanDeployStatusInterval,
									CheckComponentCanDeployStatusTimeout = deployComponentConsoleApplication.CheckComponentCanDeployStatusTimeout,
									CheckComponentCanDeployStatusHttpStatus = deployComponentConsoleApplication.CheckComponentCanDeployStatusHttpStatus,
									CheckComponentCanDeployStatusJsonPath = deployComponentConsoleApplication.CheckComponentCanDeployStatusJsonPath,
									CheckComponentCanDeployStatusJsonPathValue = deployComponentConsoleApplication.CheckComponentCanDeployStatusJsonPathValue,
									CheckComponentCanDeployStatusCommentJsonPath = deployComponentConsoleApplication.CheckComponentCanDeployStatusCommentJsonPath,
									WaitForFileLocksMaxTimeOut = deployComponentConsoleApplication.WaitForFileLocksMaxTimeOut,
									PackageFolder = deployComponentConsoleApplication.PackageFolder,
									DeployToSubfolder = deployComponentConsoleApplication.DeployToSubfolder,
									DeployToSubfolderIconFileName = deployComponentConsoleApplication.DeployToSubfolderIconFileName,
									ConsoleApplicationExe = deployComponentConsoleApplication.ConsoleApplicationExe,
									ExcludeFileNames = deployComponentConsoleApplication.ExcludeFileNames.ToNullCheckedArray(),
									ExecuteConsoleApplicationAfterInstall = deployComponentConsoleApplication.ExecuteConsoleApplicationAfterInstall,
									ExecuteConsoleApplicationAfterInstallArguments = deployComponentConsoleApplication.ExecuteConsoleApplicationAfterInstallArguments,
								} as WindowsDeploymentApiDTOs.IDeployComponent;

							case DeployComponentWebSite deployComponentWebSite:
								return new WindowsDeploymentApiDTOs.DeployComponentWebSite()
								{
									PauseComponentUrl = deployComponentWebSite.PauseComponentUrl,
									CheckComponentCanDeployStatusUrl = deployComponentWebSite.CheckComponentCanDeployStatusUrl,
									CheckComponentCanDeployStatusInterval = deployComponentWebSite.CheckComponentCanDeployStatusInterval,
									CheckComponentCanDeployStatusTimeout = deployComponentWebSite.CheckComponentCanDeployStatusTimeout,
									CheckComponentCanDeployStatusHttpStatus = deployComponentWebSite.CheckComponentCanDeployStatusHttpStatus,
									CheckComponentCanDeployStatusJsonPath = deployComponentWebSite.CheckComponentCanDeployStatusJsonPath,
									CheckComponentCanDeployStatusJsonPathValue = deployComponentWebSite.CheckComponentCanDeployStatusJsonPathValue,
									CheckComponentCanDeployStatusCommentJsonPath = deployComponentWebSite.CheckComponentCanDeployStatusCommentJsonPath,
									WaitForFileLocksMaxTimeOut = deployComponentWebSite.WaitForFileLocksMaxTimeOut,
									PackageFolder = deployComponentWebSite.PackageFolder,
									DeployToSubfolder = deployComponentWebSite.DeployToSubfolder,
									DeployToSubfolderIconFileName = deployComponentWebSite.DeployToSubfolderIconFileName,
									ExcludeFileNames = deployComponentWebSite.ExcludeFileNames.ToNullCheckedArray(),
								} as WindowsDeploymentApiDTOs.IDeployComponent;

							case DeployComponentWindowsService deployComponentWindowsService:
								return new WindowsDeploymentApiDTOs.DeployComponentWindowsService()
								{
									PauseComponentUrl = deployComponentWindowsService.PauseComponentUrl,
									CheckComponentCanDeployStatusUrl = deployComponentWindowsService.CheckComponentCanDeployStatusUrl,
									CheckComponentCanDeployStatusInterval = deployComponentWindowsService.CheckComponentCanDeployStatusInterval,
									CheckComponentCanDeployStatusTimeout = deployComponentWindowsService.CheckComponentCanDeployStatusTimeout,
									CheckComponentCanDeployStatusHttpStatus = deployComponentWindowsService.CheckComponentCanDeployStatusHttpStatus,
									CheckComponentCanDeployStatusJsonPath = deployComponentWindowsService.CheckComponentCanDeployStatusJsonPath,
									CheckComponentCanDeployStatusJsonPathValue = deployComponentWindowsService.CheckComponentCanDeployStatusJsonPathValue,
									CheckComponentCanDeployStatusCommentJsonPath = deployComponentWindowsService.CheckComponentCanDeployStatusCommentJsonPath,
									WaitForFileLocksMaxTimeOut = deployComponentWindowsService.WaitForFileLocksMaxTimeOut,
									PackageFolder = deployComponentWindowsService.PackageFolder,
									DeployToSubfolder = deployComponentWindowsService.DeployToSubfolder,
									DeployToSubfolderIconFileName = deployComponentWindowsService.DeployToSubfolderIconFileName,
									WindowsServiceExe = deployComponentWindowsService.WindowsServiceExe,
									ExcludeFileNames = deployComponentWindowsService.ExcludeFileNames.ToNullCheckedArray(),
									UninstallIfInstalled = deployComponentWindowsService.UninstallIfInstalled,
									ReverseProxySiteConfiguration = deployComponentWindowsService.ReverseProxySiteConfiguration.NullCheckedConvert(reverseProxySiteConfiguration =>
									{
										switch (reverseProxySiteConfiguration)
										{
											case ReverseProxyNginxSiteConfiguration reverseProxyNginxSiteConfiguration:
												return new WindowsDeploymentApiDTOs.ReverseProxyNginxSiteConfiguration()
												{
													ConfigurationName = reverseProxyNginxSiteConfiguration.ConfigurationName,
													Servers = reverseProxyNginxSiteConfiguration.Servers.ToNullCheckedArray(server => new WindowsDeploymentApiDTOs.ReverseProxyNginxSiteConfigurationServer()
													{
														Port = server.Port,
														SslConfigurationName = server.SslConfigurationName,
														ServerName = server.ServerName,
														Locations = server.Locations.ToNullCheckedArray(location => new WindowsDeploymentApiDTOs.ReverseProxyNginxSiteConfigurationServerLocation()
														{
															Path = location.Path,
															ServerLocationTemplateName = location.ServerLocationTemplateName,
															ServerLocationTemplateKeyValues = location.ServerLocationTemplateKeyValues,
															Directives = location.Directives.ToNullCheckedArray(),
														}),
													})
												};

											default:
												throw new ArgumentOutOfRangeException(nameof(reverseProxySiteConfiguration));
										}
									})
,
								} as WindowsDeploymentApiDTOs.IDeployComponent;

							default:
								throw new ArgumentOutOfRangeException(nameof(component));
						}
					}),
					SetDeployedVersion = request.SetDeployedVersion,
					RunAsync = request.RunAsync,
				});

				applicationIsInUse = deployArtifactResponse.DeployComponentResponses.NullCheckedAny(deployComponentResponse => deployComponentResponse.InUse);
				wouldNotStart = deployArtifactResponse.DeployComponentResponses.NullCheckedAny(deployComponentResponse => (deployComponentResponse is WindowsDeploymentApiDTOs.DeployComponentWindowsServiceResponse deployComponentWindowsServiceResponse) && deployComponentWindowsServiceResponse.WouldNotStart);
				versionIsAlreadyDeployed = deployArtifactResponse.DeployComponentResponses.NullCheckedAny(deployComponentResponse => deployComponentResponse.SameVersion);

				response.Success = deployArtifactResponse.DeployComponentResponses.NullCheckedAll(deployComponentResponse => deployComponentResponse.Success);
			}

			if (applicationIsInUse)
			{
				if (request.ThrowExceptionWhenComponentInUse)
				{
					throw new Exception("Deployment Failed, Application is in use");
				}
			}
			else if (versionIsAlreadyDeployed)
			{
				if (request.ThrowExceptionWhenVersionIsAlreadyDeployed)
				{
					throw new Exception("Deployment Failed, Version is already Deployed");
				}
			}
			else if (wouldNotStart)
			{
				if (request.ThrowExceptionWhenWouldNotStart)
				{
					throw new Exception("Deployment Failed, Would Not Start");
				}
			}
			else if (!response.Success)
			{
				if (request.ThrowExceptionWhenNotSuccessful)
				{
					throw new Exception("Deployment Failed");
				}
			}

			return response;
		}
	}
}