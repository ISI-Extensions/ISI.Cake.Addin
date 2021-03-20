#region Copyright & License
/*
Copyright (c) 2021, Integrated Solutions, Inc.
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

namespace ISI.Cake.Addin
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgSign(this global::Cake.Core.ICakeContext cakeContext, global::Cake.Core.IO.FilePath nupkgFilePath, NupkgSignToolSettings nupkgSignToolSettings)
		{
			NupkgSign(cakeContext, new []{ nupkgFilePath }, nupkgSignToolSettings);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgSign(this global::Cake.Core.ICakeContext cakeContext, IEnumerable<global::Cake.Core.IO.FilePath> nupkgFilePaths, NupkgSignToolSettings nupkgSignToolSettings)
		{
			NupkgSign(cakeContext, nupkgFilePaths.Select(filePath => filePath.FullPath), nupkgSignToolSettings);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgSign(this global::Cake.Core.ICakeContext cakeContext, string nupkgFullName, NupkgSignToolSettings nupkgSignToolSettings)
		{
			NupkgSign(cakeContext, new []{ nupkgFullName }, nupkgSignToolSettings);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgSign(this global::Cake.Core.ICakeContext cakeContext, IEnumerable<string> nupkgFullNames, NupkgSignToolSettings nupkgSignToolSettings)
		{
			var arguments = new StringBuilder();

			arguments.AppendFormat(" -Timestamper \"{0}\"", nupkgSignToolSettings.TimestamperUri);
			arguments.AppendFormat(" -TimestampHashAlgorithm \"{0}\"", nupkgSignToolSettings.TimestampHashAlgorithm);

			if (!string.IsNullOrWhiteSpace(nupkgSignToolSettings.OutputDirectory?.FullPath))
			{
				arguments.AppendFormat(" -OutputDirectory \"{0}\"", nupkgSignToolSettings.OutputDirectory.FullPath);
			}

			if (string.IsNullOrWhiteSpace(nupkgSignToolSettings.CertificatePath?.FullPath))
			{
				arguments.AppendFormat(" -CertificateStoreName \"{0}\"", nupkgSignToolSettings.CertificateStoreName);
				arguments.AppendFormat(" -CertificateStoreLocation \"{0}\"", nupkgSignToolSettings.CertificateStoreLocation);
				if (!string.IsNullOrWhiteSpace(nupkgSignToolSettings.CertificateSubjectName))
				{
					arguments.AppendFormat(" -CertificateSubjectName \"{0}\"", nupkgSignToolSettings.CertificateSubjectName);
				}
				if (!string.IsNullOrWhiteSpace(nupkgSignToolSettings.CertificateFingerprint))
				{
					arguments.AppendFormat(" -CertificateFingerprint \"{0}\"", nupkgSignToolSettings.CertificateFingerprint);
				}
			}
			else
			{
				arguments.AppendFormat(" -CertificatePath \"{0}\"", nupkgSignToolSettings.CertificatePath.FullPath);
				arguments.AppendFormat(" -CertificatePassword \"{0}\"", nupkgSignToolSettings.CertificatePassword);
			}
			arguments.AppendFormat(" -HashAlgorithm \"{0}\"", nupkgSignToolSettings.HashAlgorithm);

			if (nupkgSignToolSettings.OverwriteAnyExistingSignature)
			{
				arguments.Append(" -Overwrite");
			}

			arguments.AppendFormat(" -Verbosity \"{0}\"", nupkgSignToolSettings.Verbosity);

			foreach (var nupkgFullName in nupkgFullNames)
			{
				ISI.Cake.Addin.Process.WaitForProcessResponse(cakeContext, "nuget", string.Format("sign \"{0}\" {1}", nupkgFullName, arguments.ToString()));
			}
		}
	}
}