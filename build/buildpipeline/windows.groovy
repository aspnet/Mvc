@Library('dotnet-ci') _

// 'node' indicates to Jenkins that the enclosed block runs on a node that matches
// the label 'windows-with-vs'
simpleNode(params.config.defaultWindowsImage.image, params.config.defaultWindowsImage.version) {
    stage ('Checking out source') {
        checkout scm
    }
    stage ('Build') {
        withEnv(getDefaultEnvironmentVariables()) {
            bat getDefaultWindowsBuildCommand()
        }
    }
}
