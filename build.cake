//dotnet tool install Cake.Tool -g
//dotnet tool update Cake.Tool -g
#addin nuget:?package=Cake.FileHelpers
#addin nuget:?package=ISI.Cake.AddIn&loaddependencies=true

//mklink /D Secrets S:\
var settingsFullName = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("LocalAppData"), "Secrets", "ISI.keyValue");
var settings = GetSettings(settingsFullName);

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionPath = File("./src/ISI.Cake.Addin.sln");
var solution = ParseSolution(solutionPath);

var rootAssemblyVersionKey = "ISI.Cake.Addin";

var buildDateTime = DateTime.UtcNow;
var buildDateTimeStamp = GetDateTimeStamp(buildDateTime);
var buildRevision = GetBuildRevision(buildDateTime);

var assemblyVersions = GetAssemblyVersionFiles(rootAssemblyVersionKey, buildRevision);
var assemblyVersion = assemblyVersions[rootAssemblyVersionKey].AssemblyVersion;

Information("AssemblyVersion: {0}", assemblyVersion);

var nugetPackOutputDirectory = Argument("NugetPackOutputDirectory", System.IO.Path.GetFullPath("./Nuget"));

Task("Clean")
	.Does(() =>
	{
		Information("Cleaning Projects ...");

		foreach(var projectPath in new HashSet<string>(solution.Projects.Select(p => p.Path.GetDirectory().ToString())))
		{
			Information("Cleaning {0}", projectPath);
			CleanDirectories(projectPath + "/**/bin/" + configuration);
			CleanDirectories(projectPath + "/**/obj/" + configuration);
		}

		Information("Cleaning {0}", nugetPackOutputDirectory);
		CleanDirectories(nugetPackOutputDirectory);
	});

Task("NugetPackageRestore")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		Information("Restoring Nuget Packages ...");
		using(GetNugetLock())
		{
			RestoreNugetPackages(solutionFile);
		}
	});

Task("Build")
	.IsDependentOn("NugetPackageRestore")
	.Does(() => 
	{
		using(SetAssemblyVersionFiles(assemblyVersions))
		{
			DotNetBuild(solutionFile, new DotNetBuildSettings()
			{
				Configuration = configuration,
			});
		}
	});

Task("Sign")
	.IsDependentOn("Build")
	.Does(() =>
	{
		if (settings.CodeSigning.DoCodeSigning && configuration.Equals("Release"))
		{
			var files = GetFiles("./**/bin/" + configuration + "/**/ISI.Cake*.dll");

			SignAssemblies(new ISI.Cake.Addin.CodeSigning.SignAssembliesUsingSettingsRequest()
			{
				AssemblyPaths = files,
				Settings = settings,
			});
		}
	});

Task("Nuget")
	.IsDependentOn("Sign")
	.Does(() =>
	{
		var sourceControlUrl = GetSolutionSourceControlUrl();
		var nupkgFiles = new FilePathCollection();

		foreach(var project in solution.Projects.Where(project => project.Path.FullPath.EndsWith(".csproj")))
		{
			Information(project.Name);

			var nuspec = GenerateNuspecFromProject(new ISI.Cake.Addin.Nuget.GenerateNuspecFromProjectRequest()
			{
				ProjectFullName = project.Path.FullPath,
				IncludeSBom = false,
				Settings = settings,
			}).Nuspec;
			nuspec.Version = assemblyVersion;
			nuspec.ProjectUri = GetNullableUri(sourceControlUrl);
			nuspec.Title = project.Name;
			nuspec.Description = project.Name;

			var nuspecFile = File(project.Path.GetDirectory() + "/" + project.Name + ".nuspec");

			CreateNuspecFile(new ISI.Cake.Addin.Nuget.CreateNuspecFileRequest()
			{
				Nuspec = nuspec,
				NuspecFullName = nuspecFile.Path.FullPath,
			});

			NupkgPack(new ISI.Cake.Addin.Nuget.NupkgPackRequest()
			{
				NuspecFullName = nuspecFile.Path.FullPath,
				CsProjFullName = project.Path.FullPath,
				OutputDirectory = nugetPackOutputDirectory,
			});

			DeleteFile(nuspecFile);

			nupkgFiles.Add(File(nugetPackOutputDirectory + "/" + project.Name + "." + assemblyVersion + ".nupkg"));

			if(settings.CodeSigning.DoCodeSigning)
			{
				SignNupkgs(new ISI.Cake.Addin.CodeSigning.SignNupkgsUsingSettingsRequest()
				{
					NupkgPaths = nupkgFiles,
					Settings = settings,
				});
			}
		}
	});

Task("Nuget-Publish")
	.IsDependentOn("Nuget")
	.Does(() =>
	{
		var nupkgFiles = GetFiles(MakeAbsolute(Directory(nugetPackOutputDirectory)) + "/*.nupkg");

		NupkgPush(new ISI.Cake.Addin.Nuget.NupkgPushRequest()
		{
			NupkgPaths = nupkgFiles,
			NugetApiKey = settings.Nuget.ApiKey,
			RepositoryName = settings.Nuget.RepositoryName,
			RepositoryUri = GetNullableUri(settings.Nuget.RepositoryUrl),
		});
	});

Task("Default")
	.IsDependentOn("Nuget-Publish")
	.Does(() => 
	{
		Information("No target provided. Starting default task");
	});

using(GetSolutionLock())
{
	RunTarget(target);
}
