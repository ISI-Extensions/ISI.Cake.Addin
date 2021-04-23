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
using ISI.Cake.Addin.XmlTransform;

namespace ISI.Cake.Addin.PackageComponents
{
	public static partial class Aliases
	{
		private static void BuildPackageComponentWindowsService(global::Cake.Core.ICakeContext cakeContext, string configuration, global::Cake.Common.Tools.MSBuild.MSBuildPlatform platform, string packageComponentsDirectory, AssemblyVersionFileDictionary assemblyVersionFiles, PackageComponentWindowsService packageComponent)
		{
			var projectName = System.IO.Path.GetFileNameWithoutExtension(packageComponent.ProjectFullName);
			var projectDirectory = System.IO.Path.GetDirectoryName(packageComponent.ProjectFullName);
			var packageComponentDirectory = System.IO.Path.Combine(packageComponentsDirectory, projectName);
			
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "ProjectName: {0}", projectName);
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "ProjectDirectory: {0}", projectDirectory);
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "PackageComponentDirectory: {0}", packageComponentDirectory);
			
			System.IO.Directory.CreateDirectory(packageComponentDirectory);

			if (!string.IsNullOrWhiteSpace(packageComponent.IconFullName) && System.IO.File.Exists(packageComponent.IconFullName))
			{
				ISI.Extensions.DirectoryIcon.SetDirectoryIcon(packageComponentDirectory, packageComponent.IconFullName);
			}

			var projectBinDirectory = string.Format("{0}{1}", GetBinDirectory(packageComponent.ProjectFullName, configuration, platform).TrimEnd(System.IO.Path.DirectorySeparatorChar), System.IO.Path.DirectorySeparatorChar);

			var excludeFileDefinitions = GetExcludeFileDefinitions(packageComponent.ExcludeFiles);

			var sourceDirectories = System.IO.Directory.GetDirectories(projectBinDirectory, "*", System.IO.SearchOption.AllDirectories).ToList();
			sourceDirectories.Insert(0, projectBinDirectory);
			foreach (var sourceDirectory in sourceDirectories)
			{
				var relativeDirectory = (string.Equals(sourceDirectory, projectBinDirectory, StringComparison.InvariantCultureIgnoreCase) ? string.Empty : sourceDirectory.Substring(projectBinDirectory.Length));

				var relativeRootDirectory = (string.IsNullOrWhiteSpace(relativeDirectory) ? string.Empty : relativeDirectory.Split(new[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).First());

				if (string.IsNullOrWhiteSpace(relativeRootDirectory) || !ShouldExclude(excludeFileDefinitions, relativeRootDirectory))
				{
					var targetDirectory = packageComponentDirectory;

					if (!string.IsNullOrWhiteSpace(relativeDirectory))
					{
						targetDirectory = System.IO.Path.Combine(targetDirectory, relativeDirectory);
						System.IO.Directory.CreateDirectory(targetDirectory);
					}

					foreach (var sourceFullName in System.IO.Directory.GetFiles(sourceDirectory, "*", System.IO.SearchOption.TopDirectoryOnly))
					{
						var fileName = System.IO.Path.GetFileName(sourceFullName);

						if (!ShouldExclude(excludeFileDefinitions, fileName))
						{
							var targetFullName = System.IO.Path.Combine(targetDirectory, fileName);

							System.IO.File.Copy(sourceFullName, targetFullName, true);
						}
					}
				}
			}

			cakeContext.XmlTransformConfigsInProject(new ISI.Cake.Addin.XmlTransform.XmlTransformConfigsInProjectRequest()
			{
				ProjectFullName = packageComponent.ProjectFullName,
				DestinationDirectory = packageComponentDirectory,
				MoveConfigurationKey = true,
				TransformedFileSuffix = ".sample",
			});

			foreach (var appConfigFullName in System.IO.Directory.GetFiles(packageComponentDirectory, "app.config*", System.IO.SearchOption.TopDirectoryOnly))
			{
				System.IO.File.Move(appConfigFullName, System.IO.Path.Combine(packageComponentDirectory, string.Format("{0}{1}", projectName, System.IO.Path.GetFileName(appConfigFullName).Substring(3))));
			}
			
			{
				var appConfigFullName = System.IO.Path.Combine(packageComponentDirectory, string.Format("{0}.exe.config", projectName));
				if (System.IO.File.Exists(appConfigFullName))
				{
					System.IO.File.Move(appConfigFullName, string.Format("{0}.sample", appConfigFullName));
				}
			}

			CopyCmsData(projectDirectory, packageComponentDirectory);

			CopyDeploymentFiles(projectDirectory, packageComponentDirectory);
		}
	}
}