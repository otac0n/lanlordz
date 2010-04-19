//-----------------------------------------------------------------------
// <copyright file="SitemapController.cs" company="LAN Lordz, inc.">
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

namespace LanLordz.Controllers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Mvc;
    using System.Xml;
    using LanLordz.SiteTools;

    public class SitemapController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult Index()
        {
            using (var s = new MemoryStream())
            {
                string xml;

                using (var w = new XmlTextWriter(s, Encoding.UTF8))
                {
                    w.Formatting = Formatting.Indented;
                    w.Indentation = 2;
                    w.IndentChar = ' ';

                    w.WriteStartDocument();
                    w.WriteStartElement("urlset");
                    w.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    this.AddUrl(w, "Home", "Index");
                    this.AddUrl(w, "Home", "ViewCrew");
                    this.AddUrl(w, "Home", "ViewSponsors");
                    this.AddUrl(w, "Home", "ViewNews");

                    this.AddUrl(w, "Stats", "Index");
                    this.AddUrl(w, "Stats", "Users");
                    this.AddUrl(w, "Stats", "Events");

                    this.AddUrl(w, "Polls", "Index");
                    foreach (var i in Polls.GetAllPolls())
                    {
                        this.AddUrl(w, "Polls", "Details", i.PollId.ToString(), i.Title);
                    }

                    this.AddUrl(w, "Events", "Index");
                    foreach (var i in Events.GetAllEvents())
                    {
                        this.AddUrl(w, "Events", "Details", i.EventID.ToString(), i.Title);
                        foreach (var j in Events.GetEventTournaments(i.EventID))
                        {
                            this.AddUrl(w, "Events", "ViewTournament", j.TournamentID.ToString(), j.Title);
                        }
                    }

                    this.AddUrl(w, "Forums", "Index");
                    var groups = this.Forums.GetViewableForumGroups(null);
                    var forums = this.Forums.GetViewableForums(null, groups);
                    foreach (var i in forums)
                    {
                        this.AddUrl(w, "Forums", "ViewForum", i.Forum.ForumID.ToString(), i.Forum.Name);
                        foreach (var j in this.Forums.GetForumViewableThreads(i.Forum.ForumID))
                        {
                            this.AddUrl(w, "Forums", "ViewThread", j.ThreadID.ToString(), j.Title);
                        }
                    }

                    foreach (var i in this.Users.GetAllUsers())
                    {
                        this.AddUrl(w, "Account", "ViewProfile", i.UserID.ToString(), i.Username);
                    }

                    w.WriteEndElement();
                    w.WriteEndDocument();
                    w.Flush();

                    s.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(s))
                    {
                        xml = sr.ReadToEnd();
                    }
                }

                return Content(xml, "text/xml");
            }
        }

        private void AddUrl(XmlWriter w, string controller, string action, string id, string title)
        {
            w.WriteStartElement("url");
            w.WriteStartElement("loc");
            w.WriteString(new Uri(Request.Url, Url.Action(action, controller, new { id = id, title = this.CreateUrlTitle(title) })).ToString());
            w.WriteEndElement();
            w.WriteEndElement();
        }

        private void AddUrl(XmlWriter w, string controller, string action, string id)
        {
            w.WriteStartElement("url");
            w.WriteStartElement("loc");
            w.WriteString(new Uri(Request.Url, Url.Action(action, controller, new { id = id })).ToString());
            w.WriteEndElement();
            w.WriteEndElement();
        }

        private void AddUrl(XmlWriter w, string controller, string action)
        {
            w.WriteStartElement("url");
            w.WriteStartElement("loc");
            w.WriteString(new Uri(Request.Url, Url.Action(action, controller)).ToString());
            w.WriteEndElement();
            w.WriteEndElement();
        }
    }
}
