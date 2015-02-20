// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

var MVC = (function () {
    // Takes the data which needs to be converted to MVC understandable format.
    var _stringify = function (data) {
        // This holds the stringified result.
        var result = "";
        for (var element in data) {
            result += process(element, data[element]);
        }

        // An '&' is appended at the end. Removing it.
        return result.slice(0, -1);
    }

    function process(key, value, prefix) {
        // Ignore functions.
        if (typeof value === "function") {
            return;
        }

        if (Object.prototype.toString.call(value) === '[object Array]') {
            var result = "";
            for (var i = 0; i < value.length; i++) {
                var tempPrefix = (prefix == null ? key : prefix) + "[" + i + "]";
                result += process(key, value[i], tempPrefix);
            }

            return result;
        }
        else if (typeof value === "object") {
            var result = "";
            for (var prop in value) {
                // This is to prevent looping through inherited proeprties.
                if (value.hasOwnProperty(prop)) {
                    var tempPrefix = (prefix == null ? key : prefix) + "." + prop;
                    result += process(prop, value[prop], tempPrefix);
                }
            }

            return result;
        }
        else {
            return encodeURIComponent(prefix == null ? key : prefix) + "=" + encodeURIComponent(value) + "&";
        }
    }

    return {
        // Converts a Json object into MVC understandable format
        // when submitted as form-url-encoded data.
        stringify: _stringify
    };
})()