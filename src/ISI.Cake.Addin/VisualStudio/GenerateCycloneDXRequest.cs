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
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.VisualStudio
{
	public class GenerateCycloneDXRequest
	{
		public string FullName { get; set; }

		public string Framework { get; set; }
		public string Runtime { get; set; }

		public string OutputDirectory { get; set; }
		public string OutputFilename { get; set; }
		public bool OutputJson { get; set; }

		public ISI.Extensions.Sbom.DataTransferObjects.SbomApi.GenerateCycloneDxRequestDependency[] ExcludeDependencies { get; set; }

		public bool ExcludeDevelopmentDependencies { get; set; }
		public bool ExcludeTestProjects { get; set; }

		public string AlternativeNugetUrl { get; set; }
		public string AlternativeNugetUserName { get; set; }
		public string AlternativeNugetPasswordApiKey { get; set; }
		public bool AlternativeNugetPasswordIsClearText { get; set; }

		public bool Recursive { get; set; }

		public bool OmitSerialNumber { get; set; }

		public string GitHubUserName { get; set; }
		public string GitHubToken { get; set; }
		public string GitHubBearerToken { get; set; }
		public bool GitHubEnableLicenses { get; set; }

		public bool DisablePackageRestore { get; set; }
		public bool DisableHashComputation { get; set; }
		public TimeSpan? DotnetCommandTimeout { get; set; }

		public string BaseIntermediateOutputPath { get; set; }
		public string ImportMetadataPath { get; set; }
		public bool IncludeProjectPeferences { get; set; }
		public string SetName { get; set; }
		public Version? SetVersion { get; set; }
		public ISI.Extensions.Sbom.DataTransferObjects.SbomApi.GenerateCycloneDxRequestCycloneComponentType? SetComponentType { get; set; }
		public string SetNugetPurl { get; set; }
	}
}