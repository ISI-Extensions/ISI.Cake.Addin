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
using ISI.Cake.Addin.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace ISI.Cake.Addin.VisualStudio
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static ISI.Extensions.Locks.ILock GetSolutionLock(this global::Cake.Core.ICakeContext cakeContext, string solutionFullName = null)
		{
			ServiceProvider.Initialize();

			var logger = new CakeContextLogger(cakeContext);

			var serialization = ISI.Extensions.ServiceLocator.Current.GetService<ISI.Extensions.Serialization.ISerialization>();

			var solutionApi = new ISI.Extensions.VisualStudio.SolutionApi(logger, serialization, new ISI.Extensions.Scm.BuildScriptApi(logger), new ISI.Extensions.Scm.SourceControlClientApi(logger), new ISI.Extensions.Nuget.NugetApi(logger));

			if (string.IsNullOrWhiteSpace(solutionFullName))
			{
				solutionFullName = cakeContext.Environment.WorkingDirectory.FullPath;
			}

			logger.LogInformation(solutionFullName);

			return solutionApi.GetSolutionLock(new ISI.Extensions.VisualStudio.DataTransferObjects.SolutionApi.GetSolutionLockRequest()
			{
				SolutionFullName = solutionFullName,
				AddToLog = description => logger.LogInformation(description),
			}).Lock;
		}
	}
}