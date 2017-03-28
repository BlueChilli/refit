using System.Threading;
using System.Text.RegularExpressions;

//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////

#addin "Cake.FileHelpers"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "GitLink"
#tool "nuget:?package=xunit.runner.console"
using Cake.Common.Build.TeamCity;


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Should MSBuild & GitLink treat any errors as warnings?
var treatWarningsAsErrors = false;
Func<string, int> GetEnvironmentInteger = name => {
	
	var data = EnvironmentVariable(name);
	int d = 0;
	if(!String.IsNullOrEmpty(data) && int.TryParse(data, out d)) 
	{
		return d;
	} 
	
	return 0;

};
// Build configuration
var local = BuildSystem.IsLocalBuild;
var isTeamCity = BuildSystem.TeamCity.IsRunningOnTeamCity;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var teamCity = BuildSystem.TeamCity;
var branch = EnvironmentVariable("Git_Branch");
var isPullRequest = !String.IsNullOrEmpty(branch) && branch.ToUpper().Contains("PULL-REQUEST"); //teamCity.Environment.PullRequest.IsPullRequest;
var projectName =  EnvironmentVariable("TEAMCITY_PROJECT_NAME"); //  teamCity.Environment.Project.Name;
var isRepository = StringComparer.OrdinalIgnoreCase.Equals("refit", projectName);
var isTagged = !String.IsNullOrEmpty(branch) && branch.ToUpper().Contains("TAGS");
var buildConfName = EnvironmentVariable("TEAMCITY_BUILDCONF_NAME"); //teamCity.Environment.Build.BuildConfName
var buildNumber = GetEnvironmentInteger("BUILD_NUMBER");
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", buildConfName);

var githubOwner = "paulcbetts";
var githubRepository = "refit";
var githubUrl = string.Format("https://github.com/{0}/{1}", githubOwner, githubRepository);
var licenceUrl = string.Format("{0}/blob/master/LICENSE", githubUrl);

// Version
var version = "3.0.2";
var majorMinorPatch = version;
var semVersion = version;
var informationalVersion = version;
var nugetVersion = version;
var buildVersion = version;

// Artifacts
var artifactDirectory = "./artifacts/";
var packageWhitelist = new[] { "refit"};

var buildSolution = "./Refit.sln";

// Macros
Func<string> GetMSBuildLoggerArguments = () => {
    return BuildSystem.TeamCity.IsRunningOnTeamCity ? EnvironmentVariable("MsBuildLogger"): null;
};


Action Abort = () => { throw new Exception("A non-recoverable fatal error occurred."); };
Action<string> TestFailuresAbort = testResult => { throw new Exception(testResult); };
Action NonMacOSAbort = () => { throw new Exception("Running on platforms other macOS is not supported."); };


Action<string> RestorePackages = (solution) =>
{
    NuGetRestore(solution);
};


Action<string, string> Package = (nuspec, basePath) =>
{
    CreateDirectory(artifactDirectory);

    Information("Packaging {0} using {1} as the BasePath.", nuspec, basePath);

    NuGetPack(nuspec, new NuGetPackSettings {
        RequireLicenseAcceptance = false,
        Version                  = nugetVersion,
        ReleaseNotes             = new [] { string.Format("{0}/releases", githubUrl) },
        Verbosity                = NuGetVerbosity.Detailed,
        OutputDirectory          = artifactDirectory,
        BasePath                 = basePath
    });
};

Action<string> SourceLink = (solutionFileName) =>
{
    GitLink("./", new GitLinkSettings() {
        RepositoryUrl = githubUrl,
        SolutionFileName = solutionFileName,
        ErrorsAsWarnings = true
    });
};

Action<string, string, Exception> WriteErrorLog = (message, identity, ex) => 
{
	if(isTeamCity) 
	{
		teamCity.BuildProblem(message, identity);
		teamCity.WriteStatus(String.Format("{0}", identity), "ERROR", ex.ToString());
	}
	else {
		throw new Exception(String.Format("task {0} - {1}", identity, message), ex);
	}
};


Func<string, IDisposable> BuildBlock = message => {

	if(BuildSystem.TeamCity.IsRunningOnTeamCity) 
	{
		return BuildSystem.TeamCity.BuildBlock(message);
	}
	
	return null;
	
};

Func<string, IDisposable> Block = message => {

	if(BuildSystem.TeamCity.IsRunningOnTeamCity) 
	{
		BuildSystem.TeamCity.Block(message);
	}

	return null;
};

Action<string,string> build = (solution, configuration) =>
{
    Information("Building {0}", solution);
	using(BuildBlock("Build")) 
	{			
        if(isRunningOnUnix) {
        	
			XBuild(solution, settings => {
				settings
				.SetConfiguration(configuration)
				.WithProperty("NoWarn", "1591") // ignore missing XML doc warnings
				.WithProperty("TreatWarningsAsErrors", treatWarningsAsErrors.ToString())
				.SetVerbosity(Verbosity.Minimal);

				var msBuildLogger = GetMSBuildLoggerArguments();

				if(!string.IsNullOrEmpty(msBuildLogger)) 
				{
					settings.ArgumentCustomization = arguments => arguments.Append(string.Format(" /logger:{0}", msBuildLogger));
				}
			});
        }
        else {

        	// UWP (project.json) needs to be restored before it will build.
  			RestorePackages(solution);

        	MSBuild(solution, settings => {
				settings
				.SetConfiguration(configuration)
				.WithProperty("NoWarn", "1591") // ignore missing XML doc warnings
				.WithProperty("TreatWarningsAsErrors", treatWarningsAsErrors.ToString())
				.SetVerbosity(Verbosity.Minimal)
				.SetNodeReuse(false);

				var msBuildLogger = GetMSBuildLoggerArguments();
			
				if(!string.IsNullOrEmpty(msBuildLogger)) 
				{
					Information("Using custom MSBuild logger: {0}", msBuildLogger);
					settings.ArgumentCustomization = arguments =>
					arguments.Append(string.Format("/logger:{0}", msBuildLogger));
				}
			});

			// seems like it doesn't work for ios
			using(Block("SourceLink")) 
			{
			    SourceLink(solution);
			};
        }

		
    };		

};

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup((context) =>
{
    Information("Building {0} (isTagged: {1})", informationalVersion, isTagged);

		if (isTeamCity)
		{
			Information(
					@"Environment:
					 PullRequest: {0}
					 Build Configuration Name: {1}
					 TeamCity Project Name: {2}
					 Branch: {3}",
					 isPullRequest,
					 buildConfName,
					 projectName,
					 branch
					);
        }
        else
        {
             Information("Not running on TeamCity");
        }
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("RestorePackages")
    .IsDependentOn("UpdateAssemblyInfo")
    .Does (() =>
{
    build(buildSolution, "Release");
})
.OnError(exception => {
	WriteErrorLog("Build failed", "Build", exception);
});


Task("UpdateAssemblyInfo")
    .Does (() =>
{
    var file = "./CommonAssemblyInfo.cs";

	using(BuildBlock("UpdateAssemblyInfo")) 
	{
		CreateAssemblyInfo(file, new AssemblyInfoSettings {
			Version = majorMinorPatch,
			FileVersion = majorMinorPatch,
			InformationalVersion = informationalVersion
		});
	};
   
})
.OnError(exception => {
	WriteErrorLog("updating assembly info failed", "UpdateAssemblyInfo", exception);
});

Task("RestorePackages")
.Does (() =>
{
    Information("Restoring Packages for {0}", buildSolution);
	using(BuildBlock("RestorePackages")) 
	{
	    RestorePackages(buildSolution);
	};
})
.OnError(exception => {
	WriteErrorLog("restoring packages failed", "RestorePackages", exception);
});



Task("RunUnitTests")
    .IsDependentOn("RestorePackages")
    .IsDependentOn("Build")
    .Does(() =>
{
	Information("Running Unit Tests for {0}", buildSolution);
	using(BuildBlock("RunUnitTests")) 
	{
		XUnit2("./System.Net.Http.Formatting.Test/bin/Release/System.Net.Http.Formatting.Test.dll", new XUnit2Settings {
			OutputDirectory = artifactDirectory,
            XmlReportV1 = false,
            NoAppDomain = true
		});

		XUnit2("./Refit-Tests/bin/Release/Refit.Tests.dll", new XUnit2Settings {
			OutputDirectory = artifactDirectory,
            XmlReportV1 = false,
            NoAppDomain = true
		});


	};
});


Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .Does (() =>
{
	using(BuildBlock("Package")) 
	{
		foreach(var package in packageWhitelist)
		{
			// only push the package which was created during this build run.
			var packagePath = string.Format("./Refit/{0}.nuspec", package);

			// Push the package.
			Package(packagePath, "./Refit");
		}
	};

    
})
.OnError(exception => {
	WriteErrorLog("Generating packages failed", "Package", exception);
});


Task("PublishPackages")
	.IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRepository)
    .Does (() =>
{
	using(BuildBlock("Package"))
	{
		string apiKey;
		string source;

		if (isReleaseBranch && !isTagged)
		{
			// Resolve the API key.
			apiKey = EnvironmentVariable("MYGET_APIKEY");
			if (string.IsNullOrEmpty(apiKey))
			{
				throw new Exception("The MYGET_APIKEY environment variable is not defined.");
			}

			source = EnvironmentVariable("MYGET_SOURCE");
			if (string.IsNullOrEmpty(source))
			{
				throw new Exception("The MYGET_SOURCE environment variable is not defined.");
			}
		}
		else 
		{
			// Resolve the API key.
			apiKey = EnvironmentVariable("NUGET_APIKEY");
			if (string.IsNullOrEmpty(apiKey))
			{
				throw new Exception("The NUGET_APIKEY environment variable is not defined.");
			}

			source = EnvironmentVariable("NUGET_SOURCE");
			if (string.IsNullOrEmpty(source))
			{
				throw new Exception("The NUGET_SOURCE environment variable is not defined.");
			}
		}



		// only push whitelisted packages.
		foreach(var package in packageWhitelist)
		{
			// only push the package which was created during this build run.
			var packagePath = artifactDirectory + File(string.Concat(package, ".", nugetVersion, ".nupkg"));

			// Push the package.
			NuGetPush(packagePath, new NuGetPushSettings {
				Source = source,
				ApiKey = apiKey
			});
		}

	};

  
})
.OnError(exception => {
	WriteErrorLog("publishing packages failed", "PublishPackages", exception);
});




//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("PublishPackages")
    .Does (() =>
{

});


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
