using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.CodeGeneration
{
	public class RevertRcsKeywordsRequest
	{
		public ISI.Extensions.Scm.DataTransferObjects.RcsKeywordProcessorApi.ReplaceRcsKeywordsFile[] ModifiedFiles { get; set; }
	}
}