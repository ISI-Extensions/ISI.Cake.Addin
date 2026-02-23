using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.CodeGeneration
{
	public class ReplaceRcsKeywordsRequest
	{
		public string SourceDirectory { get; set; }
		public string FileNameExtension { get; set; }
		public bool IncludeChildDirectories { get; set; }

		public ISI.Extensions.Scm.DateTimeStampVersion DateTimeStampVersion { get; set; }
	}
}