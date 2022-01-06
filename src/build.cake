#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=ISI.Cake.AddIn&loaddependencies=true

//mklink /D Secrets S:\
var settingsFullName = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("LocalAppData"), "Secrets", "ISI.keyValue");
var settings = GetSettings(settingsFullName);

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionPath = File("./ISI.Cake.Addin.sln");
var solution = ParseSolution(solutionPath);

var assemblyVersionFile = File("./ISI.Cake.Addin.Version.cs");

var buildDateTime = DateTime.UtcNow;
var buildDateTimeStamp = GetDateTimeStamp(buildDateTime);
var buildRevision = GetBuildRevision(buildDateTime);
var assemblyVersion = GetAssemblyVersion(ParseAssemblyInfo(assemblyVersionFile).AssemblyVersion, buildRevision);
Information("AssemblyVersion: {0}", assemblyVersion);

var nugetDirectory = "../Nuget";

Task("Clean")
	.Does(() =>
	{
		// Clean solution directories.
		foreach(var projectPath in solution.Projects.Select(p => p.Path.GetDirectory()))
		{
			Information("Cleaning {0}", projectPath);
			CleanDirectories(projectPath + "/**/bin/" + configuration);
			CleanDirectories(projectPath + "/**/obj/" + configuration);
		}

		CleanDirectories(nugetDirectory);

		Information("Cleaning Projects ...");
	});

Task("NugetPackageRestore")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		using(GetNugetLock())
		{
			Information("Restoring Nuget Packages ...");
			NuGetRestore(solutionPath);
		}
	});

Task("Build")
	.IsDependentOn("NugetPackageRestore")
	.Does(() => 
	{
		CreateAssemblyInfo(assemblyVersionFile, new AssemblyInfoSettings()
		{
			Version = assemblyVersion,
		});

		MSBuild(solutionPath, configurator => configurator
			.SetConfiguration(configuration)
			.SetVerbosity(Verbosity.Quiet)
			.SetMSBuildPlatform(MSBuildPlatform.Automatic)
			.SetPlatformTarget(PlatformTarget.MSIL)
			.WithTarget("Build"));

		CreateAssemblyInfo(assemblyVersionFile, new AssemblyInfoSettings()
		{
			Version = GetAssemblyVersion(assemblyVersion, "*"),
		});
	});

Task("Sign")
	.IsDependentOn("Build")
	.Does(() =>
	{
		if (settings.CodeSigning.DoCodeSigning && configuration.Equals("Release"))
		{
			InitializeCodeSigningCertificateToken(new ISI.Cake.Addin.CodeSigning.InitializeCodeSigningCertificateTokenRequest()
			{
				CodeSigningCertificateTokenCertificateFileName = settings.CodeSigning.Token.CertificateFileName,
				CodeSigningCertificateTokenCryptographicProvider = settings.CodeSigning.Token.CryptographicProvider,
				CodeSigningCertificateTokenContainerName = settings.CodeSigning.Token.ContainerName,
				CodeSigningCertificateTokenPassword = settings.CodeSigning.Token.Password,
			});

			var files = GetFiles("./**/bin/" + configuration + "/**/ISI.Cake*.dll");
			Sign(files, new SignToolSignSettings()
			{
				TimeStampDigestAlgorithm = SignToolDigestAlgorithm.Sha256,
				TimeStampUri = GetNullableUri(settings.CodeSigning.TimeStampUrl),
				CertThumbprint = settings.CodeSigning.CertificateFingerprint,
				CertPath = GetNullableFile(settings.CodeSigning.CertificateFileName),
				Password = settings.CodeSigning.CertificatePassword,
				DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
			});
		}
	});

Task("Nuget")
	.IsDependentOn("Sign")
	.Does(() =>
	{
		foreach(var project in solution.Projects.Where(project => project.Path.FullPath.EndsWith(".csproj")))
		{
			Information(project.Name);

			var nuspec = GenerateNuspecFromProject(new ISI.Cake.Addin.Nuget.GenerateNuspecFromProjectRequest()
			{
				ProjectFullName = project.Path.FullPath,
			}).Nuspec;
			nuspec.Version = assemblyVersion;
			nuspec.IconUri = GetNullableUri(@"https://github.com/ISI-Extensions/ISI.Cake.Addin/Lantern.png");
			nuspec.ProjectUri = GetNullableUri(@"https://github.com/ISI-Extensions/ISI.Cake.Addin");
			nuspec.Title = project.Name;
			nuspec.Description = project.Name;
			nuspec.Copyright = string.Format("Copyright (c) {0}, Integrated Solutions, Inc.", DateTime.Now.Year);
			nuspec.Authors = new [] { "Integrated Solutions, Inc." };
			nuspec.Owners = new [] { "Integrated Solutions, Inc." };

			var nuspecFile = File(project.Path.GetDirectory() + "/" + project.Name + ".nuspec");

			CreateNuspecFile(new ISI.Cake.Addin.Nuget.CreateNuspecFileRequest()
			{
				Nuspec = nuspec,
				NuspecFullName = nuspecFile.Path.FullPath,
			});

			NuGetPack(project.Path.FullPath, new NuGetPackSettings()
			{
				Id = project.Name,
				Version = assemblyVersion, 
				Verbosity = NuGetVerbosity.Detailed,
				Properties = new Dictionary<string, string>
				{
					{ "Configuration", configuration }
				},
				NoPackageAnalysis = false,
				Symbols = false,
				OutputDirectory = nugetDirectory,
			});

			DeleteFile(nuspecFile);

			var nupgkFile = File(nugetDirectory + "/" + project.Name + "." + assemblyVersion + ".nupkg");

			if(settings.CodeSigning.DoCodeSigning)
			{
				NupkgSign(new ISI.Cake.Addin.Nuget.NupkgSignRequest()
				{
					NupkgFullNames = new [] { nupgkFile.Path.FullPath },
					TimeStampDigestAlgorithm = SignToolDigestAlgorithm.Sha256,
					TimeStampUri = GetNullableUri(settings.CodeSigning.TimeStampUrl),
					CertificateFingerprint = settings.CodeSigning.CertificateFingerprint,
					CertificatePath = GetNullableFile(settings.CodeSigning.CertificateFileName),
					CertificatePassword = settings.CodeSigning.CertificatePassword,
					DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
				});
			}

			NupkgPush(new ISI.Cake.Addin.Nuget.NupkgPushRequest()
			{
				NupkgFullNames = new [] { nupgkFile.Path.FullPath },
				ApiKey = settings.Nuget.ApiKey,
				RepositoryName = settings.Nuget.RepositoryName,
				RepositoryUri = GetNullableUri(settings.Nuget.RepositoryUrl),
				PackageChunksRepositoryUri = GetNullableUri(settings.Nuget.PackageChunksRepositoryUrl),
			});
		}
	});

Task("Default")
	.IsDependentOn("Nuget")
	.Does(() => 
	{
		Information("No target provided. Starting default task");
	});

using(GetSolutionLock())
{
	RunTarget(target);
}
