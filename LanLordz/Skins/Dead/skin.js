$(window).load(function(){if(typeof(window.SetTournamentTextStyle)!="undefined"){window.SetTournamentTextStyle("font-size:8pt;fill:white;");}
if(typeof(window.SetTournamentTextTransform)!="undefined"){window.SetTournamentTextTransform("matrix(1 0 0 1 0 10)");}
if(typeof(window.SetTournamentTextContrastStyle)!="undefined"){window.SetTournamentTextContrastStyle("fill:rgb(50,50,50);");}
if(typeof(window.SetTournamentAccentStyle)!="undefined"){window.SetTournamentAccentStyle("stroke:red;stroke-width:1;");}
if(typeof(window.SetTournamentBackgroundColor)!="undefined"){}});$(document).ready(function(){$(".FooterVisitors").each(function(){this.innerHTML=this.innerHTML.replace('Visitors','Survivors');});$("#main").append("<div style=\"clear : both;\"></div>");})