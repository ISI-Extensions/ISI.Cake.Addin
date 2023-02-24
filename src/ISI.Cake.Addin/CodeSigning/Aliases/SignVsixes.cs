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
 
using ISI.Cake.Addin.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.CodeSigning
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static SignVsixesResponse SignVsixes(this global::Cake.Core.ICakeContext cakeContext, ISignVsixesRequest request)
		{
			var response = new SignVsixesResponse();

			var signVsixesRequest = request as SignVsixesRequest;

			if (request is SignVsixesUsingSettingsRequest signVsixesUsingSettingsRequest)
			{
				signVsixesRequest = new SignVsixesRequest()
				{
					VsixPaths = signVsixesUsingSettingsRequest.VsixPaths,
					OutputDirectory = signVsixesUsingSettingsRequest.OutputDirectory,
					RemoteCodeSigningServiceUri = cakeContext.GetNullableUri(signVsixesUsingSettingsRequest.Settings.CodeSigning.RemoteCodeSigningServiceUrl),
					RemoteCodeSigningServiceApiKey = signVsixesUsingSettingsRequest.Settings.CodeSigning.RemoteCodeSigningServicePassword,
					CodeSigningCertificateTokenCertificateFileName = signVsixesUsingSettingsRequest.Settings.CodeSigning.Token.CertificateFileName,
					CodeSigningCertificateTokenCryptographicProvider = signVsixesUsingSettingsRequest.Settings.CodeSigning.Token.CryptographicProvider,
					CodeSigningCertificateTokenContainerName = signVsixesUsingSettingsRequest.Settings.CodeSigning.Token.ContainerName,
					CodeSigningCertificateTokenPassword = signVsixesUsingSettingsRequest.Settings.CodeSigning.Token.Password,
					TimeStampUri = cakeContext.GetNullableUri(signVsixesUsingSettingsRequest.Settings.CodeSigning.TimeStampUrl),
					TimeStampDigestAlgorithm = cakeContext.GetCodeSigningDigestAlgorithm(signVsixesUsingSettingsRequest.Settings.CodeSigning.TimeStampDigestAlgorithm),
					CertificatePath = cakeContext.GetNullableFile(signVsixesUsingSettingsRequest.Settings.CodeSigning.CertificateFileName),
					CertificatePassword = signVsixesUsingSettingsRequest.Settings.CodeSigning.CertificatePassword,
					CertificateFingerprint = signVsixesUsingSettingsRequest.Settings.CodeSigning.CertificateFingerprint,
					DigestAlgorithm = cakeContext.GetCodeSigningDigestAlgorithm(signVsixesUsingSettingsRequest.Settings.CodeSigning.DigestAlgorithm),
					RunAsync = signVsixesUsingSettingsRequest.Settings.CodeSigning.RunAsync,
				};
			}

			if (signVsixesRequest.RemoteCodeSigningServiceUri == null)
			{
				var logger = new CakeContextLogger(cakeContext);
				var codeSigningApi = new ISI.Extensions.VisualStudio.CodeSigningApi(logger, new ISI.Extensions.VisualStudio.VsixSigntoolApi(logger, new ISI.Extensions.Nuget.NugetApi(logger)));

				codeSigningApi.SignVsixes(new ISI.Extensions.VisualStudio.DataTransferObjects.CodeSigningApi.SignVsixesRequest()
				{
					VsixFullNames = request.VsixPaths.ToNullCheckedArray(vsixPath => vsixPath.FullPath),
					OutputDirectory = request.OutputDirectory?.FullPath,
					CodeSigningCertificateTokenCertificateFileName = signVsixesRequest.CodeSigningCertificateTokenCertificateFileName,
					CodeSigningCertificateTokenCryptographicProvider = signVsixesRequest.CodeSigningCertificateTokenCryptographicProvider,
					CodeSigningCertificateTokenContainerName = signVsixesRequest.CodeSigningCertificateTokenContainerName,
					CodeSigningCertificateTokenPassword = signVsixesRequest.CodeSigningCertificateTokenPassword,
					TimeStampUri = signVsixesRequest.TimeStampUri,
					TimeStampDigestAlgorithm = signVsixesRequest.TimeStampDigestAlgorithm.ToCodeSigningDigestAlgorithm(),
					CertificateFileName = signVsixesRequest.CertificatePath?.FullPath,
					CertificatePassword = signVsixesRequest.CertificatePassword,
					CertificateStoreName = signVsixesRequest.CertificateStoreName,
					CertificateStoreLocation = signVsixesRequest.CertificateStoreLocation,
					CertificateSubjectName = signVsixesRequest.CertificateSubjectName,
					CertificateFingerprint = signVsixesRequest.CertificateFingerprint,
					DigestAlgorithm = signVsixesRequest.DigestAlgorithm.ToCodeSigningDigestAlgorithm(),
					OverwriteAnyExistingSignature = signVsixesRequest.OverwriteAnyExistingSignature,
					RunAsync = signVsixesRequest.RunAsync,
					Verbosity = signVsixesRequest.Verbosity.ToCodeSigningVerbosity(),
				});
			}
			else
			{
				var remoteCodeSigningApi = new ISI.Services.SCM.RemoteCodeSigning.Rest.RemoteCodeSigningApi(null, new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

				remoteCodeSigningApi.SignVsixes(new ISI.Services.SCM.RemoteCodeSigning.Rest.DataTransferObjects.RemoteCodeSigningApi.SignVsixesRequest()
				{
					RemoteCodeSigningServiceUri = signVsixesRequest.RemoteCodeSigningServiceUri,
					RemoteCodeSigningServiceApiKey = signVsixesRequest.RemoteCodeSigningServiceApiKey,
					VsixFullNames = request.VsixPaths.ToNullCheckedArray(vsixPath => vsixPath.FullPath),
					OutputDirectory = request.OutputDirectory?.FullPath,
					OverwriteAnyExistingSignature = signVsixesRequest.OverwriteAnyExistingSignature,
					RunAsync = signVsixesRequest.RunAsync,
					Verbosity = signVsixesRequest.Verbosity.ToRemoteCodeSigningVerbosity(),
				});
			}

			return response;
		}
	}
}