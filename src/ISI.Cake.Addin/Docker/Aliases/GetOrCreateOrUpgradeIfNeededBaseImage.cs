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
		public static IGetOrCreateOrUpgradeIfNeededBaseImageRequest SetDotNetAspNetSourceBaseImage(this global::Cake.Core.ICakeContext cakeContext, IGetOrCreateOrUpgradeIfNeededBaseImageRequest request, string project)
		{
			ServiceProvider.Initialize();

			request.SourceBaseImageContainerRegistry = "mcr.microsoft.com";
			request.SourceBaseImageContainerRepository = "dotnet/aspnet";
			request.SourceBaseImageContainerImageTag = "10.0-alpine";
			request.ImageContainerRepositoryType = ImageContainerRepositoryType.Alpine;

			var logger = new CakeContextLogger(cakeContext);

			var projectApi = new ISI.Extensions.VisualStudio.ProjectApi(logger);

			var targetFrameworkVersion = projectApi.GetTargetFrameworkVersions(new()
			{
				Project = project,
			}).TargetFrameworkVersions.NullCheckedFirstOrDefault(targetFrameworkVersion => targetFrameworkVersion.StartsWith("net", StringComparison.InvariantCultureIgnoreCase));

			if (!string.IsNullOrWhiteSpace(targetFrameworkVersion))
			{
				request.SourceBaseImageContainerImageTag = $"{targetFrameworkVersion.TrimStart("net", StringComparison.InvariantCultureIgnoreCase)}-alpine";
			}

			return request;
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static IGetOrCreateOrUpgradeIfNeededBaseImageRequest SetUbuntuSourceBaseImage(this global::Cake.Core.ICakeContext cakeContext, IGetOrCreateOrUpgradeIfNeededBaseImageRequest request, string project)
		{
			ServiceProvider.Initialize();

			request.SourceBaseImageContainerRegistry = null;
			request.SourceBaseImageContainerRepository = "ubuntu";
			request.SourceBaseImageContainerImageTag = "resolute";
			request.ImageContainerRepositoryType = ImageContainerRepositoryType.Ubuntu;

			return request;
		}

		public class ImageArtifact
		{
			public IGetOrCreateOrUpgradeIfNeededBaseImageRequestArtifact Artifact { get; set; }
			public string ExistingVersion { get; set; }
			public string RepositoryVersion { get; set; }
			public string TempDownloadFullName { get; set; }
			public string TempExtractionDirectory { get; set; }
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static GetOrCreateOrUpgradeIfNeededBaseImageResponse GetOrCreateOrUpgradeIfNeededBaseImage(this global::Cake.Core.ICakeContext cakeContext, IGetOrCreateOrUpgradeIfNeededBaseImageRequest request)
		{
			ServiceProvider.Initialize();

			var response = new GetOrCreateOrUpgradeIfNeededBaseImageResponse();

			var logger = new CakeContextLogger(cakeContext);

			var context = (request as GetOrCreateOrUpgradeIfNeededBaseImageRequest)?.Context ?? (request as GetOrCreateOrUpgradeIfNeededBaseImageUsingSettingsRequest)?.Settings?.GetValue("linux-build-context", "");
			var buildArtifactsApiUri = (request as GetOrCreateOrUpgradeIfNeededBaseImageRequest)?.BuildArtifactsApiUri ?? (request as GetOrCreateOrUpgradeIfNeededBaseImageUsingSettingsRequest)?.Settings?.BuildArtifacts?.ApiUrl?.GetNullableUri();
			var buildArtifactsApiKey = (request as GetOrCreateOrUpgradeIfNeededBaseImageRequest)?.BuildArtifactsApiKey ?? (request as GetOrCreateOrUpgradeIfNeededBaseImageUsingSettingsRequest)?.Settings?.BuildArtifacts?.ApiKey;
			var baseImageContainerRegistry = (request as GetOrCreateOrUpgradeIfNeededBaseImageRequest)?.BaseImageContainerRegistry ?? (request as GetOrCreateOrUpgradeIfNeededBaseImageUsingSettingsRequest)?.Settings?.DockerRegistry?.DomainName;
			var baseImageContainerRepository = request.BaseImageContainerRepository;
			var imageArtifacts = request.Artifacts.ToNullCheckedArray(artifact => new ImageArtifact()
			{
				Artifact = artifact,
			}, NullCheckCollectionResult.Empty);

			if (request is GetOrCreateOrUpgradeIfNeededBaseImageUsingSettingsRequest alpineBaseImageUsingSettingsRequest)
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

			if (request.ImageContainerRepositoryType == ImageContainerRepositoryType.Unknown)
			{
				if (request.SourceBaseImageContainerRegistry.IndexOf("alpine", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					request.ImageContainerRepositoryType = ImageContainerRepositoryType.Alpine;
				}
				else if (request.SourceBaseImageContainerRegistry.IndexOf("ubuntu", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					request.ImageContainerRepositoryType = ImageContainerRepositoryType.Ubuntu;
				}
				else if (request.SourceBaseImageContainerRegistry.IndexOf("debian", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					request.ImageContainerRepositoryType = ImageContainerRepositoryType.Debian;
				}
			}

			if (string.IsNullOrWhiteSpace(baseImageContainerRepository))
			{
				if (imageArtifacts.Any())
				{
					throw new NullReferenceException($"{nameof(IGetOrCreateOrUpgradeIfNeededBaseImageRequest.BaseImageContainerRepository)} must be set if using {nameof(IGetOrCreateOrUpgradeIfNeededBaseImageRequest.Artifacts)}");
				}

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
				if (request.IncludeSqlite)
				{
					baseImageContainerRepositoryBuilder.Append("-sqlite");
				}
				if (request.IncludeGit)
				{
					baseImageContainerRepositoryBuilder.Append("-git");
				}
				if (request.IncludeAnt)
				{
					baseImageContainerRepositoryBuilder.Append("-ant");
				}
				if (request.IncludeLibnsl)
				{
					baseImageContainerRepositoryBuilder.Append("-libnsl");
				}
				if (request.IncludeSshPass)
				{
					baseImageContainerRepositoryBuilder.Append("-sshpass");
				}

				baseImageContainerRepository = baseImageContainerRepositoryBuilder.ToString();
			}

			logger.LogInformation($"Checking: {baseImageContainerRegistry}/{baseImageContainerRepository}");

			var alpinePackages = new List<string>();
			var ubuntuPackages = new List<string>();
			var repositoryPackageVersionByPackage = new Dictionary<string, string>(StringComparer.InvariantCulture);
			var existingPackageVersionByPackage = new Dictionary<string, string>(StringComparer.InvariantCulture);

			if (request.IncludeMicrosoftFonts)
			{
				alpinePackages.Add("msttcorefonts-installer");
				alpinePackages.Add("fontconfig");

				ubuntuPackages.Add("ttf-mscorefonts-installer");
			}

			if (request.DisableInvariantGlobalization)
			{
				alpinePackages.Add("libgdiplus");
				//alpinePackages.Add((PackageName: "libc6-compat", RepositoryVersion: null, ExistingVersion: null));
				alpinePackages.Add("icu-data-full");
				alpinePackages.Add("icu-libs");

				ubuntuPackages.Add("libgdiplus");
				ubuntuPackages.Add("libicu-dev");
				ubuntuPackages.Add("icu-devtools");
			}

			if (request.IncludeOpenjdk8)
			{
				alpinePackages.Add("openjdk8");

				ubuntuPackages.Add("openjdk-8-jdk");
			}

			if (request.IncludeOpenssl)
			{
				alpinePackages.Add("openssl");

				ubuntuPackages.Add("openssl");
				ubuntuPackages.Add("libssl-dev");
			}

			if (request.IncludeCaCertificates)
			{
				alpinePackages.Add("ca-certificates");

				ubuntuPackages.Add("ca-certificates");
			}

			if (request.IncludeLdap)
			{
				alpinePackages.Add("libldap");

				ubuntuPackages.Add("libldap2");
			}

			if (request.IncludeSqlite)
			{
				alpinePackages.Add("sqlite");

				ubuntuPackages.Add("sqlite3");
				ubuntuPackages.Add("libsqlite3-dev");
			}

			if (request.IncludeGit)
			{
				alpinePackages.Add("git");

				ubuntuPackages.Add("git");
			}

			if (request.IncludeAnt)
			{
				alpinePackages.Add("apache-ant");

				ubuntuPackages.Add("ant");
			}

			if (request.IncludeLibnsl)
			{
				alpinePackages.Add("libnsl-dev");

				ubuntuPackages.Add("libnsl-dev");
			}

			if (request.IncludeSshPass)
			{
				alpinePackages.Add("sshpass");

				ubuntuPackages.Add("sshpass");
			}

			var shouldBuild = false;

			string getVersionKey(string name)
			{
				return $"{name}-version";
			}

			switch (request.ImageContainerRepositoryType)
			{
				case ImageContainerRepositoryType.Alpine:
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
							logger.LogInformation("Package Versions Don't Match !!!!!");

							shouldBuild = true;
						}
					}
					break;
				
				case ImageContainerRepositoryType.Ubuntu:
					foreach (var ubuntuPackage in ubuntuPackages)
					{
						var repositoryVersion = (ISI.Cake.Addin.Debian.Aliases.GetPackageDetails(cakeContext, new ISI.Cake.Addin.Debian.GetPackageDetailsRequest()
						{
							Distribution = "Ubuntu",
							Branch = "resolute",
							Architecture = "amd64",
							Package = ubuntuPackage,
						}).PackageDetails?.Version ?? string.Empty).Trim();
						repositoryPackageVersionByPackage[ubuntuPackage] = repositoryVersion;

						var existingVersion = (ISI.Cake.Addin.BuildArtifacts.Aliases.GetBuildArtifactKeyValue(cakeContext, new ISI.Cake.Addin.BuildArtifacts.GetBuildArtifactKeyValueRequest()
						{
							BuildArtifactsApiUri = buildArtifactsApiUri,
							BuildArtifactsApiKey = buildArtifactsApiKey,
							BuildArtifactName = baseImageContainerRepository,
							Key = getVersionKey(ubuntuPackage),
						}).Value ?? string.Empty).Trim();
						existingPackageVersionByPackage[ubuntuPackage] = existingVersion;

						logger.LogInformation($"Checking Ubuntu Package: {ubuntuPackage}, Existing Version: {existingVersion}, Repository Version: {repositoryVersion}");

						if (!string.Equals(repositoryVersion, existingVersion, StringComparison.InvariantCulture))
						{
							logger.LogInformation("Package Versions Don't Match !!!!!");

							shouldBuild = true;
						}
					}
					break;

				default:
					if(alpinePackages.Any() || ubuntuPackages.Any())
					{
						throw new ArgumentOutOfRangeException();
					}
					break;
			}


			foreach (var imageArtifact in imageArtifacts)
			{
				switch (imageArtifact.Artifact)
				{
					case IGetOrCreateOrUpgradeIfNeededBaseImageRequestDownloadVersion getOrCreateOrUpgradeIfNeededBaseImageRequestDownloadVersion:
						imageArtifact.RepositoryVersion = ISI.Extensions.WebClient.Rest.ExecuteTextGet(getOrCreateOrUpgradeIfNeededBaseImageRequestDownloadVersion.DownloadVersionUrl, null, true);
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}

				imageArtifact.ExistingVersion = (ISI.Cake.Addin.BuildArtifacts.Aliases.GetBuildArtifactKeyValue(cakeContext, new ISI.Cake.Addin.BuildArtifacts.GetBuildArtifactKeyValueRequest()
				{
					BuildArtifactsApiUri = buildArtifactsApiUri,
					BuildArtifactsApiKey = buildArtifactsApiKey,
					BuildArtifactName = baseImageContainerRepository,
					Key = getVersionKey(imageArtifact.Artifact.ArtifactName),
				}).Value ?? string.Empty).Trim();

				logger.LogInformation($"Checking Artifact: {imageArtifact.Artifact.ArtifactName}, Existing Version: {imageArtifact.ExistingVersion}, Repository Version: {imageArtifact.RepositoryVersion}");

				if (!string.Equals(imageArtifact.RepositoryVersion, imageArtifact.ExistingVersion, StringComparison.InvariantCulture))
				{
					logger.LogInformation("Artifact Versions Don't Match !!!!!");

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
				logger.LogInformation("BaseImage Signatures Don't Match !!!!!");

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

					if (imageArtifacts.Any())
					{
						stringBuilder.AppendLine("FROM alpine:latest AS builder");
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("WORKDIR /app");
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("RUN apk update && apk add curl && apk add bzip2");
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("RUN mkdir -p /tmp");

						foreach (var imageArtifact in imageArtifacts)
						{
							if (imageArtifact.Artifact is IGetOrCreateOrUpgradeIfNeededBaseImageRequestDownload getOrCreateOrUpgradeIfNeededBaseImageRequestDownload)
							{
								if (imageArtifact.Artifact is IGetOrCreateOrUpgradeIfNeededBaseImageRequestArtifactTarGz)
								{
									imageArtifact.TempDownloadFullName = $"/tmp/f_{Guid.NewGuid().Formatted(GuidExtensions.GuidFormat.NoFormatting)}.tar.gz";
									imageArtifact.TempExtractionDirectory = $"/tmp/d_{Guid.NewGuid().Formatted(GuidExtensions.GuidFormat.NoFormatting)}";
									stringBuilder.AppendLine($"RUN mkdir -p {imageArtifact.TempExtractionDirectory.TrimEnd('/')}");
								}
								else if (imageArtifact.Artifact is IGetOrCreateOrUpgradeIfNeededBaseImageRequestArtifactTarBz2)
								{
									imageArtifact.TempDownloadFullName = $"/tmp/f_{Guid.NewGuid().Formatted(GuidExtensions.GuidFormat.NoFormatting)}.tar.bz2";
									imageArtifact.TempExtractionDirectory = $"/tmp/d_{Guid.NewGuid().Formatted(GuidExtensions.GuidFormat.NoFormatting)}";
									stringBuilder.AppendLine($"RUN mkdir -p {imageArtifact.TempExtractionDirectory.TrimEnd('/')}");
								}
								else
								{
									imageArtifact.TempDownloadFullName = $"/tmp/f_{Guid.NewGuid().Formatted(GuidExtensions.GuidFormat.NoFormatting)}";
								}

								stringBuilder.AppendLine($"RUN curl {getOrCreateOrUpgradeIfNeededBaseImageRequestDownload.DownloadUrl} --output {imageArtifact.TempDownloadFullName}");
							}

							if (imageArtifact.Artifact is IGetOrCreateOrUpgradeIfNeededBaseImageRequestArtifactTarGz)
							{
								stringBuilder.AppendLine($"RUN tar -zxf {imageArtifact.TempDownloadFullName} -C {imageArtifact.TempExtractionDirectory}");
							}

							if (imageArtifact.Artifact is IGetOrCreateOrUpgradeIfNeededBaseImageRequestArtifactTarBz2)
							{
								stringBuilder.AppendLine($"RUN tar -jxf {imageArtifact.TempDownloadFullName} -C {imageArtifact.TempExtractionDirectory}");
							}

							stringBuilder.AppendLine();
						}
					}

					stringBuilder.AppendLine($"FROM {ISI.Extensions.Docker.Extensions.ContainerImageExtensions.GetContainerImageReference(request.SourceBaseImageContainerRegistry, request.SourceBaseImageContainerRepository, request.SourceBaseImageContainerImageTag)} as base");
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

					switch (request.ImageContainerRepositoryType)
					{
						case ImageContainerRepositoryType.Alpine:
							if (alpinePackages.Any())
							{
								stringBuilder.Append("RUN apk update");
								foreach (var alpinePackage in alpinePackages)
								{
									stringBuilder.Append($" && apk add {alpinePackage}");
								}

								stringBuilder.AppendLine();
							}

							if (request.IncludeMicrosoftFonts)
							{
								stringBuilder.AppendLine("RUN update-ms-fonts");
								stringBuilder.AppendLine();
								stringBuilder.AppendLine("RUN fc-cache -f");
								stringBuilder.AppendLine();
							}
							break;

						case ImageContainerRepositoryType.Ubuntu:
							if (ubuntuPackages.Any())
							{
								stringBuilder.Append("RUN apt update && apt install");
								foreach (var ubuntuPackage in ubuntuPackages)
								{
									stringBuilder.Append($" {ubuntuPackage}");
								}
								stringBuilder.Append(" -y");

								stringBuilder.AppendLine();
							}

							if (request.IncludeMicrosoftFonts)
							{
								stringBuilder.AppendLine("RUN fc-cache -f -v");
								stringBuilder.AppendLine();
							}
							break;
					}

					if (imageArtifacts.Any())
					{
						foreach (var imageArtifact in imageArtifacts)
						{
							if (!string.IsNullOrWhiteSpace(imageArtifact.TempExtractionDirectory))
							{
								stringBuilder.AppendLine($"RUN mkdir -p {imageArtifact.Artifact.DeployToDirectory.TrimEnd('/')}");
								stringBuilder.AppendLine($"COPY --from=builder {imageArtifact.TempExtractionDirectory}/ {imageArtifact.Artifact.DeployToDirectory.TrimEnd('/')}/");
								if (!string.IsNullOrWhiteSpace(imageArtifact.Artifact.PostDeployCommand))
								{
									stringBuilder.AppendLine($"RUN {imageArtifact.Artifact.PostDeployCommand}");
								}
							}
						}

						stringBuilder.AppendLine();
					}

					System.IO.File.WriteAllText(System.IO.Path.Combine(tempDirectory.FullName, "Dockerfile"), stringBuilder.ToString());

					response.Errored = DockerBuildImage(cakeContext, new ISI.Cake.Addin.Docker.DockerBuildImageRequest()
					{
						AppDirectory = tempDirectory.FullName,
						Context = context,

						ContainerRegistry = baseImageContainerRegistry,
						ContainerRepository = baseImageContainerRepository,
						ContainerImageTags =
						[
							baseImageContainerImageTag
						],
					}).Errored;

					if (response.Errored)
					{
						throw new Exception("Error Building Base Image");
					}
				}

				if (!response.Errored)
				{
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

					foreach (var imageArtifact in imageArtifacts)
					{
						var key = getVersionKey(imageArtifact.Artifact.ArtifactName);
						var repositoryVersion = imageArtifact.RepositoryVersion;

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
			}

			if (!response.Errored)
			{
				response.BaseImageContainerRegistry = baseImageContainerRegistry;
				response.BaseImageContainerRepository = baseImageContainerRepository;
				response.BaseImageContainerImageTag = baseImageContainerImageTag;
			}

			return response;
		}
	}
}