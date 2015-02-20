// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

var Mvc = (function ($) {

    // This holds the stringified result.
    var result = "";

    // Takes the data which needs to be converted to MVC understandable format.
    var _stringify = function (data) {
        result = "";
        for (var element in data) {
            Process(element, data[element], null);
        }

        // An '&' is appended at the end. Removing it.
        return result.slice(0, -1);
    }

    function Process(key, value, prefix) {
        // Ignore functions.
        if ($.isFunction(value)) {
            return;
        }

        if ($.isArray(value)) {
            for (var i = 0; i < value.length; i++) {
                var tempPrefix = (prefix == null ? key : prefix) + "[" + i + "]";
                Process(key, value[i], tempPrefix);
            }
        }
        else if ($.type(value) == "object") {
            for (var prop in value) {
                // This is to prevent looping through inherited proeprties.
                if (value.hasOwnProperty(prop)) {
                    var tempPrefix = (prefix == null ? key : prefix) + "." + prop;
                    Process(prop, value[prop], tempPrefix);
                }
            }
        }
        else {
            result += (prefix == null ? key : prefix) + "=" + value + "&";
        }
    }

    return {
        // Converts a Json object into MVC understandable format
        // when submitted as form-url-encoded data.
        stringify: _stringify
    };
})(jQuery)