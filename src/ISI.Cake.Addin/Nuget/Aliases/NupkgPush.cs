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
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.Nuget
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static NupkgPushResponse NupkgPush(this global::Cake.Core.ICakeContext cakeContext, INupkgPushRequest request)
		{
			var response = new NupkgPushResponse();

			var nupkgPushRequest = request as NupkgPushRequest;

			if (request is NupkgPushUsingSettingsRequest nupkgPushUsingSettingsRequest)
			{
				var nugetApiKey = nupkgPushUsingSettingsRequest.Settings.Nuget.ApiKey;

				if (string.IsNullOrWhiteSpace(nugetApiKey))
				{
					nugetApiKey = GetNugetApiKey(cakeContext, new GetNugetApiKeyUsingSettingsRequest()
					{
						Settings = nupkgPushUsingSettingsRequest.Settings,
					}).NugetApiKey;
				}

				nupkgPushRequest = new()
				{
					RepositoryName = nupkgPushUsingSettingsRequest.Settings.Nuget.RepositoryName,
					RepositoryUri = nupkgPushUsingSettingsRequest.Settings.Nuget.ApiUrl.GetNullableUri(),
					NugetApiKey = nugetApiKey,
				};
			}

			if (request is NupkgPushUsingSettingsActiveDirectoryRequest nupkgPushUsingSettingsActiveDirectoryRequest)
			{
				var nugetApiKey = nupkgPushUsingSettingsActiveDirectoryRequest.Settings.Nuget.ApiKey;

				if (string.IsNullOrWhiteSpace(nugetApiKey))
				{
					nugetApiKey = GetNugetApiKey(cakeContext, new GetNugetApiKeyUsingSettingsActiveDirectoryRequest()
					{
						Settings = nupkgPushUsingSettingsActiveDirectoryRequest.Settings,
					}).NugetApiKey;
				}

				nupkgPushRequest = new()
				{
					RepositoryName = nupkgPushUsingSettingsActiveDirectoryRequest.Settings.Nuget.RepositoryName,
					RepositoryUri = nupkgPushUsingSettingsActiveDirectoryRequest.Settings.Nuget.ApiUrl.GetNullableUri(),
					NugetApiKey = nugetApiKey,
				};
			}

			if (string.IsNullOrWhiteSpace(nupkgPushRequest.NugetApiKey))
			{
				throw new Exception("Could not get NugetApiKey");
			}

			var nugetApi = new ISI.Extensions.Nuget.NugetApi(new CakeContextLogger(cakeContext));

			nugetApi.NupkgPush(new ISI.Extensions.Nuget.DataTransferObjects.NugetApi.NupkgPushRequest()
			{
				NupkgFullNames = request.NupkgPaths.ToNullCheckedArray(nupkgPath => nupkgPath.FullPath),
				NugetApiKey = nupkgPushRequest.NugetApiKey,
				RepositoryName = nupkgPushRequest.RepositoryName,
				RepositoryUri = nupkgPushRequest.RepositoryUri,
				WorkingDirectory = cakeContext.Environment?.WorkingDirectory?.FullPath,
			});

			return response;
		}
	}
}