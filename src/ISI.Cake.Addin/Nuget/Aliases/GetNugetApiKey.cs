#region Copyright & License
/*
Copyright (c) 2024, Integrated Solutions, Inc.
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ISI.Cake.Addin.Nuget
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static GetNugetApiKeyResponse GetNugetApiKey(this global::Cake.Core.ICakeContext cakeContext, IGetNugetApiKeyRequest request)
		{
			ServiceProvider.Initialize();

			var response = new GetNugetApiKeyResponse();

			var getNugetApiKeyRequest = request as GetNugetApiKeyRequest;

			if (request is GetNugetApiKeyUsingSettingsRequest getNugetApiKeyUsingSettingsRequest)
			{
				if (!string.IsNullOrWhiteSpace(getNugetApiKeyUsingSettingsRequest.Settings.BuildArtifacts.ApiKey))
				{
					response.NugetApiKey = getNugetApiKeyUsingSettingsRequest.Settings.BuildArtifacts.ApiKey;
				}

				getNugetApiKeyRequest = new()
				{
					RepositoryName = getNugetApiKeyUsingSettingsRequest.Settings.Nuget.RepositoryName,
					NugetServiceApiUri = getNugetApiKeyUsingSettingsRequest.Settings.Nuget.ApiUrl.GetNullableUri(),
					UserName = getNugetApiKeyUsingSettingsRequest.Settings.Nuget.UserName,
					Password = getNugetApiKeyUsingSettingsRequest.Settings.Nuget.Password,
				};
			}

			if (request is GetNugetApiKeyUsingSettingsActiveDirectoryRequest getNugetApiKeyUsingSettingsActiveDirectoryRequest)
			{
				if (!string.IsNullOrWhiteSpace(getNugetApiKeyUsingSettingsActiveDirectoryRequest.Settings.BuildArtifacts.ApiKey))
				{
					response.NugetApiKey = getNugetApiKeyUsingSettingsActiveDirectoryRequest.Settings.BuildArtifacts.ApiKey;
				}

				getNugetApiKeyRequest = new()
				{
					RepositoryName = getNugetApiKeyUsingSettingsActiveDirectoryRequest.Settings.Nuget.RepositoryName,
					NugetServiceApiUri = getNugetApiKeyUsingSettingsActiveDirectoryRequest.Settings.Nuget.ApiUrl.GetNullableUri(),
					UserName = getNugetApiKeyUsingSettingsActiveDirectoryRequest.Settings.ActiveDirectory.GetDomainUserName(),
					Password = getNugetApiKeyUsingSettingsActiveDirectoryRequest.Settings.ActiveDirectory.Password,
				};
			}

			if (string.IsNullOrWhiteSpace(response.NugetApiKey))
			{
				var nugetServiceApi = new ISI.Services.SCM.Nuget.NugetServiceApi(new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

				if ((getNugetApiKeyRequest.NugetServiceApiUri == null) && !string.IsNullOrWhiteSpace(getNugetApiKeyRequest.RepositoryName))
				{
					var jsonSerializer = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.JsonSerialization.IJsonSerializer>();

					var nugetApi = new ISI.Extensions.Nuget.NugetApi(ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.Nuget.Configuration>(), new CakeContextLogger(cakeContext), jsonSerializer);

					var nugetServersLookUp = nugetApi.GetNugetServers(new()
					{
						WorkingDirectory = cakeContext.Environment?.WorkingDirectory?.FullPath,
					}).NugetServers.ToNullCheckedDictionary(nugetServer => nugetServer.Name, _ => _, NullCheckDictionaryResult.Empty);

					if (nugetServersLookUp.TryGetValue(getNugetApiKeyRequest.RepositoryName, out var nugetServer) && nugetServer.Enabled)
					{
						getNugetApiKeyRequest.NugetServiceApiUri = nugetServer.Url.GetNullableUri();
					}
				}

				if ((getNugetApiKeyRequest.NugetServiceApiUri == null))
				{
					throw new Exception("Could not get NugetApiKey, cannot determine NugetServiceApiUri");
				}

				var getAuthenticationTokenResponse = nugetServiceApi.GetAuthenticationToken(new ()
				{
					NugetServiceApiUri = getNugetApiKeyRequest.NugetServiceApiUri,

					UserName = getNugetApiKeyRequest.UserName,
					Password = getNugetApiKeyRequest.Password,
				});

				response.NugetApiKey = getAuthenticationTokenResponse.AuthenticationToken;
			}

			if (string.IsNullOrWhiteSpace(response.NugetApiKey))
			{
				throw new Exception("Could not get NugetApiKey");
			}

			return response;
		}
	}
}