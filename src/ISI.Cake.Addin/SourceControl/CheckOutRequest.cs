using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.SourceControl
{
	public class CheckOutRequest
	{
		public Guid SourceControlTypeUuid { get; set; }
		public string SourceUrl { get; set; }
		public string TargetFullName { get; set; }
	}
}