using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.BuildArtifacts
{
	public class GetOrCreateBuildArtifactRedirectEnvironmentBuildArtifactKeyValueResponse
	{
		public Guid BuildArtifactRedirectUuid { get; set; }
		public string FileName { get; set; }
	}
}