(function (a, b, c) {
    var d = document,
        s = d.getElementsByTagName("SCRIPT"),
        m = s[s.length - 1].previousElementSibling;
    (d.defaultView.getComputedStyle(m)[a] === b ||
        d.write('\u003clink rel="stylesheet" href="' + c + '" /\u003e'));
})("[[[0]]]", "[[[1]]]", "[[[2]]]");