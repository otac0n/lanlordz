//-----------------------------------------------------------------------
// <copyright file="PollsController.cs" company="LAN Lordz, inc.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.SiteTools;

    public class PollsController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult Index()
        {
            var allPolls = this.Polls.GetAllPolls();

            var model = new PollsIndexModel(this)
            {
                Polls = allPolls
            };

            return View(model);
        }

        [CompressFilter]
        public ActionResult Details(int id, int? partial)
        {
            var poll = this.Db.Polls.Where(p => p.PollId == id).SingleOrDefault();

            if (poll == null)
            {
                return View("NotFound");
            }

            var responses = poll.PollResponses.ToList();

            List<UsersPollResponse> userResponses = null;

            if (!poll.IsPrivate || (this.CurrentUser != null && (this.CurrentUser.UserID == poll.CreatorUserId || this.Security.IsUserAdministrator(CurrentUser))))
            {
                userResponses = this.Db.UsersPollResponses.Where(ur => this.Db.PollResponses.Where(r => r.PollId == poll.PollId && r.PollResponseId == ur.PollResponseId).Any()).ToList();
            }

            List<UsersPollResponse> currentUserResponses = null;

            if (this.CurrentUser != null)
            {
                currentUserResponses = this.Db.UsersPollResponses.Where(ur => ur.UserId == this.CurrentUser.UserID && this.Db.PollResponses.Where(r => r.PollId == poll.PollId && r.PollResponseId == ur.PollResponseId).Any()).ToList();
            }

            var model = new PollDetailsModel(this)
            {
                Poll = poll,
                Responses = responses,
                UsersResponses = userResponses,
                CurrentUserResponses = currentUserResponses
            };

            if (!partial.HasValue || partial.Value == 0)
            {
                return View("Details", model);
            }
            else
            {
                return PartialView("DetailsPartial", model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Vote(int id, FormCollection values)
        {
            if (this.CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            var poll = this.Db.Polls.Where(p => p.PollId == id).SingleOrDefault();

            if (poll == null)
            {
                return View("NotFound");
            }

            var response = values["Response"] == null ? "" : values["Response"];

            var responses = new List<string>(response.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            List<long> votes = new List<long>();
            foreach (var r in responses)
            {
                long vote = 0;
                if (!long.TryParse(r, out vote))
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could save your vote because at least one of your votes was invalid."
                    });
                }

                if (!(from pr in this.Db.PollResponses
                      where pr.PollId == id
                      where pr.PollResponseId == vote
                      select pr).Any())
                {
                    return View("Error", new ErrorInfoModel(this)
                    {
                        ErrorMessage = "Could save your vote because at least one of your votes was for a response that was not found."
                    });
                }

                votes.Add(vote);
            }

            if (!poll.IsMultiAnswer && votes.Count > 1)
            {
                return View("Error", new ErrorInfoModel(this)
                {
                    ErrorMessage = "Could save your vote because you specified more than one response for a poll that was not listed as multi-answer."
                });
            }

            var currentVotes = (from ur in this.Db.UsersPollResponses
                                where ur.UserId == this.CurrentUser.UserID
                                where this.Db.PollResponses.Where(pr => pr.PollId == id && pr.PollResponseId == ur.PollResponseId).Any()
                                select ur).ToList();

            foreach (var currentVote in currentVotes)
            {
                currentVote.IsSelected = false;
            }

            foreach (var vote in votes)
            {
                var currentVote = currentVotes.Where(cv => cv.PollResponseId == vote).SingleOrDefault();

                if (currentVote != null)
                {
                    currentVote.IsSelected = true;
                }
                else
                {
                    currentVote = new UsersPollResponse
                    {
                        UserId = this.CurrentUser.UserID,
                        IsSelected = true,
                        PollResponseId = vote
                    };

                    this.Db.UsersPollResponses.InsertOnSubmit(currentVote);
                }
            }

            this.Db.SubmitChanges();

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            else
            {
                return RedirectToAction("Details", new { id = id });
            }
        }

        [CompressFilter]
        public ActionResult Create()
        {
            if (this.CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            return View("Create", new CreatePollModel(this)
            {
                Title = "",
                Description = "",
                IsMultiAnswer = false,
                IsPrivate = false,
                Responses = new List<string>()
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Create(FormCollection values)
        {
            if (this.CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            var poll = new Poll
            {
                Title = values["Title"],
                Text = values["Description"],
                IsPrivate = values["IsPrivate"] == "on",
                IsMultiAnswer = values["IsMultiAnswer"] == "on",
                CreatorUserId = this.CurrentUser.UserID,
            };

            var responses = (from k in values.AllKeys
                             where !string.IsNullOrEmpty(k) && k.Length >= 8
                             where k.Substring(0, 8).ToUpperInvariant() == "RESPONSE"
                             let value = values[k] == null ? string.Empty : values[k].Trim()
                             where !string.IsNullOrEmpty(value)
                             select new PollResponse
                             {
                                 Label = values[k],
                                 Poll = poll
                             }).ToList();

            if (!poll.IsValid || responses.Where(r => !r.IsValid).Any() || responses.Count == 0)
            {
                foreach (var violation in poll.GetRuleViolations())
                {
                    this.ModelState.AddModelError(violation.PropertyName, violation.ErrorMessage);
                }

                responses.ForEach(r =>
                {
                    foreach (var violation in r.GetRuleViolations())
                    {
                        this.ModelState.AddModelError(violation.PropertyName, violation.ErrorMessage);
                    }
                });

                if (responses.Count == 0)
                {
                    this.ModelState.AddModelError("Responses", "You must specify at least one poll response.");
                }

                return View("Create", new CreatePollModel(this)
                {
                    Title = poll.Title,
                    Description = poll.Text,
                    IsPrivate = poll.IsPrivate,
                    IsMultiAnswer = poll.IsMultiAnswer,
                    Responses = (from r in responses
                                 select r.Label).ToList(),
                });
            }

            this.Db.Polls.InsertOnSubmit(poll);

            this.Db.PollResponses.InsertAllOnSubmit(responses);

            this.Db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}