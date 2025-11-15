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
	}
}