(function (cssTestPropertyName, cssTestPropertyValue, fallbackHref) {
    // This function finds the previous element (assumed to be meta) and tests its current CSS style using the passed
    // values, to determine if a stylesheet was loaded. If not, this function loads the fallback stylesheet via
    // document.write.
    var doc = document,
        scriptElements = doc.getElementsByTagName("SCRIPT"),
        meta = scriptElements[scriptElements.length - 1].previousElementSibling,
        i;

    if (doc.defaultView.getComputedStyle(meta)[cssTestPropertyName] !== cssTestPropertyValue) {
        if (Object.prototype.toString.call(fallbackHref) === "[object Array]") {
            for (i = 0; i < fallbackHref.length; i++) {
                doc.write('<link rel="stylesheet" href="' + fallbackHref[i] + '"/>');
            }
        } else {
            doc.write('<link rel="stylesheet" href="' + fallbackHref + '"/>');
        }
    }
})("[[[0]]]", "[[[1]]]", [[[2]]]);