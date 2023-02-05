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
		public static SignNupkgsResponse SignNupkgs(this global::Cake.Core.ICakeContext cakeContext, ISignNupkgsRequest request)
		{
			var response = new SignNupkgsResponse();

			var signNupkgsRequest = request as SignNupkgsRequest;

			if (request is SignNupkgsUsingSettingsRequest signNupkgsUsingSettingsRequest)
			{
				signNupkgsRequest = new SignNupkgsRequest()
				{
					NupkgPaths = signNupkgsUsingSettingsRequest.NupkgPaths,
					OutputDirectory = signNupkgsUsingSettingsRequest.OutputDirectory,
					CodeSigningCertificateTokenCertificateFileName = signNupkgsUsingSettingsRequest.Settings.CodeSigning.Token.CertificateFileName,
					CodeSigningCertificateTokenCryptographicProvider = signNupkgsUsingSettingsRequest.Settings.CodeSigning.Token.CryptographicProvider,
					CodeSigningCertificateTokenContainerName = signNupkgsUsingSettingsRequest.Settings.CodeSigning.Token.ContainerName,
					CodeSigningCertificateTokenPassword = signNupkgsUsingSettingsRequest.Settings.CodeSigning.Token.Password,
					TimeStampUri = cakeContext.GetNullableUri(signNupkgsUsingSettingsRequest.Settings.CodeSigning.TimeStampUrl),
					TimeStampDigestAlgorithm = cakeContext.GetCodeSigningDigestAlgorithm(signNupkgsUsingSettingsRequest.Settings.CodeSigning.TimeStampDigestAlgorithm),
					CertificatePath = cakeContext.GetNullableFile(signNupkgsUsingSettingsRequest.Settings.CodeSigning.CertificateFileName),
					CertificatePassword = signNupkgsUsingSettingsRequest.Settings.CodeSigning.CertificatePassword,
					CertificateFingerprint = signNupkgsUsingSettingsRequest.Settings.CodeSigning.CertificateFingerprint,
					DigestAlgorithm = cakeContext.GetCodeSigningDigestAlgorithm(signNupkgsUsingSettingsRequest.Settings.CodeSigning.DigestAlgorithm),
					RunAsync = signNupkgsUsingSettingsRequest.Settings.CodeSigning.RunAsync,
				};
			}

			if (signNupkgsRequest.RemoteCodeSigningServiceUri == null)
			{
				var codeSigningApi = new ISI.Extensions.VisualStudio.CodeSigningApi(new CakeContextLogger(cakeContext));

				codeSigningApi.SignNupkgs(new ISI.Extensions.VisualStudio.DataTransferObjects.CodeSigningApi.SignNupkgsRequest()
				{
					NupkgFullNames = request.NupkgPaths.ToNullCheckedArray(assemblyPath => assemblyPath.FullPath),
					OutputDirectory = request.OutputDirectory?.FullPath,
					WorkingDirectory = cakeContext.Environment?.WorkingDirectory?.FullPath,
					CodeSigningCertificateTokenCertificateFileName = signNupkgsRequest.CodeSigningCertificateTokenCertificateFileName,
					CodeSigningCertificateTokenCryptographicProvider = signNupkgsRequest.CodeSigningCertificateTokenCryptographicProvider,
					CodeSigningCertificateTokenContainerName = signNupkgsRequest.CodeSigningCertificateTokenContainerName,
					CodeSigningCertificateTokenPassword = signNupkgsRequest.CodeSigningCertificateTokenPassword,
					TimeStampUri = signNupkgsRequest.TimeStampUri,
					TimeStampDigestAlgorithm = signNupkgsRequest.TimeStampDigestAlgorithm.ToCodeSigningDigestAlgorithm(),
					CertificateFileName = signNupkgsRequest.CertificatePath?.FullPath,
					CertificatePassword = signNupkgsRequest.CertificatePassword,
					CertificateStoreName = signNupkgsRequest.CertificateStoreName,
					CertificateStoreLocation = signNupkgsRequest.CertificateStoreLocation,
					CertificateSubjectName = signNupkgsRequest.CertificateSubjectName,
					CertificateFingerprint = signNupkgsRequest.CertificateFingerprint,
					DigestAlgorithm = signNupkgsRequest.DigestAlgorithm.ToCodeSigningDigestAlgorithm(),
					OverwriteAnyExistingSignature = signNupkgsRequest.OverwriteAnyExistingSignature,
					RunAsync = signNupkgsRequest.RunAsync,
					Verbosity = signNupkgsRequest.Verbosity.ToCodeSigningVerbosity(),
				});
			}
			else
			{
				var remoteCodeSigningApi = new ISI.Services.SCM.RemoteCodeSigning.Rest.RemoteCodeSigningApi(new CakeContextLogger(cakeContext), new ISI.Extensions.DateTimeStamper.LocalMachineDateTimeStamper());

				remoteCodeSigningApi.SignNupkgs(new ISI.Services.SCM.RemoteCodeSigning.DataTransferObjects.RemoteCodeSigningApi.SignNupkgsRequest()
				{
					RemoteCodeSigningServiceUrl = signNupkgsRequest.RemoteCodeSigningServiceUri.ToString(),
					RemoteCodeSigningServicePassword = signNupkgsRequest.RemoteCodeSigningServicePassword,
					NupkgFullNames = request.NupkgPaths.ToNullCheckedArray(assemblyPath => assemblyPath.FullPath),
					OutputDirectory = request.OutputDirectory?.FullPath,
					OverwriteAnyExistingSignature = signNupkgsRequest.OverwriteAnyExistingSignature,
					RunAsync = signNupkgsRequest.RunAsync,
					Verbosity = signNupkgsRequest.Verbosity.ToRemoteCodeSigningVerbosity(),
				});
			}

			return response;
		}
	}
}