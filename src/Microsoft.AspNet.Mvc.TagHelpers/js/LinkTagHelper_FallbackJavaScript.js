(function (cssTestPropertyName, cssTestPropertyValue, fallbackHref) {
    // This function finds the previous element (assumed to be meta) and tests it current CSS style using the passed
    // values, to determine if a stylesheet was loaded. If not, it loads the fallback stylesheet via document.write.
    var scriptElements = document.getElementsByTagName("SCRIPT"),
        meta = scriptElements[scriptElements.length - 1].previousElementSibling;

    if (document.defaultView.getComputedStyle(meta)[cssTestPropertyName] !== cssTestPropertyValue) {
        document.write('\u003clink rel="stylesheet" href="' + fallbackHref + '"/\u003e');
    }
})("[[[0]]]", "[[[1]]]", "[[[2]]]");