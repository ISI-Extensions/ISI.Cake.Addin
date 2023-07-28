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
		public static GetBuildArtifactsAuthenticationTokenResponse GetBuildArtifactsAuthenticationToken(this global::Cake.Core.ICakeContext cakeContext, IGetBuildArtifactsAuthenticationTokenRequest request)
		{
			var response = new GetBuildArtifactsAuthenticationTokenResponse();

			var getBuildArtifactsAuthenticationTokenRequest = request as GetBuildArtifactsAuthenticationTokenRequest;

			if (request is GetBuildArtifactsAuthenticationTokenUsingSettingsRequest getBuildArtifactsAuthenticationTokenUsingSettingsRequest)
			{
				if (!string.IsNullOrWhiteSpace(getBuildArtifactsAuthenticationTokenUsingSettingsRequest.Settings.BuildArtifacts.ApiKey))
				{
					response.AuthenticationToken = getBuildArtifactsAuthenticationTokenUsingSettingsRequest.Settings.BuildArtifacts.ApiKey;
				}

				getBuildArtifactsAuthenticationTokenRequest = new()
				{
					BuildArtifactsApiUri = getBuildArtifactsAuthenticationTokenUsingSettingsRequest.Settings.BuildArtifacts.ApiUrl.GetNullableUri(),
					UserName = getBuildArtifactsAuthenticationTokenUsingSettingsRequest.Settings.BuildArtifacts.UserName,
					Password = getBuildArtifactsAuthenticationTokenUsingSettingsRequest.Settings.BuildArtifacts.Password,
				};
			}

			if (string.IsNullOrWhiteSpace(response.AuthenticationToken))
			{
				var buildArtifactsApi = new ISI.Services.SCM.BuildArtifacts.Rest.BuildArtifactsApi(null, new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

				var getAuthenticationTokenResponse = buildArtifactsApi.GetAuthenticationToken(new ISI.Services.SCM.BuildArtifacts.Rest.DataTransferObjects.BuildArtifactsApi.GetAuthenticationTokenRequest()
				{
					BuildArtifactsApiUri = getBuildArtifactsAuthenticationTokenRequest.BuildArtifactsApiUri,

					UserName = getBuildArtifactsAuthenticationTokenRequest.UserName,
					Password = getBuildArtifactsAuthenticationTokenRequest.Password,
				});

				response.AuthenticationToken = getAuthenticationTokenResponse.AuthenticationToken;
			}

			if (string.IsNullOrWhiteSpace(response.AuthenticationToken))
			{
				throw new Exception("Could not get BuildArtifactsAuthenticationToken");
			}

			return response;
		}
	}
}