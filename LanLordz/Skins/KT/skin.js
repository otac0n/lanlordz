$(window).load(function() {
    // Change the Tournament View
    if (typeof (window.SetTournamentTextStyle) != "undefined") {
        window.SetTournamentTextStyle("font-size:8pt;fill:black;");
    }
    if (typeof (window.SetTournamentTextTransform) != "undefined") {
        window.SetTournamentTextTransform("matrix(1 0 0 1 0 10)");
    }
    if (typeof (window.SetTournamentBackgroundColor) != "undefined") {
    }
    if (typeof (window.SetTournamentAccentStyle) != "undefined") {
        window.SetTournamentAccentStyle("stroke:rgb(136,68,136);stroke-width:1;");
    }
});