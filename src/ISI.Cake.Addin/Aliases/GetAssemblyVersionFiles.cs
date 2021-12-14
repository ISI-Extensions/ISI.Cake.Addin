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
using Cake.Common.IO;
using Cake.Common.Solution.Project.Properties;
using LogLevel = Cake.Core.Diagnostics.LogLevel;
using Verbosity = Cake.Core.Diagnostics.Verbosity;

namespace ISI.Cake.Addin
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static ISI.Extensions.VisualStudio.AssemblyVersionFileDictionary GetAssemblyVersionFiles(this global::Cake.Core.ICakeContext cakeContext, global::Cake.Common.Solution.SolutionParserResult solution, string rootAssemblyVersionKey, string buildRevision)
		{
			var response = new ISI.Extensions.VisualStudio.AssemblyVersionFileDictionary();

			void addVersionFile(string assemblyGroupName, global::Cake.Common.IO.Paths.ConvertableFilePath filePath)
			{
				var assemblyVersion = GetAssemblyVersion(cakeContext, cakeContext.ParseAssemblyInfo(filePath).AssemblyVersion, buildRevision);

				response.Add(new ISI.Extensions.VisualStudio.AssemblyVersionFile()
				{
					FullName = filePath.Path.FullPath,
					AssemblyGroupName = assemblyGroupName,
					AssemblyVersion = assemblyVersion,
					AssemblyFileContent = System.IO.File.ReadAllText(filePath.Path.FullPath),
				});
			}

			if(!string.IsNullOrWhiteSpace(rootAssemblyVersionKey))
			{
				var assemblyVersionFile = cakeContext.File("./" + rootAssemblyVersionKey + ".Version.cs");
				if (System.IO.File.Exists(assemblyVersionFile))
				{
					addVersionFile(rootAssemblyVersionKey, assemblyVersionFile);
				}
				else
				{
					cakeContext.Log.Write(Verbosity.Normal, LogLevel.Error, "Root Assembly Version File Not Found");
				}
			}

			foreach(var projectPath in solution.Projects.Select(project => project.Path.GetDirectory()))
			{
				var assemblyGroupDirectory = System.IO.Path.GetDirectoryName(projectPath.FullPath);
				var assemblyGroupName = System.IO.Path.GetFileName(assemblyGroupDirectory);

				if(!response.ContainsKey(assemblyGroupName))
				{
					var assemblyVersionFile = cakeContext.File(System.IO.Path.Combine(assemblyGroupDirectory, string.Format("{0}.Version.cs", assemblyGroupName)));
					if(System.IO.File.Exists(assemblyVersionFile))
					{
						addVersionFile(assemblyGroupName, assemblyVersionFile);
					}
				}
			}

			return response;
		}
	}
}