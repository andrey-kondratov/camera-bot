#addin nuget:?package=Cake.Docker&version=0.11.0

var target = Argument("target", "Default");
var tag = Argument("tag", "latest");
var image = Argument("image", "camera-bot");
var registry = Argument("registry", "andeadlier");

// General
Task("Default")
    .IsDependentOn("Build");

// Build
var dockerComposeBuildSettings = new DockerComposeBuildSettings
{
    ProjectDirectory = "./src"
};

Task("Build")
    .IsDependentOn("BuildAmd64")
    .IsDependentOn("BuildArm32");

Task("BuildAmd64")
    .Does(() => 
    {
        var settings = new DockerComposeBuildSettings
        {
            Files = new [] {"./src/docker-compose.yml"}
        };

        DockerComposeBuild(settings);
    });

Task("BuildArm32")
    .Does(() => 
    {
        var settings = new DockerComposeBuildSettings
        {
            Files = new [] {"./src/docker-compose.arm32.yml"}
        };

        DockerComposeBuild(settings);
    });

// Tag
string amd64ImageReference = $"{image}:{tag}";
string amd64RegistryReference = $"{registry}/{image}:{tag}";
string arm32ImageReference = $"{image}:{tag}-arm32v7";
string arm32RegistryReference = $"{registry}/{image}:{tag}-arm32v7";

Task("TagAmd64")
    .IsDependentOn("BuildAmd64")
    .Does(() => DockerTag(amd64ImageReference, amd64RegistryReference));

Task("TagArm32")
    .IsDependentOn("BuildArm32")
    .Does(() => DockerTag(arm32ImageReference, arm32RegistryReference));

// Push
Task("Push")
    .IsDependentOn("PushAmd64")
    .IsDependentOn("PushArm32");

Task("PushAmd64")
    .IsDependentOn("TagAmd64")
    .Does(() => DockerPush(amd64RegistryReference));

Task("PushArm32")
    .IsDependentOn("TagArm32")
    .Does(() => DockerPush(arm32RegistryReference));

RunTarget(target);