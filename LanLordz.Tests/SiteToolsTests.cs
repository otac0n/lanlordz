using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using LanLordz.SiteTools;
using System.Xml.Linq;
using System.Xml;

namespace LanLordzTests
{
    [TestFixture]
    public class SiteToolsTests
    {
        [Test]
        public void AcceptParserFollowsClentPreferenceOrder()
        {
            string[] supported = new string[] { "gzip", "deflate" };

            Assert.That((new AcceptList("gzip,deflate", supported)).First(), Is.EqualTo("gzip"));
            Assert.That((new AcceptList("deflate,gzip", supported)).First(), Is.EqualTo("deflate"));

            supported = new string[] { "deflate", "gzip" };

            Assert.That((new AcceptList("gzip,deflate", supported)).First(), Is.EqualTo("gzip"));
            Assert.That((new AcceptList("deflate,gzip", supported)).First(), Is.EqualTo("deflate"));
        }

        [Test]
        public void AcceptParserIgnoresQValuesOfZero()
        {
            string[] supported = new string[] { "gzip", "deflate" };

            Assert.That((new AcceptList("gzip;q=0,deflate", supported)).First(), Is.EqualTo("deflate"));
            Assert.That((new AcceptList("gzip,deflate;q=0", supported)).First(), Is.EqualTo("gzip"));
        }
        
        [Test]
        public void AcceptParserFollowsQValueOrders()
        {
            string[] supported = new string[] { "gzip", "deflate" };

            Assert.That((new AcceptList("gzip;q=.5,deflate", supported)).First(), Is.EqualTo("deflate"));
            Assert.That((new AcceptList("deflate;q=0.5,gzip;q=0.5,identity", supported)).First(), Is.EqualTo("identity"));
        }

        [Test]
        public void AcceptParserFollowsRequesterOrderOnStar()
        {
            string[] supported = new string[] { "gzip", "deflate" };

            Assert.That((new AcceptList("*", supported)).First(), Is.EqualTo("gzip"));

            supported = new string[] { "deflate", "gzip" };

            Assert.That((new AcceptList("*", supported)).First(), Is.EqualTo("deflate"));
        }

        [Test]
        public void PagerGenoratorReturnsExpectedResults()
        {
            XDocument config1 = XDocument.Parse(@"<?xml version=""1.0""?>
<PagerStyle>
	<First>
        <Format>&lt;a class=&quot;PagerLink&quot; href=&quot;{0}""&gt;&amp;lt;&amp;lt;&lt;/a&gt; </Format>
    </First>
	<Previous>
        <Format>&lt;a class=&quot;PagerLink&quot; href=&quot;{0}""&gt;&amp;lt;&lt;/a&gt; </Format>
    </Previous>
	<PageCount>
		<Format>Page {0} of {1}</Format>
	</PageCount>
	<Next>
        <Format> &lt;a class=&quot;PagerLink&quot; href=&quot;{0}""&gt;&amp;gt;&lt;/a&gt;</Format>
    </Next>
	<Last>
        <Format> &lt;a class=&quot;PagerLink&quot; href=&quot;{0}""&gt;&amp;gt;&amp;gt;&lt;/a&gt;</Format>
    </Last>
</PagerStyle>");

            XDocument config2 = XDocument.Parse(@"<PagerStyle>
	<Literal>Jump&amp;nbsp;to&amp;nbsp;page:&amp;nbsp;</Literal>
	<First>
		<Format>&lt;a href=&quot;{0}&quot;&gt;&amp;lt;&lt;/a&gt;&amp;nbsp;</Format>
	</First>
	<NumberRun collapseAdjacentSeperators=""False"">
	    <Regions>
			<First number=""0""/>
			<FirstSeperator>...</FirstSeperator>
			<Current adjacentMin=""3"" adjacentMax=""6"" />
			<LastSeperator>...</LastSeperator>
			<Last number=""0""/>
	    </Regions>
		<Format>
			<Current>{0}</Current>
			<Other>&lt;a href=&quot;{1}&quot;&gt;{0}&lt;/a&gt;</Other>
			<Seperator>&amp;nbsp;</Seperator>
		</Format>
	</NumberRun>
	<Last>
		<Format>&amp;nbsp;&lt;a href=&quot;{0}&quot;&gt;&amp;gt;&lt;/a&gt;</Format>
	</Last>
</PagerStyle>");

            XDocument config3 = XDocument.Parse(@"<?xml version=""1.0""?>
<PagerStyle>
	<ItemCount>
		<Format><![CDATA[{0} topics]]></Format>
	</ItemCount>
	<Literal><![CDATA[ &bull; ]]></Literal>
	<PageCount>
		<Format><![CDATA[Page <strong>{0}</strong> of <strong>{1}</strong>]]></Format>
	</PageCount>
	<Literal><![CDATA[ &bull; ]]></Literal>
	<Literal><![CDATA[<span>]]></Literal>
	<NumberRun collapseAdjacentSeperators=""True"" collapseAdjacentElements=""True"">
	    <Regions>
			<First number=""1""/>
			<FirstSeperator><![CDATA[ ... ]]></FirstSeperator>
			<Current adjacentMin=""3"" adjacentMax=""4"" />
			<LastSeperator><![CDATA[ ... ]]></LastSeperator>
			<Last number=""1""/>
	    </Regions>
		<Format>
			<Current><![CDATA[<strong>{0}</strong>]]></Current>
			<Other><![CDATA[<a href=""{1}"">{0}</a>]]></Other>
			<Seperator><![CDATA[<span class=""page-sep"">, </span>]]></Seperator>
		</Format>
	</NumberRun>
	<Literal><![CDATA[</span>]]></Literal>
</PagerStyle>");

            string expected;
            string result;

            Pager pg1 = new Pager(config1);
            Pager pg2 = new Pager(config2);
            Pager pg3 = new Pager(config3);

            expected = "Page 1 of 10 <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=2\">&gt;</a> <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=10\">&gt;&gt;</a>"; 
            result = pg1.CreatePager(1, 236, 25, i => "/Forums/ViewForum/4?page=" + i);
            Assert.That(result, Is.EqualTo(expected));

            expected = "<a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=1\">&lt;&lt;</a> <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=1\">&lt;</a> Page 2 of 10 <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=3\">&gt;</a> <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=10\">&gt;&gt;</a>";
            result = pg1.CreatePager(2, 236, 25, i => "/Forums/ViewForum/4?page=" + i);
            Assert.That(result, Is.EqualTo(expected));

            expected = "<a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=1\">&lt;&lt;</a> <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=8\">&lt;</a> Page 9 of 10 <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=10\">&gt;</a> <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=10\">&gt;&gt;</a>";
            result = pg1.CreatePager(9, 236, 25, i => "/Forums/ViewForum/4?page=" + i);
            Assert.That(result, Is.EqualTo(expected));

            expected = "<a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=1\">&lt;&lt;</a> <a class=\"PagerLink\" href=\"/Forums/ViewForum/4?page=9\">&lt;</a> Page 10 of 10";
            result = pg1.CreatePager(10, 236, 25, i => "/Forums/ViewForum/4?page=" + i);
            Assert.That(result, Is.EqualTo(expected));

            Console.WriteLine(@"<style>
a:link	{ color: #898989; text-decoration: none; }
a:visited	{ color: #898989; text-decoration: none; }
a:hover	{ color: #d3d3d3; text-decoration: underline; }
a:active	{ color: #d2d2d2; text-decoration: none; }

.pagination {
	height: 1%; /* IE tweak (holly hack) */
	width: auto;
	text-align: right;
	margin-top: 5px;
	float: right;
}

.pagination span.page-sep {
	display: none;
}

li.pagination {
	margin-top: 0;
}

.pagination strong, .pagination b {
	font-weight: normal;
}

.pagination span strong {
	padding: 0 2px;
	margin: 0 2px;
	font-weight: normal;
	color: #FFFFFF;
	background-color: #bfbfbf;
	border: 1px solid #bfbfbf;
	font-size: 0.9em;
}

.pagination span a, .pagination span a:link, .pagination span a:visited, .pagination span a:active {
	font-weight: normal;
	text-decoration: none;
	color: #747474;
	margin: 0 2px;
	padding: 0 2px;
	background-color: #eeeeee;
	border: 1px solid #bababa;
	font-size: 0.9em;
	line-height: 1.5em;
}

.pagination span a:hover {
	border-color: #d2d2d2;
	background-color: #d2d2d2;
	color: #FFF;
	text-decoration: none;
}

.pagination img {
	vertical-align: middle;
}
</style>");

            Console.WriteLine(pg2.CreatePager(1, 2000, 2, i => "/?page=" + i));
            Console.WriteLine("<br/>");
            Console.WriteLine(pg2.CreatePager(3, 2000, 2, i => "/?page=" + i));
            Console.WriteLine("<br/>");
            Console.WriteLine(pg2.CreatePager(7, 2000, 2, i => "/?page=" + i));
            Console.WriteLine("<br/>");
            Console.WriteLine(pg2.CreatePager(336, 2000, 2, i => "/?page=" + i));
            Console.WriteLine("<br/>");
            Console.WriteLine(pg2.CreatePager(995, 2000, 2, i => "/?page=" + i));
            Console.WriteLine("<br/>");
            Console.WriteLine(pg2.CreatePager(998, 2000, 2, i => "/?page=" + i));
            Console.WriteLine("<br/>");
            Console.WriteLine(pg2.CreatePager(1000, 2000, 2, i => "/?page=" + i));

            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");

            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(1, 73217, 50, i => "/?page=" + i) + "</div>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(5, 73217, 50, i => "/?page=" + i) + "</div>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(7, 73217, 50, i => "/?page=" + i) + "</div>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(900, 73217, 50, i => "/?page=" + i) + "</div>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(1460, 73217, 50, i => "/?page=" + i) + "</div>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(1464, 73217, 50, i => "/?page=" + i) + "</div>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<br/>");
            Console.WriteLine("<div class=\"pagination\">" + pg3.CreatePager(1465, 73217, 50, i => "/?page=" + i) + "</div>");
        }
    }
}
