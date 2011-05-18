//-----------------------------------------------------------------------
// <copyright file="AtomResult.cs" company="LAN Lordz, inc.">
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
    using System.ServiceModel.Syndication;
    using System.Web.Mvc;
    using System.Xml;

    public static class AtomResultHelper
    {
        public static AtomResult Atom(this Controller controller, string title, IEnumerable<SyndicationItem> items)
        {
            return new AtomResult(title, title, items);
        }

        public static AtomResult Atom(this Controller controller, string title, string description, IEnumerable<SyndicationItem> items)
        {
            return new AtomResult(title, description, items);
        }
    }

    public class AtomResult : FileResult
    {
        private readonly IEnumerable<SyndicationItem> items;
        private readonly string title;
        private readonly string description;

        private Uri currentUrl;

        public AtomResult(string title, string description, IEnumerable<SyndicationItem> items)
            : base("application/atom+xml")
        {
            this.items = items;
            this.title = title;
            this.description = description;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            currentUrl = context.RequestContext.HttpContext.Request.Url;
            base.ExecuteResult(context);
        }

        protected override void WriteFile(System.Web.HttpResponseBase response)
        {
            SyndicationFeed feed = new SyndicationFeed(
                this.title,
                this.description,
                currentUrl,
                items);

            var formatter = new Atom10FeedFormatter(feed);

            using (XmlWriter writer = XmlWriter.Create(response.Output))
            {
                formatter.WriteTo(writer);
            }
        }
    }
}
