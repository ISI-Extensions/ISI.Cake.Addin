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

namespace ISI.Cake.Addin.BuildArtifacts
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static GetOrCreateBuildArtifactRedirectsResponse GetOrCreateBuildArtifactRedirects(this global::Cake.Core.ICakeContext cakeContext, GetOrCreateBuildArtifactRedirectsRequest request)
		{
			var response = new GetOrCreateBuildArtifactRedirectsResponse();
			
			request.WarmUpWebService(cakeContext.Log);

			var buildArtifactsApi = new ISI.Services.SCM.BuildArtifacts.BuildArtifactsApi(new ISI.Services.SCM.BuildArtifacts.Configuration(), new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

			var buildArtifactRedirects = buildArtifactsApi.FindBuildArtifactRedirects(new ()
			{
				BuildArtifactsApiUri = request.BuildArtifactsApiUri,
				BuildArtifactsApiKey = request.BuildArtifactsApiKey,
				BuildArtifactNames = new[] { request.BuildArtifactName },
			}).BuildArtifactRedirects.ToNullCheckedArray(NullCheckCollectionResult.Empty);

			var buildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp = buildArtifactRedirects.Select(buildArtifactRedirect => buildArtifactRedirect as ISI.Services.SCM.BuildArtifacts.BuildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp).FirstOrDefault(buildArtifactRedirect => string.Equals(buildArtifactRedirect?.Environment ?? string.Empty, request.Environment, StringComparison.InvariantCultureIgnoreCase));
			var buildArtifactRedirectEnvironmentBuildArtifact = buildArtifactRedirects.Select(buildArtifactRedirect => buildArtifactRedirect as ISI.Services.SCM.BuildArtifacts.BuildArtifactRedirectEnvironmentBuildArtifact).FirstOrDefault(buildArtifactRedirect => string.Equals(buildArtifactRedirect?.Environment ?? string.Empty, request.Environment, StringComparison.InvariantCultureIgnoreCase));

			var modifiedBuildArtifactRedirects = new List<ISI.Services.SCM.BuildArtifacts.IBuildArtifactRedirect>();

			if (buildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp == null)
			{
				buildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp = new ISI.Services.SCM.BuildArtifacts.BuildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp()
				{
					BuildArtifactRedirectUuid = Guid.NewGuid(),
					Environment = request.Environment,
					BuildArtifactName = request.BuildArtifactName,
					IsActive = true,
					CreateDateTimeUtc = DateTime.UtcNow,
				};

				modifiedBuildArtifactRedirects.Add(buildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp);
			}

			if (buildArtifactRedirectEnvironmentBuildArtifact == null)
			{
				buildArtifactRedirectEnvironmentBuildArtifact = new ISI.Services.SCM.BuildArtifacts.BuildArtifactRedirectEnvironmentBuildArtifact()
				{
					BuildArtifactRedirectUuid = Guid.NewGuid(),
					Environment = request.Environment,
					BuildArtifactName = request.BuildArtifactName,
					IsActive = true,
					CreateDateTimeUtc = DateTime.UtcNow,
				};

				modifiedBuildArtifactRedirects.Add(buildArtifactRedirectEnvironmentBuildArtifact);
			}

			if (modifiedBuildArtifactRedirects.Any())
			{
				buildArtifactsApi.SetBuildArtifactRedirects(new ()
				{
					BuildArtifactsApiUri = request.BuildArtifactsApiUri,
					BuildArtifactsApiKey = request.BuildArtifactsApiKey,
					BuildArtifactRedirects = modifiedBuildArtifactRedirects,
				});
			}

			response.ArtifactFileStoreUuid = buildArtifactRedirectEnvironmentBuildArtifact.BuildArtifactRedirectUuid;
			response.ArtifactVersionFileStoreUuid = buildArtifactRedirectEnvironmentBuildArtifactDateTimeStamp.BuildArtifactRedirectUuid;

			return response;
		}
	}
}