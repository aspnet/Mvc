// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
(
 /**
  * This function finds the previous element (assumed to be meta) and tests its current CSS style using the passed
  * values, to determine if a stylesheet was loaded. If not, this function loads the fallback stylesheet via
  * document.write.
  *
  * @param {string} cssTestPropertyName - The name of the CSS property to test.
  * @param {string} cssTestPropertyValue - The value to test the specified CSS property for.
  * @param {string[]} fallbackHref - The URLs to the stylesheets to load in the case the test fails.
  */
 function loadFallbackStylesheet(cssTestPropertyName, cssTestPropertyValue, fallbackHref) {
    var doc = document,
        scriptElements = doc.getElementsByTagName("SCRIPT"),
        meta = scriptElements[scriptElements.length - 1].previousElementSibling,
        i;

    if (doc.defaultView.getComputedStyle(meta)[cssTestPropertyName] !== cssTestPropertyValue) {
        for (i = 0; i < fallbackHref.length; i++) {
            doc.write('<link rel="stylesheet" href="' + fallbackHref[i] + '"/>');
        }
    }
})("[[[0]]]", "[[[1]]]", [[[2]]]);