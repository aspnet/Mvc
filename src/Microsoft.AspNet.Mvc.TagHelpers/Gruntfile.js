// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

module.exports = function (grunt) {
    grunt.initConfig({
        uglify: {
            scripts: {
                files: [{
                    expand: true,
                    cwd: "js",
                    src: "**/*.js",
                    dest: "Compiler/Resources"
                }]
            }
        }
    });

    grunt.loadNpmTasks("grunt-contrib-uglify");

    grunt.registerTask("default", [ "uglify" ]);
};