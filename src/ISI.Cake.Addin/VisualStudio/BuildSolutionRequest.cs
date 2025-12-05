using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.VisualStudio
{
	public class BuildSolutionRequest
	{
		public string Configuration { get; set; } = "Release";
		public string Target { get; set; } = "Rebuild";
		public ISI.Extensions.VisualStudio.MSBuildVerbosity? Verbosity { get; set; }

		public ISI.Extensions.VisualStudio.MSBuildVersion MsBuildVersion { get; set; } = ISI.Extensions.VisualStudio.MSBuildVersion.Automatic;
		public ISI.Extensions.VisualStudio.MSBuildPlatform MsBuildPlatform { get; set; } = ISI.Extensions.VisualStudio.MSBuildPlatform.Automatic;

		public bool UseMSBuild { get; set; } = false;
	}
}