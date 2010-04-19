$(window).load(function() {
    // Change the Tournament View
    if (typeof (window.SetTournamentTextStyle) != "undefined") {
        window.SetTournamentTextStyle("font-size:8pt;fill:white;");
    }
    if (typeof (window.SetTournamentTextTransform) != "undefined") {
        window.SetTournamentTextTransform("matrix(1 0 0 1 0 10)");
    }
    if (typeof (window.SetTournamentTextContrastStyle) != "undefined") {
        window.SetTournamentTextContrastStyle("fill:rgb(30,30,30);");
    }
    if (typeof (window.SetTournamentAccentStyle) != "undefined") {
        window.SetTournamentAccentStyle("stroke:white;stroke-width:1;");
    }
    if (typeof (window.SetTournamentBackgroundColor) != "undefined") {
    }
});