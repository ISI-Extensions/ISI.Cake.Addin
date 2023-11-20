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

namespace ISI.Cake.Addin.BuildArtifacts
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static GetBuildArtifactsApiKeyResponse GetBuildArtifactsApiKey(this global::Cake.Core.ICakeContext cakeContext, IGetBuildArtifactsApiKeyRequest request)
		{
			var response = new GetBuildArtifactsApiKeyResponse();

			var getBuildArtifactsApiKeyRequest = request as GetBuildArtifactsApiKeyRequest;

			if (request is GetBuildArtifactsApiKeyUsingSettingsRequest getBuildArtifactsApiKeyUsingSettingsRequest)
			{
				if (!string.IsNullOrWhiteSpace(getBuildArtifactsApiKeyUsingSettingsRequest.Settings.BuildArtifacts.ApiKey))
				{
					response.BuildArtifactsApiKey = getBuildArtifactsApiKeyUsingSettingsRequest.Settings.BuildArtifacts.ApiKey;
				}

				getBuildArtifactsApiKeyRequest = new()
				{
					BuildArtifactsApiUri = getBuildArtifactsApiKeyUsingSettingsRequest.Settings.BuildArtifacts.ApiUrl.GetNullableUri(),
					UserName = getBuildArtifactsApiKeyUsingSettingsRequest.Settings.BuildArtifacts.UserName,
					Password = getBuildArtifactsApiKeyUsingSettingsRequest.Settings.BuildArtifacts.Password,
				};
			}

			if (request is GetBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest getBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest)
			{
				if (!string.IsNullOrWhiteSpace(getBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest.Settings.BuildArtifacts.ApiKey))
				{
					response.BuildArtifactsApiKey = getBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest.Settings.BuildArtifacts.ApiKey;
				}

				getBuildArtifactsApiKeyRequest = new()
				{
					BuildArtifactsApiUri = getBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest.Settings.BuildArtifacts.ApiUrl.GetNullableUri(),
					UserName = getBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest.Settings.ActiveDirectory.GetDomainUserName(),
					Password = getBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest.Settings.ActiveDirectory.Password,
				};
			}

			if (string.IsNullOrWhiteSpace(response.BuildArtifactsApiKey))
			{
				var buildArtifactsApi = new ISI.Services.SCM.BuildArtifacts.BuildArtifactsApi( new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

				var getAuthenticationTokenResponse = buildArtifactsApi.GetAuthenticationToken(new ()
				{
					BuildArtifactsApiUri = getBuildArtifactsApiKeyRequest.BuildArtifactsApiUri,

					UserName = getBuildArtifactsApiKeyRequest.UserName,
					Password = getBuildArtifactsApiKeyRequest.Password,
				});

				response.BuildArtifactsApiKey = getAuthenticationTokenResponse.AuthenticationToken;
			}

			if (string.IsNullOrWhiteSpace(response.BuildArtifactsApiKey))
			{
				throw new Exception("Could not get BuildArtifactsApiKey");
			}

			return response;
		}
	}
}