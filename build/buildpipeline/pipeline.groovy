import org.dotnet.ci.pipelines.Pipeline
load "build/buildpipeline/configuration.groovy"

def windowsPipeline = Pipeline.createPipeline(this, 'build/buildpipeline/windows.groovy')
def linuxPipeline = Pipeline.createPipeline(this, 'build/buildpipeline/linux.groovy')
def osxPipeline = Pipeline.createPipeline(this, 'build/buildpipeline/osx.groovy')
def aspnetConfig = new AspNetConfiguration()
String configuration = 'Release'
def parameters = [ 'Configuration': configuration]

windowsPipeline.triggerPipelineOnEveryGithubPR("Windows ${configuration} x64 Build", [ 'config': aspnetConfig, configuration: 'Release'])
windowsPipeline.triggerPipelineOnGithubPush([ 'config': aspnetConfig, configuration: 'Release'])

linuxPipeline.triggerPipelineOnEveryGithubPR("Ubuntu 16.04 ${configuration} Build", parameters)
linuxPipeline.triggerPipelineOnGithubPush(parameters)

osxPipeline.triggerPipelineOnEveryGithubPR("OSX 10.12 ${configuration} Build", parameters)
osxPipeline.triggerPipelineOnGithubPush(parameters)
