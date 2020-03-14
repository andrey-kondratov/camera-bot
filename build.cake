var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nuGetApiKey = Argument("nuGetApiKey", "");
var outputDirectory = "./out";

// General
Task("Default")
    .IsDependentOn("Pack");

Task("Clean")
    .Does(() => 
    {
        EnsureDirectoryExists(outputDirectory);
        CleanDirectory(outputDirectory);
    });

// Pack
var packSettings = new DotNetCorePackSettings
{
    Configuration = configuration,
    OutputDirectory = outputDirectory
};

Task("Pack")
    .IsDependentOn("Clean")
    .DoesForEach(GetFiles("**/*.csproj"), file => 
    {
        DotNetCorePack(file.FullPath, packSettings);
    });

// Push
var pushSettings = new DotNetCoreNuGetPushSettings
{
    Source = "https://api.nuget.org/v3/index.json",
    ApiKey = nuGetApiKey
};

Task("Push")
    .IsDependentOn("Pack")
    .DoesForEach(GetFiles(outputDirectory + "/*.nupkg"), file => 
    {
        DotNetCoreNuGetPush(file.FullPath, pushSettings);
    });

RunTarget(target);