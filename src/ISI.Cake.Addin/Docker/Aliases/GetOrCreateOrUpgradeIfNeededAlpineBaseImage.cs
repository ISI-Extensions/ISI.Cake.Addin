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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISI.Cake.Addin.Extensions;
using ISI.Extensions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ISI.Cake.Addin.Docker
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static GetOrCreateOrUpgradeIfNeededAlpineBaseImageResponse GetOrCreateOrUpgradeIfNeededAlpineBaseImage(this global::Cake.Core.ICakeContext cakeContext, IGetOrCreateOrUpgradeIfNeededAlpineBaseImageRequest request)
		{
			ServiceProvider.Initialize();

			var response = new GetOrCreateOrUpgradeIfNeededAlpineBaseImageResponse();

			var logger = new CakeContextLogger(cakeContext);

			var context = (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageRequest)?.Context ?? (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageUsingSettingsRequest)?.Settings?.GetValue("linux-build-context", "");
			var buildArtifactsApiUri = (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageRequest)?.BuildArtifactsApiUri ?? (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageUsingSettingsRequest)?.Settings?.BuildArtifacts?.ApiUrl?.GetNullableUri();
			var buildArtifactsApiKey = (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageRequest)?.BuildArtifactsApiKey ?? (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageUsingSettingsRequest)?.Settings?.BuildArtifacts?.ApiKey;
			var baseImageContainerRegistry = (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageRequest)?.BaseImageContainerRegistry ?? (request as GetOrCreateOrUpgradeIfNeededAlpineBaseImageUsingSettingsRequest)?.Settings?.DockerRegistry?.DomainName;
			var baseImageContainerRepository = request.BaseImageContainerRepository;

			if (request is GetOrCreateOrUpgradeIfNeededAlpineBaseImageUsingSettingsRequest alpineBaseImageUsingSettingsRequest)
			{
				if (string.IsNullOrWhiteSpace(buildArtifactsApiKey))
				{
					buildArtifactsApiKey = ISI.Cake.Addin.BuildArtifacts.Aliases.GetBuildArtifactsApiKey(cakeContext, new ISI.Cake.Addin.BuildArtifacts.GetBuildArtifactsApiKeyUsingSettingsActiveDirectoryRequest()
					{
						Settings = alpineBaseImageUsingSettingsRequest.Settings,
					}).BuildArtifactsApiKey;
				}

				DockerLogin(cakeContext, context, alpineBaseImageUsingSettingsRequest.Settings);
			}

			if (string.IsNullOrWhiteSpace(baseImageContainerRepository))
			{
				var baseImageContainerRepositoryBuilder = new StringBuilder();

				baseImageContainerRepositoryBuilder.Append($"{request.SourceBaseImageContainerRepository.Replace("\\", "-").Replace("/", "-")}-{request.SourceBaseImageContainerImageTag}");
				if (request.IncludeMicrosoftFonts)
				{
					baseImageContainerRepositoryBuilder.Append("-msfonts");
				}
				if (request.DisableInvariantGlobalization)
				{
					baseImageContainerRepositoryBuilder.Append("-noglobal");
				}
				if (request.IncludeOpenjdk8)
				{
					baseImageContainerRepositoryBuilder.Append("-jdk8");
				}
				if (request.IncludeOpenssl)
				{
					baseImageContainerRepositoryBuilder.Append("-ssl");
				}
				if (request.IncludeCaCertificates)
				{
					baseImageContainerRepositoryBuilder.Append("-certs");
				}
				if (request.IncludeLdap)
				{
					baseImageContainerRepositoryBuilder.Append("-ldap");
				}
				if (request.IncludeGit)
				{
					baseImageContainerRepositoryBuilder.Append("-git");
				}

				baseImageContainerRepository = baseImageContainerRepositoryBuilder.ToString();
			}

			logger.LogInformation($"Checking: {baseImageContainerRegistry}/{baseImageContainerRepository}");

			var alpinePackages = new List<string>();
			var repositoryPackageVersionByPackage = new Dictionary<string, string>(StringComparer.InvariantCulture);
			var existingPackageVersionByPackage = new Dictionary<string, string>(StringComparer.InvariantCulture);



			if (request.IncludeMicrosoftFonts)
			{
				alpinePackages.Add("msttcorefonts-installer");
				alpinePackages.Add("fontconfig");
			}

			if (request.DisableInvariantGlobalization)
			{
				alpinePackages.Add("libgdiplus");
				//alpinePackages.Add((PackageName: "libc6-compat", RepositoryVersion: null, ExistingVersion: null));
				alpinePackages.Add("icu-data-full");
				alpinePackages.Add("icu-libs");
			}

			if (request.IncludeOpenjdk8)
			{
				alpinePackages.Add("openjdk8");
			}

			if (request.IncludeOpenssl)
			{
				alpinePackages.Add("openssl");
			}

			if (request.IncludeCaCertificates)
			{
				alpinePackages.Add("ca-certificates");
			}

			if (request.IncludeLdap)
			{
				alpinePackages.Add("libldap");
			}

			if (request.IncludeGit)
			{
				alpinePackages.Add("git");
			}

			var shouldBuild = false;

			string getVersionKey(string name)
			{
				return $"{name}-version";
			}

			foreach (var alpinePackage in alpinePackages)
			{
				var repositoryVersion = (ISI.Cake.Addin.Alpine.Aliases.GetPackageDetails(cakeContext, new ISI.Cake.Addin.Alpine.GetPackageDetailsRequest()
				{
					Package = alpinePackage,
				}).PackageDetails?.Version ?? string.Empty).Trim();
				repositoryPackageVersionByPackage[alpinePackage] = repositoryVersion;

				var existingVersion = (ISI.Cake.Addin.BuildArtifacts.Aliases.GetBuildArtifactKeyValue(cakeContext, new ISI.Cake.Addin.BuildArtifacts.GetBuildArtifactKeyValueRequest()
				{
					BuildArtifactsApiUri = buildArtifactsApiUri,
					BuildArtifactsApiKey = buildArtifactsApiKey,
					BuildArtifactName = baseImageContainerRepository,
					Key = getVersionKey(alpinePackage),
				}).Value ?? string.Empty).Trim();
				existingPackageVersionByPackage[alpinePackage] = existingVersion;

				logger.LogInformation($"Checking Alpine Package: {alpinePackage}, Existing Version: {existingVersion}, Repository Version: {repositoryVersion}");

				if (!string.Equals(repositoryVersion, existingVersion, StringComparison.InvariantCulture))
				{
					logger.LogInformation($"Package Versions Don't Match !!!!!");

					shouldBuild = true;
				}
			}

			var baseImageSignature = InspectImageManifest(cakeContext, new ISI.Cake.Addin.Docker.InspectImageManifestRequest()
			{
				ContainerRegistry = request.SourceBaseImageContainerRegistry,
				ContainerRepository = request.SourceBaseImageContainerRepository,
				ContainerImageTag = request.SourceBaseImageContainerImageTag,
			}).ImageManifest?.ToString() ?? string.Empty;

			var existingBaseImageSignature = (ISI.Cake.Addin.BuildArtifacts.Aliases.GetBuildArtifactKeyValue(cakeContext, new ISI.Cake.Addin.BuildArtifacts.GetBuildArtifactKeyValueRequest()
			{
				BuildArtifactsApiUri = buildArtifactsApiUri,
				BuildArtifactsApiKey = buildArtifactsApiKey,
				BuildArtifactName = baseImageContainerRepository,
				Key = getVersionKey("baseImageSignature"),
			}).Value ?? string.Empty).Trim();


			logger.LogInformation($"Checking Base Image Signature, Existing: {existingBaseImageSignature}, Repository: {baseImageSignature}");

			if (!string.Equals(existingBaseImageSignature, baseImageSignature, StringComparison.InvariantCulture))
			{
				logger.LogInformation($"BaseImage Signatures Don't Match !!!!!");

				shouldBuild = true;
			}

			var baseImageContainerImageDateTimeStampVersion = ISI.Cake.Addin.BuildArtifacts.Aliases.GetBuildArtifactEnvironmentDateTimeStampVersion(cakeContext, new ISI.Cake.Addin.BuildArtifacts.GetBuildArtifactEnvironmentDateTimeStampVersionRequest()
			{
				BuildArtifactsApiUri = buildArtifactsApiUri,
				BuildArtifactsApiKey = buildArtifactsApiKey,
				BuildArtifactName = baseImageContainerRepository,
				Environment = "Build",
				DoNotThrowErrorIfNotFound = true,
			}).DateTimeStampVersion;

			var baseImageContainerImageTag = $"v{baseImageContainerImageDateTimeStampVersion.Version}";

			if (!string.IsNullOrWhiteSpace(baseImageContainerImageTag))
			{
				var baseImageContainerImageSignature = InspectImageManifest(cakeContext, new ISI.Cake.Addin.Docker.InspectImageManifestRequest()
				{
					ContainerRegistry = baseImageContainerRegistry,
					ContainerRepository = baseImageContainerRepository,
					ContainerImageTag = baseImageContainerImageTag,
				}).ImageManifest?.ToString() ?? string.Empty;

				if (string.IsNullOrWhiteSpace(baseImageContainerImageSignature))
				{
					logger.LogInformation("Base Image missing");

					shouldBuild = true;
				}
			}


			if (shouldBuild)
			{
				logger.LogInformation("Building new Base Image");

				baseImageContainerImageDateTimeStampVersion = request.BuildDateTimeStampVersion;

				baseImageContainerImageTag = $"v{baseImageContainerImageDateTimeStampVersion.Version}";

				using (var tempDirectory = new ISI.Extensions.IO.Path.TempDirectory())
				{
					var stringBuilder = new StringBuilder();

					stringBuilder.AppendLine($"FROM {request.SourceBaseImageContainerRegistry}/{request.SourceBaseImageContainerRepository}:{request.SourceBaseImageContainerImageTag}");
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("USER root");
					stringBuilder.AppendLine();
					if (request.DisableInvariantGlobalization)
					{
						stringBuilder.AppendLine("ENV \\");
						stringBuilder.AppendLine("  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \\");
						stringBuilder.AppendLine("  LC_ALL=en_US.UTF-8 \\");
						stringBuilder.AppendLine("  LANG=en_US.UTF-8");
						stringBuilder.AppendLine();
					}
					stringBuilder.Append("RUN apk update");
					foreach (var alpinePackage in alpinePackages)
					{
						stringBuilder.Append($" && apk add {alpinePackage}");
					}
					stringBuilder.AppendLine();
					if (request.IncludeMicrosoftFonts)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("RUN update-ms-fonts");
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("RUN fc-cache -f");
					}

					System.IO.File.WriteAllText(System.IO.Path.Combine(tempDirectory.FullName, "Dockerfile"), stringBuilder.ToString());

					DockerBuildImage(cakeContext, new ISI.Cake.Addin.Docker.DockerBuildImageRequest()
					{
						AppDirectory = tempDirectory.FullName,
						Context = context,

						ContainerRegistry = baseImageContainerRegistry,
						ContainerRepository = baseImageContainerRepository,
						ContainerImageTags =
						[
							baseImageContainerImageTag
						],
					});
				}

				ISI.Cake.Addin.BuildArtifacts.Aliases.SetBuildArtifactEnvironmentDateTimeStampVersion(cakeContext, new ISI.Cake.Addin.BuildArtifacts.SetBuildArtifactEnvironmentDateTimeStampVersionRequest()
				{
					BuildArtifactsApiUri = buildArtifactsApiUri,
					BuildArtifactsApiKey = buildArtifactsApiKey,
					BuildArtifactName = baseImageContainerRepository,
					Environment = "Build",
					DateTimeStampVersion = baseImageContainerImageDateTimeStampVersion,
				});

				foreach (var alpinePackage in alpinePackages)
				{
					var key = getVersionKey(alpinePackage);
					var repositoryVersion = repositoryPackageVersionByPackage[alpinePackage];

					logger.LogInformation($"SetBuildArtifactKeyValue, Key: {key}, Value: {repositoryVersion}");

					ISI.Cake.Addin.BuildArtifacts.Aliases.SetBuildArtifactKeyValue(cakeContext, new ISI.Cake.Addin.BuildArtifacts.SetBuildArtifactKeyValueRequest()
					{
						BuildArtifactsApiUri = buildArtifactsApiUri,
						BuildArtifactsApiKey = buildArtifactsApiKey,
						BuildArtifactName = baseImageContainerRepository,
						Key = key,
						Value = repositoryVersion,
					});
				}

				{
					var key = getVersionKey("baseImageSignature");
					logger.LogInformation($"SetBuildArtifactKeyValue, Key: {key}, Value: {baseImageSignature}");

					ISI.Cake.Addin.BuildArtifacts.Aliases.SetBuildArtifactKeyValue(cakeContext, new ISI.Cake.Addin.BuildArtifacts.SetBuildArtifactKeyValueRequest()
					{
						BuildArtifactsApiUri = buildArtifactsApiUri,
						BuildArtifactsApiKey = buildArtifactsApiKey,
						BuildArtifactName = baseImageContainerRepository,
						Key = key,
						Value = baseImageSignature,
					});
				}
			}

			response.BaseImageContainerRegistry = baseImageContainerRegistry;
			response.BaseImageContainerRepository = baseImageContainerRepository;
			response.BaseImageContainerImageTag = baseImageContainerImageTag;

			return response;
		}
	}
}