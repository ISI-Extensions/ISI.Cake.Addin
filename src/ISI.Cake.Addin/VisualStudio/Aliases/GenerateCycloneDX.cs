#region Copyright & License
/*
Copyright (c) 2026, Integrated Solutions, Inc.
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ISI.Cake.Addin.VisualStudio
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static GenerateCycloneDXResponse GenerateCycloneDX(this global::Cake.Core.ICakeContext cakeContext, GenerateCycloneDXRequest request)
		{
			ServiceProvider.Initialize();

			var response = new GenerateCycloneDXResponse();

			var logger = new CakeContextLogger(cakeContext);
			var dateTimeStamper = new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper();

			var sBomApi = new ISI.Extensions.Sbom.SbomApi(ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.Sbom.Configuration>(), logger, dateTimeStamper);

			sBomApi.GenerateCycloneDX(new()
			{
				FullName = request.FullName,
				Framework = request.Framework,
				Runtime = request.Runtime,
				OutputDirectory = request.OutputDirectory,
				OutputFilename = request.OutputFilename,
				OutputJson = request.OutputJson,
				ExcludeDependencies = request.ExcludeDependencies.ToNullCheckedArray(),
				ExcludeDevelopmentDependencies = request.ExcludeDevelopmentDependencies,
				ExcludeTestProjects = request.ExcludeTestProjects,
				AlternativeNugetUrl = request.AlternativeNugetUrl,
				AlternativeNugetUserName = request.AlternativeNugetUserName,
				AlternativeNugetPasswordApiKey = request.AlternativeNugetPasswordApiKey,
				AlternativeNugetPasswordIsClearText = request.AlternativeNugetPasswordIsClearText,
				Recursive = request.Recursive,
				OmitSerialNumber = request.OmitSerialNumber,
				GitHubUserName = request.GitHubUserName,
				GitHubToken = request.GitHubToken,
				GitHubBearerToken = request.GitHubBearerToken,
				GitHubEnableLicenses = request.GitHubEnableLicenses,
				DisablePackageRestore = request.DisablePackageRestore,
				DisableHashComputation = request.DisableHashComputation,
				DotnetCommandTimeout = request.DotnetCommandTimeout,
				BaseIntermediateOutputPath = request.BaseIntermediateOutputPath,
				ImportMetadataPath = request.ImportMetadataPath,
				IncludeProjectPeferences = request.IncludeProjectPeferences,
				SetName = request.SetName,
				SetVersion = request.SetVersion,
				SetComponentType = request.SetComponentType,
				SetNugetPurl = request.SetNugetPurl,
			});

			return response;
		}
	}
}