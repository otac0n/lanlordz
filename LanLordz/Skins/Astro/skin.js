$(window).load(function() {
    // Change the Tournament View
    if (typeof (window.SetTournamentTextStyle) != "undefined") {
        window.SetTournamentTextStyle("font-size:8pt;fill:white;");
    }
    if (typeof (window.SetTournamentTextTransform) != "undefined") {
        window.SetTournamentTextTransform("matrix(1 0 0 1 0 10)");
    }
    if (typeof (window.SetTournamentTextContrastStyle) != "undefined") {
        window.SetTournamentTextContrastStyle("fill:rgb(41,41,255);opacity:0.1875;stroke:white;stroke-width:1;");
    }
    if (typeof (window.SetTournamentAccentStyle) != "undefined") {
        window.SetTournamentAccentStyle("stroke:white;stroke-width:1;");
    }
    if (typeof (window.SetTournamentBackgroundColor) != "undefined") {
    }
});

$(document).ready(function() {
    $(".Thread:even").addClass("ThreadEven");
    $(".Post:even").addClass("PostEven");
    $("#main").append("<div style=\"clear : both;\"></div>");
})