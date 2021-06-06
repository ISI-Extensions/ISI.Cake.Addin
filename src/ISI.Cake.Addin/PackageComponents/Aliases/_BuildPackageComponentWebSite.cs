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
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.MSBuild;
using ISI.Cake.Addin.XmlTransform;

namespace ISI.Cake.Addin.PackageComponents
{
	public static partial class Aliases
	{
		private static void BuildPackageComponentWebSite(global::Cake.Core.ICakeContext cakeContext, string configuration, global::Cake.Common.Tools.MSBuild.MSBuildPlatform platform, string packageComponentsDirectory, AssemblyVersionFileDictionary assemblyVersionFiles, PackageComponentWebSite packageComponent)
		{
			var projectName = System.IO.Path.GetFileNameWithoutExtension(packageComponent.ProjectFullName);
			var projectDirectory = System.IO.Path.GetDirectoryName(packageComponent.ProjectFullName);
			var packageComponentDirectory = System.IO.Path.Combine(packageComponentsDirectory, projectName);
			
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "Package Component WebSite");
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "  ProjectName: {0}", projectName);
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "  ProjectDirectory: {0}", projectDirectory);
			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "  PackageComponentDirectory: {0}", packageComponentDirectory);

			using (var tempBuildDirectory = new ISI.Extensions.IO.Path.TempDirectory())
			{
				var buildDirectory = tempBuildDirectory.FullName;
				
				using (var tempPublishDirectory = new ISI.Extensions.IO.Path.TempDirectory())
				{
					var publishDirectory = tempPublishDirectory.FullName;

					if (assemblyVersionFiles != null)
					{
						cakeContext.SetAssemblyVersionFiles(assemblyVersionFiles);
					}

					try
					{
						if (platform == MSBuildPlatform.Automatic)
						{
							cakeContext.MSBuild(packageComponent.ProjectFullName, configurator => configurator
								.SetConfiguration(configuration)
								.SetVerbosity(global::Cake.Core.Diagnostics.Verbosity.Quiet)
								.WithProperty("Platform", string.Empty)
								.WithProperty("DebugSymbols", "true")
								.WithProperty("OutputPath", System.IO.Path.Combine(buildDirectory, "bin"))
								.WithProperty("DeployOnBuild", "true")
								.WithProperty("WebPublishMethod", "FileSystem")
								.WithProperty("PackageAsSingleFile", "true")
								.WithProperty("SkipInvalidConfigurations", "true")
								.WithProperty("publishUrl", publishDirectory)
								.WithProperty("DeployDefaultTarget", "WebPublish"));
						}
						else
						{
							cakeContext.MSBuild(packageComponent.ProjectFullName, configurator => configurator
								.SetConfiguration(configuration)
								.SetVerbosity(global::Cake.Core.Diagnostics.Verbosity.Quiet)
								.SetMSBuildPlatform(platform)
								.WithProperty("DebugSymbols", "true")
								.WithProperty("OutputPath", System.IO.Path.Combine(buildDirectory, "bin"))
								.WithProperty("DeployOnBuild", "true")
								.WithProperty("WebPublishMethod", "FileSystem")
								.WithProperty("PackageAsSingleFile", "true")
								.WithProperty("SkipInvalidConfigurations", "true")
								.WithProperty("publishUrl", publishDirectory)
								.WithProperty("DeployDefaultTarget", "WebPublish"));
						}

						System.IO.Directory.CreateDirectory(packageComponentDirectory);

						if (!string.IsNullOrWhiteSpace(packageComponent.IconFullName) && System.IO.File.Exists(packageComponent.IconFullName))
						{
							ISI.Extensions.DirectoryIcon.SetDirectoryIcon(packageComponentDirectory, packageComponent.IconFullName);
						}

						cakeContext.CopyDirectory(publishDirectory, packageComponentDirectory);
					}
					finally
					{
						if (assemblyVersionFiles != null)
						{
							cakeContext.ResetAssemblyVersionFiles(assemblyVersionFiles);
						}
					}
				}
			}





			// /p:DeployOnBuild=true/p:WebPublishMethod=Package/p:PackageAsSingleFile=true/p:PackageLocation="C:\temp\web.zip"
			//if (platform == MSBuildPlatform.Automatic)
			//{
			//	cakeContext.MSBuild(packageComponent.ProjectFullName, configurator => configurator
			//		.SetConfiguration(configuration)
			//		.SetVerbosity(global::Cake.Core.Diagnostics.Verbosity.Quiet)
			//		.WithProperty("Platform", string.Empty)
			//		.WithProperty("OutputPath", System.IO.Path.Combine(projectDirectory, "bin"))
			//		.WithProperty("OutDir", packageComponentDirectory)
			//		.WithProperty("Retries", "1")
			//		.WithProperty("RetryDelayMilliseconds", "100")
			//		.SetMaxCpuCount(0)
			//		.SetNodeReuse(false)
			//		.SetPlatformTarget(PlatformTarget.MSIL)
			//		.WithTarget("_CopyWebApplication"));
			//}
			//else
			//{
			//	cakeContext.MSBuild(packageComponent.ProjectFullName, configurator => configurator
			//		.SetConfiguration(configuration)
			//		.SetVerbosity(global::Cake.Core.Diagnostics.Verbosity.Quiet)
			//		.SetMSBuildPlatform(platform)
			//		.WithProperty("OutputPath", System.IO.Path.Combine(projectDirectory, "bin"))
			//		.WithProperty("OutDir", packageComponentDirectory)
			//		.WithProperty("Retries", "1")
			//		.WithProperty("RetryDelayMilliseconds", "100")
			//		.SetMaxCpuCount(0)
			//		.SetNodeReuse(false)
			//		.SetPlatformTarget(PlatformTarget.MSIL)
			//		.WithTarget("_CopyWebApplication"));
			//}

			cakeContext.XmlTransformConfigsInProject(new ISI.Cake.Addin.XmlTransform.XmlTransformConfigsInProjectRequest()
			{
				ProjectFullName = packageComponent.ProjectFullName,
				DestinationDirectory = packageComponentDirectory,
				MoveConfigurationKey = true,
				TransformedFileSuffix = ".sample",
			});

			foreach (var webConfigFullName in System.IO.Directory.GetFiles(packageComponentDirectory, "web.config*", System.IO.SearchOption.TopDirectoryOnly))
			{
				System.IO.File.Move(webConfigFullName, System.IO.Path.Combine(packageComponentDirectory, System.IO.Path.GetFileName(webConfigFullName)));
			}
						
			foreach (var appConfigFullName in System.IO.Directory.GetFiles(packageComponentDirectory, "web.*.config", System.IO.SearchOption.TopDirectoryOnly))
			{
				System.IO.File.Delete(appConfigFullName);
			}

			{
				var webConfigFullName = System.IO.Path.Combine(packageComponentDirectory, "web.config");
				if (System.IO.File.Exists(webConfigFullName))
				{
					System.IO.File.Move(webConfigFullName, string.Format("{0}.sample", webConfigFullName));
				}
			}

			CopyCmsData(projectDirectory, packageComponentDirectory);

			CopyDeploymentFiles(projectDirectory, packageComponentDirectory);

			cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "Finish");
		}
	}
}