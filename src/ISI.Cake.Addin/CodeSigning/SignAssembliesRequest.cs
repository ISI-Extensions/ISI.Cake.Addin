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

namespace ISI.Cake.Addin.CodeSigning
{
	public interface ISignAssembliesRequest
	{
		global::Cake.Core.IO.FilePathCollection AssemblyPaths { get; }

		global::Cake.Core.IO.DirectoryPath OutputDirectory { get; }
	}

	public class SignAssembliesRequest : ISignAssembliesRequest
	{
		public global::Cake.Core.IO.FilePathCollection AssemblyPaths { get; set; }

		public global::Cake.Core.IO.DirectoryPath OutputDirectory { get; set; }

		public Uri RemoteCodeSigningServiceUri { get; set; }
		public string RemoteCodeSigningServiceApiKey { get; set; }

		public string CodeSigningCertificateTokenCertificateFileName { get; set; }
		public string CodeSigningCertificateTokenCryptographicProvider { get; set; }
		public string CodeSigningCertificateTokenContainerName { get; set; }
		public string CodeSigningCertificateTokenPassword { get; set; }

		public Uri TimeStampUri { get; set; } = new("http://timestamp.digicert.com");
		public CodeSigningDigestAlgorithm TimeStampDigestAlgorithm { get; set; } = CodeSigningDigestAlgorithm.Sha256;

		public global::Cake.Core.IO.FilePath CertificatePath { get; set; }
		public string CertificatePassword { get; set; }
		public string CertificateStoreName { get; set; } = "My";
		public string CertificateStoreLocation { get; set; } = "CurrentUser";
		public string CertificateSubjectName { get; set; }
		public string CertificateFingerprint { get; set; }

		public CodeSigningDigestAlgorithm DigestAlgorithm { get; set; } = CodeSigningDigestAlgorithm.Sha256;

		public bool OverwriteAnyExistingSignature { get; set; } = false;

		public bool RunAsync { get; set; } = false;

		public CodeSigningVerbosity Verbosity { get; set; } = CodeSigningVerbosity.Normal;
	}

	public class SignAssembliesUsingSettingsRequest : ISignAssembliesRequest
	{
		public global::Cake.Core.IO.FilePathCollection AssemblyPaths { get; set; }

		public global::Cake.Core.IO.DirectoryPath OutputDirectory { get; set; }

		public ISI.Extensions.Scm.Settings Settings { get; set; }
	}
}