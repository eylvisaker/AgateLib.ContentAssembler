using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Set during automated build to write the version.")]
    readonly bool StampVersion;

    [Parameter("Sets the third number in the version 1.2.X.4")]
    readonly string BuildNumber = "0";

    [Parameter("The NuGet API key for publishing")]
    readonly string NugetApiKey = "";

    [Parameter] 
    readonly string NugetApiUrl = "https://api.nuget.org/v3/index.json"; //default
    
    [Parameter("Overrides the branch name from git.")]
    readonly string BranchName;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    [PathExecutable(@"powershell.exe")]
    readonly Tool Powershell;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetVersion(Version)
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Console.WriteLine("Setting version to " + Version);
            DotNetBuild(s => s
                .SetVersion(Version)
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .EnableNoRestore());
        });

    private string Version
    {
        get
        {
            string version = System.IO.File.ReadAllText("version.info");
            version += "." + BuildNumber.ToString();

            if (!string.IsNullOrWhiteSpace(BranchName) && BranchName != "main")
            {
                version += "-" + BranchName.Replace("/", "-");
            }

            return version;
        }
    }

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .SetLogger("nunit;LogFilePath=" + ArtifactsDirectory / $"tests/UnitTests-{Version}.xml")
                .SetProjectFile(Solution));
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetVersion(Version)
                .SetConfiguration(Configuration)
                .SetProject("src/AgateLib.ContentModel"));

            DotNetPack(s => s
                .SetVersion(Version)
                .SetConfiguration(Configuration)
                .SetProject("src/AgateLib.ContentAssembler"));
                
            DotNetPack(s => s
                .SetVersion(Version)
                .SetConfiguration(Configuration)
                .SetProject("src/AgateLib.ContentAssembler.Task"));
        });

    Target IntegrationTest => _ => _
        .DependsOn(Pack)
        .Executes(() => 
        {
            Powershell("./BuildTestApp.ps1",
                       "tests/PackageTest");
        });

    Target Publish => _ => _
        .DependsOn(IntegrationTest)
        .Executes(() => 
        {
            GlobFiles(ArtifactsDirectory, "**/*.nupkg")
               .NotEmpty()
               .ForEach(x =>
               {
                   if (BranchName != "main" && BranchName != "devel") 
                   {
                       Console.WriteLine($"File {x} is not designated for pushing as release or prerelease Nuget package.");
                       return;
                   }

                   DotNetNuGetPush(s => s
                       .SetTargetPath(x)
                       .SetSource(NugetApiUrl)
                       .SetApiKey(NugetApiKey)
                   );
               });
        });
        
}
