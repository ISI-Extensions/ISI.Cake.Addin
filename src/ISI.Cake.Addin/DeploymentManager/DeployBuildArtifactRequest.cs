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

namespace ISI.Cake.Addin.DeploymentManager
{
	public class DeployBuildArtifactRequest : IWarmUpWebService
	{
		public Uri WindowsDeploymentApiUri { get; set; }
		public string WindowsDeploymentApiKey { get; set; }

		public Uri BuildArtifactsApiUri { get; set; }
		public string BuildArtifactsApiKey { get; set; }

		public string BuildArtifactName { get; set; }

		public string BuildArtifactDateTimeStampVersionUrl { get; set; }
		public string BuildArtifactDownloadUrl { get; set; }

		public ISI.Extensions.Scm.DateTimeStampVersion ToDateTimeStampVersion { get; set; }
		public string FromEnvironment { get; set; }
		public string ToEnvironment { get; set; }
		public string ConfigurationKey { get; set; }
		public IEnumerable<IDeployComponent> Components { get; set; }
		public bool SetDeployedVersion { get; set; } = true;
		public bool ThrowExceptionWhenVersionIsAlreadyDeployed { get; set; } = true;
		public bool ThrowExceptionWhenComponentInUse { get; set; } = true;
		public bool ThrowExceptionWhenWouldNotStart { get; set; } = true;
		public bool ThrowExceptionWhenNotSuccessful { get; set; } = true;
		public bool RunAsync { get; set; } = true;

		Uri IWarmUpWebService.WebServiceUri => WindowsDeploymentApiUri;
		public bool WarmUpWebService { get; } = true;
		public int WarmUpWebServiceMaxTries { get; set; } = 5;
	}
}