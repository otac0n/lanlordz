//-----------------------------------------------------------------------
// <copyright file="Pager.cs" company="LAN Lordz, inc.">
//  Copyright © 2010 LAN Lordz, inc.
//
//  This file is part of The LAN Lordz LAN Party System.
//
//  The LAN Lordz LAN Party System is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  The LAN Lordz LAN Party System is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with The LAN Lordz LAN Party System.  If not, see [http://www.gnu.org/licenses/].
// </copyright>
// <author>John Gietzen</author>
//-----------------------------------------------------------------------

namespace LanLordz.SiteTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    public delegate string GeneratePageLinkDelegate(int page);

    public delegate string GenerateItemDelegate(int page);

    public class Pager
    {
        IEnumerable<PagerElement> Elements;

        public Pager(XmlDocument configuration)
        {
            LoadConfiguration(XDocument.Load(new XmlNodeReader(configuration)));
        }

        public Pager(XDocument configuration)
        {
            LoadConfiguration(configuration);
        }

        private void LoadConfiguration(XDocument configuration)
        {
            XElement documentElement = configuration.Elements().First();

            List<PagerElement> pagerElements = new List<PagerElement>();

            foreach (XElement element in documentElement.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "Literal":
                        pagerElements.Add(LoadLiteral(element));
                        break;
                    case "First":
                        pagerElements.Add(LoadFirst(element));
                        break;
                    case "Last":
                        pagerElements.Add(LoadLast(element));
                        break;
                    case "Previous":
                        pagerElements.Add(LoadPrevious(element));
                        break;
                    case "Next":
                        pagerElements.Add(LoadNext(element));
                        break;
                    case "PageCount":
                        pagerElements.Add(LoadPageCount(element));
                        break;
                    case "ItemCount":
                        pagerElements.Add(LoadItemCount(element));
                        break;
                    case "NumberRun":
                        {
                            PagerElement elm = LoadNumberRun(element);
                            if (elm != null)
                            {
                                pagerElements.Add(elm);
                            }

                            break;
                        }
                }
            }

            this.Elements = pagerElements;
        }

        private PagerElement LoadNumberRun(XElement element)
        {
            string currentFormat = "";
            string otherFormat = "";
            string seperatorFormat = "";
            bool collapseAdjacentSeperators = true;
            bool collapseAdjacentRanges = true;

            XElement formats = element.Element("Format");
            XElement regions = element.Element("Regions");
            if (formats != null && regions != null)
            {
                XElement currentElement = formats.Element("Current");
                XElement otherElement = formats.Element("Other");
                XElement seperatorElement = formats.Element("Seperator");
                XAttribute collapseSeperatorsProperty = element.Attribute("collapseAdjacentSeperators");
                XAttribute collapseRangesProperty = element.Attribute("collapseAdjacentRanges");

                if (currentElement != null)
                {
                    currentFormat = currentElement.Value;
                }

                if (otherElement != null)
                {
                    otherFormat = otherElement.Value;
                }

                if (seperatorElement != null)
                {
                    seperatorFormat = seperatorElement.Value;
                }

                if (collapseSeperatorsProperty != null)
                {
                    collapseAdjacentSeperators = bool.Parse(collapseSeperatorsProperty.Value);
                }

                if (collapseRangesProperty != null)
                {
                    collapseAdjacentRanges = bool.Parse(collapseRangesProperty.Value);
                }

                int firstNumber = 0;
                string firstSeperator = "";
                int adjacentMin = 0;
                int adjacentMax = 0;
                int fillMin = 0;
                int fillMax = 0;
                NumberRunElement.FillMode fillMode = NumberRunElement.FillMode.Linear;
                string lastSeperator = "";
                int lastNumber = 0;

                XElement firstRegion = regions.Element("First");
                XElement firstSeperatorRegion = regions.Element("FirstSeperator");
                XElement currentRegion = regions.Element("Current");
                XElement lastSeperatorRegion = regions.Element("LastSeperator");
                XElement lastRegion = regions.Element("Last");

                if (firstRegion != null)
                {
                    firstNumber = int.Parse(firstRegion.Attribute("number").Value);
                }

                if (firstSeperatorRegion != null)
                {
                    firstSeperator = firstSeperatorRegion.Value;
                }

                if (currentRegion != null)
                {
                    XAttribute adjacentMinAttribute = currentRegion.Attribute("adjacentMin");
                    XAttribute adjacentMaxAttribute = currentRegion.Attribute("adjacentMax");
                    XAttribute fillMinAttribute = currentRegion.Attribute("fillMin");
                    XAttribute fillMaxAttribute = currentRegion.Attribute("fillMax");
                    XAttribute fillModeAttribute = currentRegion.Attribute("fillMode");

                    if (adjacentMinAttribute != null)
                    {
                        adjacentMin = int.Parse(adjacentMinAttribute.Value);
                    }

                    if (adjacentMaxAttribute != null)
                    {
                        adjacentMax = int.Parse(adjacentMaxAttribute.Value);
                    }

                    if (fillMinAttribute != null)
                    {
                        fillMin = int.Parse(fillMinAttribute.Value);
                    }

                    if (fillMaxAttribute != null)
                    {
                        fillMax = int.Parse(fillMaxAttribute.Value);
                    }

                    if (fillModeAttribute != null)
                    {
                        fillMode = (NumberRunElement.FillMode)Enum.Parse(typeof(NumberRunElement.FillMode), fillModeAttribute.Value);
                    }
                }

                if (lastSeperatorRegion != null)
                {
                    lastSeperator = lastSeperatorRegion.Value;
                }

                if (lastRegion != null)
                {
                    lastNumber = int.Parse(firstRegion.Attribute("number").Value);
                }

                return new NumberRunElement(currentFormat, otherFormat, seperatorFormat, collapseAdjacentSeperators, collapseAdjacentRanges, firstNumber, firstSeperator, adjacentMin, adjacentMax, fillMin, fillMax, fillMode, lastSeperator, lastNumber);
            }

            return null;
        }

        private LiteralElement LoadLiteral(XElement element)
        {
            return new LiteralElement(element.Value);
        }

        private FirstElement LoadFirst(XElement element)
        {
            string format = element.Element("Format").Value;
            bool hideOnFirst = true;

            return new FirstElement(format, hideOnFirst);
        }

        private LastElement LoadLast(XElement element)
        {
            string format = element.Element("Format").Value;
            bool hideOnLast = true;

            return new LastElement(format, hideOnLast);
        }

        private PreviousElement LoadPrevious(XElement element)
        {
            string format = element.Element("Format").Value;
            bool hideOnFirst = true;

            return new PreviousElement(format, hideOnFirst);
        }

        private NextElement LoadNext(XElement element)
        {
            string format = element.Element("Format").Value;
            bool hideOnLast = true;

            return new NextElement(format, hideOnLast);
        }

        private PageCountElement LoadPageCount(XElement element)
        {
            string format = element.Element("Format").Value;

            return new PageCountElement(format);
        }

        private ItemCountElement LoadItemCount(XElement element)
        {
            string format = element.Element("Format").Value;

            return new ItemCountElement(format);
        }

        public string CreatePager(int? currentPage, int itemCount, int pageSize, GeneratePageLinkDelegate urlGenerator)
        {
            return this.CreatePager(currentPage, itemCount, pageSize, x => x.ToString(), urlGenerator);
        }

        public string CreatePager(int? currentPage, int itemCount, int pageSize, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
        {
            int pageCount = PageCount(itemCount, pageSize);
            int? page = currentPage;

            if (page.HasValue)
            {
                page = ClampPage(page, pageCount);
            }

            StringBuilder result = new StringBuilder();

            foreach (PagerElement element in this.Elements)
            {
                result.Append(element.Build(currentPage, pageCount, itemCount, itemGenerator, urlGenerator));
            }

            return result.ToString();
        }

        public static int ClampPage(int? page, int pageCount)
        {
            int pages = Math.Max(1, pageCount);

            if (!page.HasValue || page.Value <= 0)
            {
                return 1;
            }

            if (page.Value >= pages)
            {
                return pages;
            }

            return page.Value;
        }

        public static int PageCount(int itemCount, int pageSize)
        {
            return Math.Max(1, (int)Math.Ceiling((float)itemCount / pageSize));
        }

        public abstract class PagerElement
        {
            public abstract string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator);
        }

        private class FirstElement : PagerElement
        {
            string format;
            bool hideOnFirst = true;

            public FirstElement(string format, bool hideOnFirst)
            {
                this.format = format;
                this.hideOnFirst = hideOnFirst;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                if (currentPage.HasValue)
                {
                    if (currentPage.Value != 1 || !hideOnFirst)
                    {
                        string link = urlGenerator(1);

                        return string.Format(this.format, link);
                    }
                }

                return "";
            }
        }

        private class PreviousElement : PagerElement
        {
            string format;
            bool hideOnFirst = true;

            public PreviousElement(string format, bool hideOnFirst)
            {
                this.format = format;
                this.hideOnFirst = hideOnFirst;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                if (currentPage.HasValue)
                {
                    if (currentPage.Value != 1 || !hideOnFirst)
                    {
                        string link = urlGenerator(Pager.ClampPage(currentPage.Value - 1, pageCount));

                        return string.Format(this.format, link);
                    }
                }

                return "";
            }
        }

        private class NextElement : PagerElement
        {
            string format;
            bool hideOnLast = true;

            public NextElement(string format, bool hideOnLast)
            {
                this.format = format;
                this.hideOnLast = hideOnLast;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                if (currentPage.HasValue)
                {
                    if (currentPage.Value != pageCount || !hideOnLast)
                    {
                        string link = urlGenerator(Pager.ClampPage(currentPage.Value + 1, pageCount));

                        return string.Format(this.format, link);
                    }
                }

                return "";
            }
        }

        private class LastElement : PagerElement
        {
            string format;
            bool hideOnLast = true;

            public LastElement(string format, bool hideOnLast)
            {
                this.format = format;
                this.hideOnLast = hideOnLast;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                if (currentPage.HasValue)
                {
                    if (currentPage.Value != pageCount || !hideOnLast)
                    {
                        string link = urlGenerator(pageCount);

                        return string.Format(this.format, link);
                    }
                }

                return "";
            }
        }

        private class ItemCountElement : PagerElement
        {
            string format;

            public ItemCountElement(string format)
            {
                this.format = format;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                return string.Format(this.format, itemCount);
            }
        }

        private class LiteralElement : PagerElement
        {
            string text;

            public LiteralElement(string text)
            {
                this.text = text;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                return this.text;
            }
        }

        private class PageCountElement : PagerElement
        {
            string format;

            public PageCountElement(string format)
            {
                this.format = format;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                string pageNumber = "?";

                if (currentPage.HasValue)
                {
                    pageNumber = currentPage.Value.ToString();
                }

                return string.Format(this.format, pageNumber, pageCount);
            }
        }

        private class NumberRunElement : PagerElement
        {
            public enum FillMode
            {
                Linear, Exponential, OrderOfMagnitude
            };

            private class NumberRange
            {
                int min;
                int max;
                string seperatorBefore;
                string seperatorAfter;

                public NumberRange(int min, int max, string seperatorBefore, string seperatorAfter)
                {
                    if (min > max)
                    {
                        throw new InvalidOperationException();
                    }

                    this.min = min;
                    this.max = max;
                    this.seperatorBefore = seperatorBefore;
                    this.seperatorAfter = seperatorAfter;
                }

                public int Min
                {
                    get
                    {
                        return this.min;
                    }
                }

                public int Max
                {
                    get
                    {
                        return this.max;
                    }
                }

                public string SeperatorBefore
                {
                    get
                    {
                        return this.seperatorBefore;
                    }
                }

                public string SeperatorAfter
                {
                    get
                    {
                        return this.seperatorAfter;
                    }
                }
            }

            string currentFormat;
            string otherFormat;
            string seperatorFormat;
            bool collapseSeperators;
            bool collapseAdjacentElements;
            int firstNumber;
            string firstSeperator;
            int adjacentMin;
            int adjacentMax;
            int fillMin;
            int fillMax;
            FillMode fillMode;
            string lastSeperator;
            int lastNumber;

            public NumberRunElement(string currentFormat, string otherFormat, string seperatorFormat, bool collapseSeperators, bool collapseAdjacentElements, int firstNumber, string firstSeperator, int adjacentMin, int adjacentMax, int fillMin, int fillMax, FillMode fillMode, string lastSeperator, int lastNumber)
            {
                this.currentFormat = currentFormat;
                this.otherFormat = otherFormat;
                this.seperatorFormat = seperatorFormat;
                this.collapseSeperators = collapseSeperators;
                this.collapseAdjacentElements = collapseAdjacentElements;
                this.firstNumber = firstNumber;
                this.firstSeperator = firstSeperator;
                this.adjacentMin = adjacentMin;
                this.adjacentMax = adjacentMax;
                this.fillMin = fillMin;
                this.fillMax = fillMax;
                this.fillMode = fillMode;
                this.lastSeperator = lastSeperator;
                this.lastNumber = lastNumber;
            }

            public override string Build(int? currentPage, int pageCount, int itemCount, GenerateItemDelegate itemGenerator, GeneratePageLinkDelegate urlGenerator)
            {
                List<NumberRange> ranges = new List<NumberRange>();

                if (firstNumber > 0)
                {
                    int min = Pager.ClampPage(1, pageCount);
                    int max = Pager.ClampPage(firstNumber - 1, pageCount);

                    ranges.Add(new NumberRange(min, max, "", max == pageCount ? "" : this.firstSeperator));
                }

                if (lastNumber > 0)
                {
                    int min = Pager.ClampPage(pageCount - (lastNumber - 1), pageCount);
                    int max = Pager.ClampPage(pageCount, pageCount);

                    ranges.Add(new NumberRange(min, max, min == 1 ? "" : this.lastSeperator, ""));
                }

                int effectiveCurrent = 1;

                if (currentPage.HasValue)
                {
                    effectiveCurrent = currentPage.Value;
                }

                {
                    int min = Pager.ClampPage(effectiveCurrent - adjacentMin, pageCount);
                    int max = Pager.ClampPage(effectiveCurrent + adjacentMin, pageCount);

                    int addMax = adjacentMin - (effectiveCurrent - min);
                    int subMin = adjacentMin - (max - effectiveCurrent);

                    if (addMax > 0)
                    {
                        max += addMax;
                        max = Math.Min(effectiveCurrent + adjacentMax, max);
                    }

                    if (subMin > 0)
                    {
                        min -= subMin;
                        min = Math.Max(effectiveCurrent - adjacentMax, min);
                    }

                    max = Pager.ClampPage(max, pageCount);
                    min = Pager.ClampPage(min, pageCount);

                    ranges.Add(new NumberRange(min, max, "", ""));
                }

                while (true)
                {
                    var match = from a in ranges
                                from b in ranges
                                where a != b
                                where a.Min <= b.Min
                                where (!this.collapseAdjacentElements && (a.Min < (b.Max + 1) && b.Min < (a.Max + 1))) || (this.collapseAdjacentElements && (a.Min <= (b.Max + 1) && b.Min <= (a.Max + 1)))
                                select new
                                {
                                    a,
                                    b
                                };

                    if (!match.Any())
                    {
                        break;
                    }

                    var firstMatch = match.First();

                    ranges.Remove(firstMatch.a);
                    ranges.Remove(firstMatch.b);

                    int min;
                    int max;
                    string seperatorBefore;
                    string seperatorAfter;

                    if (firstMatch.a.Min < firstMatch.b.Min)
                    {
                        min = firstMatch.a.Min;
                        seperatorBefore = firstMatch.a.SeperatorBefore;
                    }
                    else if (firstMatch.b.Min < firstMatch.a.Min)
                    {
                        min = firstMatch.b.Min;
                        seperatorBefore = firstMatch.b.SeperatorBefore;
                    }
                    else
                    {
                        min = firstMatch.a.Min; // == firstMatch.b.Min
                        if (string.IsNullOrEmpty(firstMatch.b.SeperatorBefore))
                        {
                            seperatorBefore = firstMatch.a.SeperatorBefore;
                        }
                        else
                        {
                            seperatorBefore = firstMatch.b.SeperatorBefore;
                        }
                    }

                    if (firstMatch.a.Max > firstMatch.b.Max)
                    {
                        max = firstMatch.a.Max;
                        seperatorAfter = firstMatch.a.SeperatorAfter;
                    }
                    else if (firstMatch.b.Max > firstMatch.a.Max)
                    {
                        max = firstMatch.b.Max;
                        seperatorAfter = firstMatch.b.SeperatorAfter;
                    }
                    else
                    {
                        max = firstMatch.a.Max;
                        if (string.IsNullOrEmpty(firstMatch.b.SeperatorAfter))
                        {
                            seperatorAfter = firstMatch.a.SeperatorAfter;
                        }
                        else
                        {
                            seperatorAfter = firstMatch.b.SeperatorAfter;
                        }
                    }

                    ranges.Add(new NumberRange(min, max, seperatorBefore, seperatorAfter));
                }

                StringBuilder output = new StringBuilder();

                var finalRanges = from a in ranges
                                  orderby a.Min
                                  select a;

                bool seperated = true;
                foreach (var range in finalRanges)
                {
                    if (!string.IsNullOrEmpty(range.SeperatorBefore))
                    {
                        if (!this.collapseSeperators && !seperated)
                        {
                            output.Append(this.seperatorFormat);
                        }

                        output.Append(range.SeperatorBefore);

                        if (!this.collapseSeperators)
                        {
                            output.Append(this.seperatorFormat);
                            seperated = true;
                        }
                        else
                        {
                            seperated = true;
                        }
                    }
                    else
                    {
                        if (!seperated)
                        {
                            output.Append(this.seperatorFormat);
                            seperated = true;
                        }
                    }


                    for (int i = range.Min; i <= range.Max; i++)
                    {
                        if (!seperated)
                        {
                            output.Append(this.seperatorFormat);
                            seperated = true;
                        }

                        string item = itemGenerator(i);
                        string url = urlGenerator(i);

                        if (currentPage.HasValue && i == currentPage.Value)
                        {
                            output.Append(string.Format(this.currentFormat, item, url));
                            seperated = false;
                        }
                        else
                        {
                            output.Append(string.Format(this.otherFormat, item, url));
                            seperated = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(range.SeperatorAfter))
                    {
                        if (!this.collapseSeperators && !seperated)
                        {
                            output.Append(this.seperatorFormat);
                            seperated = true;
                        }

                        output.Append(range.SeperatorAfter);

                        if (!this.collapseSeperators)
                        {
                            output.Append(this.seperatorFormat);
                            seperated = true;
                        }
                        else
                        {
                            seperated = true;
                        }
                    }
                }

                return output.ToString();
            }
        }
    }
}