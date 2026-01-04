#region Copyright & License
/*
Copyright (c) 2026, Integrated Solutions, Inc.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

		* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
		* Neither the name of the Integrated Solutions, Inc. nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
 
using Cake.Common.IO;
using Cake.Common.Solution.Project.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static ISI.Extensions.VisualStudio.AssemblyVersionFileDictionary GetAssemblyVersionFiles(this global::Cake.Core.ICakeContext cakeContext, string rootAssemblyVersionKey, string buildRevision)
		{
			return GetAssemblyVersionFiles(cakeContext, rootAssemblyVersionKey, buildRevision, (string)null);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static ISI.Extensions.VisualStudio.AssemblyVersionFileDictionary GetAssemblyVersionFiles(this global::Cake.Core.ICakeContext cakeContext, string rootAssemblyVersionKey, string buildRevision, global::Cake.Core.IO.FilePath solution)
		{
			return GetAssemblyVersionFiles(cakeContext, rootAssemblyVersionKey, buildRevision, solution?.MakeAbsolute(cakeContext.Environment).FullPath);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static ISI.Extensions.VisualStudio.AssemblyVersionFileDictionary GetAssemblyVersionFiles(this global::Cake.Core.ICakeContext cakeContext, string rootAssemblyVersionKey, string buildRevision, string solution)
		{
			ServiceProvider.Initialize();

			var logger = new CakeContextLogger(cakeContext);

			var dateTimeStamper = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.DateTimeStamper.IDateTimeStamper>();
			var jsonSerializer = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.JsonSerialization.IJsonSerializer>();
			var configuration = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.VisualStudio.Configuration>();
			var nugetApi = new ISI.Extensions.Nuget.NugetApi(ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.Nuget.Configuration>(), logger, jsonSerializer);

			var solutionApi = new ISI.Extensions.VisualStudio.SolutionApi(configuration, logger, jsonSerializer, new ISI.Extensions.Scm.BuildScriptApi(logger), new ISI.Extensions.Scm.SourceControlClientApi(logger), new ISI.Extensions.VisualStudio.MSBuildApi(logger, new ISI.Extensions.VisualStudio.VsWhereApi(configuration, logger, dateTimeStamper, nugetApi)), new ISI.Extensions.VisualStudio.CodeGenerationApi(logger), new ISI.Extensions.VisualStudio.ProjectApi(logger), nugetApi);

			return solutionApi.GetAssemblyVersionFiles(new ()
			{
				Solution = solution ?? cakeContext.Environment.WorkingDirectory.FullPath,
				RootAssemblyVersionKey = rootAssemblyVersionKey,
				BuildRevision = buildRevision,
			}).AssemblyVersionFiles;
		}
	}
}