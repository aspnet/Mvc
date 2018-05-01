class AspNetConfiguration {
    def defaultConfigurations = [ 'Release']
    def defaultParameters = [ 'Configuration': 'Release' ]

    def defaultWindowsImage = [ image: 'Windows.10.Enterprise.RS3.ASPNET' ]
    def defaultMacOSImage = [ image: 'OSX10.12', version: 'latest' ]
    def defaultLinuxImage = [ image: 'Ubuntu16.04', version: 'latest-or-auto-docker' ]

    def toEnvKeyValue(Map map) {
        return map.collect { /$map.key=$map.value/ } join "&&"
    }

    def getDefaultEnvironmentVariables() {
        def logFolder = getLogFolder()
        
        return [
            "ASPNETCORE_TEST_LOG_DIR=${WORKSPACE}\\${logFolder}"
        ]
    }

    def getDefaultWindowsBuildCommand() {
        // def environmentVariables = toEnvKeyValue(environmentVariables)    
        return ".\\run.cmd -CI default-build"
    }

    def getDefaultMacOSBuildCommand(Map<String,Object> environmentVariables) {
        def environmentVariables = toEnvKeyValue(environmentVariables)    
        return "${environmentVariables}&&./build.sh --ci"
    }

    def getDefaultLinuxBuildCommand(Map<String,Object> environmentVariables) {
        def environmentVariables = toEnvKeyValue(environmentVariables)    
        return "${environmentVariables}&&./build.sh --ci"
    }
}


return this
