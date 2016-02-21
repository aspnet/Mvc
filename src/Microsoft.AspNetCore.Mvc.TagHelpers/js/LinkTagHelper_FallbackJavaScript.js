﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
        // Find the last script tag on the page which will be this one, as JS executes as it loads
        scriptElements = doc.getElementsByTagName("SCRIPT"),
        // Find the meta tag before this script tag, that's the element we're going to test the CSS property on
        meta = scriptElements[scriptElements.length - 1].previousElementSibling,
        // Get the current style of the meta tag starting with standards-based API and falling back to <=IE8 API
        metaStyle = (doc.defaultView && doc.defaultView.getComputedStyle) ? doc.defaultView.getComputedStyle(meta)
            : meta.currentStyle,
        i;

    if (metaStyle && metaStyle[cssTestPropertyName] !== cssTestPropertyValue) {
        for (i = 0; i < fallbackHref.length; i++) {
            doc.write('<link rel="stylesheet" href="' + fallbackHref[i] + '"/>');
        }
    }
})();