//-----------------------------------------------------------------------
// <copyright file="EventsController.cs" company="LAN Lordz, inc.">
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
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using LanLordz.Controllers.CachedRepositories;
    using LanLordz.Models;
    using LanLordz.Models.CachedModels;
    using LanLordz.SiteTools;
    using Tournaments;

    public class EventsController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult Index()
        {
            var eventList = this.Events.GetAllEvents();
            var venues = this.Events.GetAllVenues();

            var events = new EventsIndexModel
            {
                Venues = venues,
                Upcoming = EventRepository.FilterFutureOnly(eventList),
                Current = EventRepository.FilterCurrentOnly(eventList),
                Past = EventRepository.FilterPastOnly(eventList)
            };

            return View("Index", events);
        }

        [CompressFilter]
        public ActionResult Details(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var evt = this.Events.GetEvent(id.Value);

            if (evt == null)
            {
                return View("NotFound");
            }

            var e = new EventDetailsModel
            {
                Event = evt,
                Registrants = this.Events.GetEventRegistrations(id.Value),
                Images = this.Events.GetEventImages(id.Value),
                Tournaments = this.Events.GetEventTournaments(id.Value),
                Venue = this.Events.GetVenue(evt.VenueID)
            };

            return View("Details", e);
        }

        public TournamentAccess GetUserTournamentAccess(CachedTournament tournament, User user)
        {
            IPairingsGenerator pg = null;
            TournamentRound rnd = null;

            var tmtEvent = this.Db.Events.Where(e => e.EventID == tournament.EventID).Single();
            var tmtRounds = this.Db.Rounds.Where(r => r.TournamentID == tournament.TournamentID);
            var tmtTeams = this.Db.Teams.Where(t => t.TournamentID == tournament.TournamentID);

            var newRoundThrewException = false;
            try
            {
                pg = this.GetInitializedTournamentPairingsGenerator(tournament);
                if (pg != null)
                {
                    rnd = pg.CreateNextRound(null);
                }
            }
            catch (InvalidTournamentStateException)
            {
                if (pg != null)
                {
                    newRoundThrewException = true;
                }
            }

            var pairingsGeneratorFound = pg != null;
            var nextRoundAvailable = rnd != null;
            var userLoggedOn = user != null;
            var userIsAdmin = userLoggedOn && this.Security.IsUserAdministrator(user);
            var userIsTD = userIsAdmin || (userLoggedOn && (tournament.TournamentDirectorUserID == user.UserID));
            var userRegisteredForEvent = userLoggedOn && tmtEvent.Registrations.Where(reg => reg.UserID == user.UserID).Any();
            var userOnATeam = userLoggedOn && tmtTeams.Where(t => t.UsersTeams.Where(ut => ut.UserID == user.UserID).Any()).Any();
            var tournyIsSinglePlayer = tournament.TeamSize == 1;
            var userIsTeamCaptain = userLoggedOn && userOnATeam && (tournyIsSinglePlayer || tmtTeams.Where(t => t.UsersTeams.Where(ut => ut.UserID == user.UserID && ut.IsTeamCaptain).Any()).Any());
            var eventOver = tmtEvent.EndDateTime <= DateTime.UtcNow;
            var tournyStarted = tmtRounds.Count() > 0;
            var tournyAllowsLateEntry = pairingsGeneratorFound && tournament.AllowLateEntry && pg.SupportsLateEntry;
            var tournyOver = pairingsGeneratorFound && tournyStarted && (!nextRoundAvailable && !newRoundThrewException);
            var tournyAcceptingTeams = !tournyOver && (!tournyStarted || tournyAllowsLateEntry);
            var userHasTDAccess = userIsAdmin || (userIsTD && !eventOver);
            var tournyIsLocked = tournament.IsLocked;

            var canCreateTeam = userLoggedOn && userRegisteredForEvent && !userOnATeam && tournyAcceptingTeams && !tournyIsLocked;
            var canJoinTeam = userLoggedOn && userRegisteredForEvent && !tournyIsSinglePlayer && !userOnATeam && tournyAcceptingTeams && !tournyIsLocked;
            var canLeaveTeam = userLoggedOn && userOnATeam && !tournyStarted && !tournyIsLocked;
            var canDisbandTeam = userLoggedOn && userIsTeamCaptain && !tournyStarted && !tournyIsLocked;
            var canManageTeam = userLoggedOn && !tournyIsSinglePlayer && userIsTeamCaptain && !tournyStarted && !tournyIsLocked;
            var canInputScores = userLoggedOn && userHasTDAccess && tournyStarted;
            var canStartNextRound = userLoggedOn && userHasTDAccess && nextRoundAvailable;
            var canRollBackRound = userLoggedOn && userHasTDAccess && tournyStarted;
            var canLockTeams = userLoggedOn && userHasTDAccess && !tournyIsLocked && !tournyOver;
            var canUnlockTeams = userLoggedOn && userHasTDAccess && tournyIsLocked && !tournyOver;
            var canEditTeams = userLoggedOn && userHasTDAccess && !tournyStarted && !tournyOver;

            return new TournamentAccess(canCreateTeam, canJoinTeam, canLeaveTeam, canDisbandTeam, canManageTeam, canInputScores, canStartNextRound, canRollBackRound, canLockTeams, canUnlockTeams, canEditTeams);
        }

        public ActionResult RenderTournament(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var pg = this.GetInitializedTournamentPairingsGenerator(tmt);
            ITournamentVisualizer viz = null;
            if (pg is ITournamentVisualizer)
            {
                viz = (ITournamentVisualizer)pg;
            }

            if (viz == null)
            {
                return View("NotAvailable");
            }

            var teams = new Dictionary<long, string>();
            foreach (var team in from t in this.Db.Teams
                                 where t.TournamentID == tmt.TournamentID
                                 select new KeyValuePair<long, string>(t.TeamID, t.TeamName))
            {
                teams.Add(team.Key, team.Value);
            }

            var g = new SvgNet.SvgGdi.SvgGraphics();
            viz.Render(g, new TournamentNameTable(teams));
            
            var xml = g.WriteSVGString(false, true);

            // This was preferred over the loading of the XML document, due to the huge performance hit (on the order of 5 seconds) of loading it in.
            // Is this because it was hitting an external link for the schema? I don't know, but this turned up to be about 9 seconds faster overall.
            xml = xml.Replace(@"<svg id=""SvgGdi_output"" xmlns=""http://www.w3.org/2000/svg"">", @"<svg id=""SvgGdi_output"" xmlns=""http://www.w3.org/2000/svg"" onload=""Initialize(evt)"">")
                     .Replace("</svg>", LanLordz.Properties.Resources.SvgScriptBlock + "</svg>");

            return Content(xml, "image/svg+xml");
        }

        [CompressFilter]
        public ActionResult ViewTournament(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var rounds = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID).ToList();
            var teams = this.Db.Teams.Where(t => t.TournamentID == tmt.TournamentID).ToList();
            var tournamentFinished = false;
            IEnumerable<TournamentStandingInformation> standings = null;
            var canRender = false;
            Size? renderedSize = null;

            IPairingsGenerator pg = null;
            ITournamentVisualizer viz = null;
            var stateValid = false;

            try
            {
                pg = this.GetInitializedTournamentPairingsGenerator(tmt);
                if (pg is ITournamentVisualizer)
                {
                    viz = (ITournamentVisualizer)pg;
                }

                stateValid = true;
            }
            catch (InvalidTournamentStateException ex)
            {
                Debug.WriteLine(ex);
            }

            if (viz != null)
            {
                canRender = true;
                var teamNames = new Dictionary<long, string>();
                foreach (var team in from tm in teams
                                     select new KeyValuePair<long, string>(tm.TeamID, tm.TeamName))
                {
                    teamNames.Add(team.Key, team.Value);
                }

                var g = new SvgNet.SvgGdi.SvgGraphics();

                var measuredSize = viz.Measure(g, new TournamentNameTable(teamNames));
                if (measuredSize.Width != 0 || measuredSize.Height != 0)
                {
                    renderedSize = new Size((int)Math.Round(measuredSize.Width, MidpointRounding.AwayFromZero), (int)Math.Round(measuredSize.Height, MidpointRounding.AwayFromZero));
                }
            }

            if (pg != null && stateValid)
            {
                try
                {
                    var nextRound = pg.CreateNextRound(null);
                    tournamentFinished = (nextRound == null) && (rounds.Count() > 1);
                }
                catch (InvalidTournamentStateException)
                {
                }

                try
                {
                    var rankings = pg.GenerateRankings();
                    standings = new List<TournamentStandingInformation>(from rk in rankings
                                                                        select new TournamentStandingInformation
                                                                        {
                                                                            Rank = rk.Rank,
                                                                            Team = teams.Where(tm => tm.TeamID == rk.Team.TeamId).Single(),
                                                                            Details = rk.ScoreDescription
                                                                        });
                }
                catch (InvalidTournamentStateException)
                {
                }
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            var tdm = new TournamentDetailsModel
            {
                Tournament = tmt,
                Rounds = rounds,
                Teams = teams,
                UserAccess = userAccess,
                Event = this.Db.Events.Where(e => e.EventID == tmt.EventID).Single(),
                Standings = standings,
                CanRenderView = canRender,
                RenderedSize = renderedSize,
                TournamentFinished = tournamentFinished
            };

            return View("ViewTournament", tdm);
        }

        [CompressFilter]
        public ActionResult CreateTournament()
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            var model = new CreateTournamentModel
            {
                Events = this.Events.GetAllEvents(),
                PairingsGenerators = this.Plugins.GetPairingsGeneratorList(),
                ScoreModes = GetScoreModes(),
            };

            return View("CreateTournament", model);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult CreateTournament(FormCollection values)
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            var tmtModel = new CreateTournamentModel
            {
                Events = this.Events.GetAllEvents(),
                PairingsGenerators = this.Plugins.GetPairingsGeneratorList(),
                ScoreModes = GetScoreModes(),
            };

            try
            {
                UpdateModel(tmtModel, new[] { "Event", "Title", "TeamSize", "Game", "ScoreMode", "PairingsGenerator", "TDUsername", "GameInfo", "ServerSettings" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Form", ex.Message);
                return View("CreateTournament", tmtModel);
            }

            var tmt = new Tournament
            {
                Title = tmtModel.Title,
                EventID = tmtModel.Event,
                TeamSize = tmtModel.TeamSize,
                Game = tmtModel.Game,
                ScoreMode = tmtModel.ScoreMode,
                PairingsGenerator = tmtModel.PairingsGenerator,
                GameInfo = tmtModel.GameInfo,
                ServerSettings = tmtModel.ServerSettings,
                TournamentDirectorUserID = null,
            };

            if (!string.IsNullOrEmpty(values["TDUsername"]))
            {
                var user = this.GetUser(values["TDUsername"]);

                if (user == null)
                {
                    ModelState.AddModelError("Form", "The user you entered could not be found.");
                    return View("CreateTournament", tmtModel);
                }

                tmt.TournamentDirectorUserID = user.UserID;
            }

            try
            {
                this.Db.Tournaments.InsertOnSubmit(tmt);
                this.Db.SubmitChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Form", ex.Message);
                return View("CreateTournament", tmtModel);
            }

            this.Events.InvalidateTournament(tmt.TournamentID);
            this.Events.InvalidateEventTournaments(tmt.EventID);

            return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult EditTournament(long? id)
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = (from t in this.Db.Tournaments
                       where t.TournamentID == id.Value
                       select t).SingleOrDefault();

            if (tmt == null)
            {
                return View("NotFound");
            }

            return View("EditTournament", new EditTournamentModel
            {
                Tournament = tmt,
                TDUsername = tmt.User != null ? tmt.User.Username : string.Empty,
                PairingsGenerators = this.Plugins.GetPairingsGeneratorList(),
                ScoreModes = GetScoreModes()
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult EditTournament(long? id, FormCollection values)
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = (from t in this.Db.Tournaments
                       where t.TournamentID == id.Value
                       select t).SingleOrDefault();

            if (tmt == null)
            {
                return View("NotFound");
            }

            long? userId;

            try
            {
                UpdateModel(tmt);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Form", ex.Message);

                return View("EditTournament", new EditTournamentModel
                {
                    Tournament = tmt,
                    TDUsername = values["TDUsername"],
                    PairingsGenerators = this.Plugins.GetPairingsGeneratorList(),
                    ScoreModes = GetScoreModes()
                });
            }

            if (string.IsNullOrEmpty(values["TDUsername"]))
            {
                userId = null;
            }
            else
            {
                var user = this.GetUser(values["TDUsername"]);

                if (user == null)
                {
                    ModelState.AddModelError("Form", "The user you entered could not be found.");

                    return View("EditTournament", new EditTournamentModel
                    {
                        Tournament = tmt,
                        TDUsername = values["TDUsername"],
                        PairingsGenerators = this.Plugins.GetPairingsGeneratorList(),
                        ScoreModes = GetScoreModes()
                    });
                }

                userId = user.UserID;
            }

            tmt.TournamentDirectorUserID = userId;

            try
            {
                this.Db.SubmitChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Form", ex.Message);

                return View("EditTournament", new EditTournamentModel
                {
                    Tournament = tmt,
                    PairingsGenerators = this.Plugins.GetPairingsGeneratorList(),
                    ScoreModes = GetScoreModes()
                });
            }

            this.Events.InvalidateTournament(tmt.TournamentID);
            this.Events.InvalidateEventTournaments(tmt.EventID);

            return RedirectToAction("ViewTournament", new { id = id });
        }

        [CompressFilter]
        public ActionResult CreateEvent()
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            return View(new CreateEventModel
            {
                RegisterCrew = true,
                AvailableVenues = this.Db.Venues.ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult CreateEvent(FormCollection values)
        {
            if (!this.Security.IsUserAdministrator(this.CurrentUser))
            {
                return View("NotAuthorized");
            }

            var viewModel = new CreateEventModel
            {
                AvailableVenues = this.Db.Venues.ToList()
            };

            if (!TryUpdateModel(viewModel, new[] { "BeginDateTime", "EndDateTime", "Info", "Title", "Seats", "VenueId", "RegisterCrew" }))
            {
                ModelState.AddModelError("Form", "Could not update model.");
                return View(viewModel);
            }

            int seats = 0;
            if (!int.TryParse(viewModel.Seats, out seats))
            {
                ModelState.AddModelError("Form", "Number of seats was invalid.");
                return View(viewModel);
            }

            DateTime beginDateTime;
            if (!DateTime.TryParse(viewModel.BeginDateTime, out beginDateTime))
            {
                ModelState.AddModelError("Form", "Begin date/time was invalid.");
                return View(viewModel);
            }

            DateTime endDateTime;
            if (!DateTime.TryParse(viewModel.EndDateTime, out endDateTime))
            {
                ModelState.AddModelError("Form", "End date/time was invalid.");
                return View(viewModel);
            }

            var tzi = this.AppManager.GetUserInformation(this.CurrentUser.UserID, false).TimeZoneInfo;
            if (tzi == null)
            {
                tzi = this.Config.DefaultTimeZoneInfo;
            }

            var evt = new Event
            {
                Title = viewModel.Title,
                Info = viewModel.Info,
                Seats = seats,
                VenueID = viewModel.VenueId,
                BeginDateTime = this.AppManager.ConvertDateTimeToUtc(beginDateTime, tzi),
                EndDateTime = this.AppManager.ConvertDateTimeToUtc(endDateTime, tzi)
            };

            if (!evt.IsValid)
            {
                foreach (var violation in evt.GetRuleViolations())
                {
                    ModelState.AddModelError("Form", violation.ErrorMessage);
                }

                return View(viewModel);
            }

            this.Db.Events.InsertOnSubmit(evt);
            this.Db.SubmitChanges();

            this.Events.InvalidateAllEvents();

            if (viewModel.RegisterCrew)
            {
                var g = this.Config.CrewGroup;
                var now = DateTime.UtcNow;

                var userIds = (from m in this.Db.UsersRoles
                               where m.RoleID == g
                               select m.UserID).ToList();

                var regs = from u in userIds
                           select new Registration
                           {
                               EventID = evt.EventID,
                               IsCheckedIn = false,
                               RegistrationDate = now,
                               UserID = u
                           };

                this.Db.Registrations.InsertAllOnSubmit(regs);
                this.Db.SubmitChanges();
            }

            return RedirectToAction("Details", "Events", new { id = evt.EventID });
        }

        private IPairingsGenerator GetTournamentPairingsGenerator(CachedTournament tournament)
        {
            return this.Plugins.GetPairingsGenerator(tournament.PairingsGenerator);
        }

        private enum ScoreMode
        {
            [Description("Highest Points Wins")]
            HighestPoints,

            [Description("Lowest Points Wins")]
            LowestPoints,

            [Description("Highest Time Wins")]
            HighestTime,

            [Description("Lowest Time Wins")]
            LowestTime,
        }

        private class Description : Attribute
        {
            private string text;

            public Description(string text)
            {
                this.text = text;
            }

            public static string GetDescription(Enum obj)
            {
                var t = obj.GetType();
                
                var mi = t.GetMember(obj.ToString());
                
                if (mi == null || mi.Length == 0)
                {
                    return obj.ToString();
                }

                var a = mi[0].GetCustomAttributes(typeof(Description), false);

                if (a == null || a.Length == 0)
                {
                    return obj.ToString();
                }

                return ((Description)a[0]).text;
            }
        }

        private IPairingsGenerator GetInitializedTournamentPairingsGenerator(CachedTournament tmt)
        {
            var pg = this.GetTournamentPairingsGenerator(tmt);
            var mode = (ScoreMode)Enum.Parse(typeof(ScoreMode), tmt.ScoreMode);

            if (pg != null)
            {
                var teams = (from team in this.Db.Teams
                             where team.TournamentID == tmt.TournamentID
                             select new TournamentTeam(team.TeamID, (int)team.UsersTeams.Average(ut => ut.Rating))).ToList();
                
                var rounds = new List<TournamentRound>();
                foreach (var round in from r in this.Db.Rounds
                                      where r.TournamentID == tmt.TournamentID
                                      orderby r.RoundNumber
                                      select r)
                {
                    var pairings = (from p in round.Pairings
                                    orderby p.PairingID
                                    select new TournamentPairing(
                                        (from tp in p.TeamsPairings
                                         orderby tp.TeamPairingID
                                         select new TournamentTeamScore(
                                             teams.Where(t => t.TeamId == tp.Team.TeamID).Single(),
                                             ParseScore(mode, tp.Score))).ToList())).ToList();
                    rounds.Add(new TournamentRound(pairings));
                }

                pg.LoadState(teams, rounds);

                return pg;
            }
            
            return null;
        }

        private static Score ParseScore(ScoreMode mode, string score)
        {
            if (string.IsNullOrEmpty(score))
            {
                return null;
            }

            try
            {
                switch (mode)
                {
                    case ScoreMode.HighestPoints:
                        return new HighestPointsScore(double.Parse(score));
                    case ScoreMode.LowestPoints:
                        return new LowestPointsScore(double.Parse(score));
                    case ScoreMode.HighestTime:
                        return new HighestTimeScore(ParseTime(score));
                    case ScoreMode.LowestTime:
                        return new LowestTimeScore(ParseTime(score));
                    default:
                        throw new InvalidOperationException();
                }
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static Dictionary<string, string> GetScoreModes()
        {
            var modes = new Dictionary<string, string>();

            var values = Enum.GetValues(typeof(ScoreMode));
            foreach (var value in values)
            {
                var name = Enum.GetName(typeof(ScoreMode), value);

                modes.Add(name, Description.GetDescription((ScoreMode)value));
            }

            return modes;
        }

        private static long ParseTime(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var rxp = new Regex(@"\A\s*(?:(?:(?<hours>[0-9]+):)?(?:(?<minutes>[0-5]?[0-9]):))?(?<seconds>[0-5]?[0-9](?:\.[0-9]*)?)\s*\Z", RegexOptions.Compiled);
            var match = rxp.Match(value);
            if (!match.Success)
            {
                throw new FormatException();
            }

            long hours;
            long minutes;
            long.TryParse(match.Groups["hours"].Value, out hours);
            long.TryParse(match.Groups["minutes"].Value, out minutes);
            var s = decimal.Parse(match.Groups["seconds"].Value);
            var milliseconds = (long)(s * 1000);

            return milliseconds + (1000 * (60 * (minutes + (60 * hours))));
        }

        [CompressFilter]
        public ActionResult CreateTeam(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanCreateTeam)
            {
                return View("NotAuthorized");
            }

            return View("CreateTeam", new CreateTeamModel
            {
                Tournament = tmt,
                Event = evt,
                TeamName = string.Empty,
                TeamTagFormat = "{0}"
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult CreateTeam(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanCreateTeam)
            {
                return View("NotAuthorized");
            }

            var currentTeam = from t in this.Db.Teams
                              where t.TournamentID == tmt.TournamentID
                              where t.UsersTeams.Where(ut => ut.UserID == CurrentUser.UserID).Any()
                              select t;

            int rating = 50;
            int.TryParse(values["Rating"], out rating);
            rating = Math.Max(Math.Min(rating, 100), 0);

            if (tmt.TeamSize == 1)
            {
                if (!currentTeam.Any())
                {
                    Team t = new Team
                    {
                        TournamentID = tmt.TournamentID,
                        TeamName = CurrentUser.Username,
                        TeamTagFormat = "{0}"
                    };

                    this.Db.Teams.InsertOnSubmit(t);

                    var ut = new UsersTeam
                    {
                        Team = t,
                        UserID = CurrentUser.UserID,
                        IsTeamCaptain = false,
                        Rating = rating,
                    };

                    this.Db.UsersTeams.InsertOnSubmit(ut);

                    this.Db.SubmitChanges();
                }

                return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
            }
            else
            {
                if (currentTeam.Any())
                {
                    ModelState.AddModelError("Form", "You are already on a team.");

                    return View("CreateTeam", new CreateTeamModel
                    {
                        Tournament = tmt,
                        Event = evt,
                        TeamName = values["TeamName"],
                        TeamTagFormat = values["TeamTagFormat"]
                    });
                }

                var t = new Team
                        {
                            TournamentID = tmt.TournamentID,
                            TeamName = values["TeamName"],
                            TeamTagFormat = values["TeamTagFormat"]
                        };

                if (!t.IsValid)
                {
                    foreach (RuleViolation rule in t.GetRuleViolations())
                    {
                        this.ModelState.AddModelError("Form", rule.ErrorMessage);
                    }

                    return this.View("CreateTeam", new CreateTeamModel
                                              {
                                                  Tournament = tmt,
                                                  Event = evt,
                                                  TeamName = values["TeamName"],
                                                  TeamTagFormat = values["TeamTagFormat"]
                                              });
                }

                this.Db.Teams.InsertOnSubmit(t);

                var ut = new UsersTeam
                         {
                             Team = t,
                             UserID = this.CurrentUser.UserID,
                             IsTeamCaptain = true,
                             Rating = rating,
                         };

                this.Db.UsersTeams.InsertOnSubmit(ut);

                this.Db.SubmitChanges();

                return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
            }
        }

        [CompressFilter]
        public ActionResult JoinTeam(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            TournamentAccess userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanJoinTeam)
            {
                return View("NotAuthorized");
            }

            var teams = from t in this.Db.Teams
                        where t.TournamentID == tmt.TournamentID
                        where t.UsersTeams.Count() < tmt.TeamSize
                        select t;

            return View("JoinTeam", new JoinTeamModel
            {
                Tournament = tmt,
                Event = evt,
                AvailableTeams = teams,
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult JoinTeam(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanJoinTeam)
            {
                return View("NotAuthorized");
            }

            var teams = from t in this.Db.Teams
                        where t.TournamentID == tmt.TournamentID
                        where t.UsersTeams.Count() < tmt.TeamSize
                        select t;

            long teamId = long.Parse(values["Team"]);

            if (!teams.Where(t => t.TeamID == teamId).Any())
            {
                this.ModelState.AddModelError("Form", "The team you chose is no longer available.");

                return View("JoinTeam", new JoinTeamModel
                {
                    Tournament = tmt,
                    Event = evt,
                    AvailableTeams = teams,
                });
            }
            else
            {
                int rating = 50;
                int.TryParse(values["Rating"], out rating);
                rating = Math.Max(Math.Min(rating, 100), 0);

                var ut = new UsersTeam
                {
                    UserID = this.CurrentUser.UserID,
                    TeamID = teamId,
                    IsTeamCaptain = false,
                    Rating = rating,
                };

                this.Db.UsersTeams.InsertOnSubmit(ut);

                this.Db.SubmitChanges();
            }

            return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult LeaveTeam(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanLeaveTeam && !userAccess.CanDisbandTeam)
            {
                return View("NotAuthorized");
            }

            var currentTeam = from t in this.Db.Teams
                              where t.TournamentID == tmt.TournamentID
                              where t.UsersTeams.Where(ut => ut.UserID == CurrentUser.UserID).Any()
                              select t;

            var team = currentTeam.Single();
            var userTeam = team.UsersTeams.Where(ut => ut.UserID == CurrentUser.UserID).Single();

            return View("LeaveTeam", new LeaveTeamModel
            {
                Tournament = tmt,
                Event = evt,
                Team = team,
                UserTeam = userTeam
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult LeaveTeam(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanLeaveTeam && !userAccess.CanDisbandTeam)
            {
                return View("NotAuthorized");
            }

            var currentTeam = from t in this.Db.Teams
                              where t.TournamentID == tmt.TournamentID
                              where t.UsersTeams.Where(ut => ut.UserID == CurrentUser.UserID).Any()
                              select t;

            var team = currentTeam.Single();
            var userTeam = team.UsersTeams.Where(ut => ut.UserID == CurrentUser.UserID).Single();

            var disband = false;
            if (team.UsersTeams.Count() > 1 && userTeam.IsTeamCaptain)
            {
                disband = values["Disband"] == "on";
            }
            else if (team.UsersTeams.Count() == 1)
            {
                disband = true;
            }

            if (disband)
            {
                this.Db.UsersTeams.DeleteAllOnSubmit(team.UsersTeams);
                this.Db.Teams.DeleteOnSubmit(team);
                this.Db.SubmitChanges();

                return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
            }

            if (userTeam.IsTeamCaptain && team.UsersTeams.Count() > 1 && team.UsersTeams.Where(ut => ut.IsTeamCaptain).Count() == 1)
            {
                this.ModelState.AddModelError("Form", "You may not leave your team, because you are the last team captain.  Either disband the team or grant team captain to another player first.");

                return this.View("LeaveTeam", new LeaveTeamModel
                                         {
                                             Tournament = tmt,
                                             Event = evt,
                                             Team = team,
                                             UserTeam = userTeam
                                         });
            }
            
            this.Db.UsersTeams.DeleteOnSubmit(userTeam);
            this.Db.SubmitChanges();

            return this.RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult ManageTeam(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanManageTeam)
            {
                return View("NotAuthorized");
            }

            var team = this.Db.Teams.Where(t => t.TournamentID == tmt.TournamentID && t.UsersTeams.Where(ut => ut.UserID == this.CurrentUser.UserID).Any()).Single();

            return View("ManageTeam", new ManageTeamModel
            {
                Tournament = tmt,
                Team = team,
                Event = this.Events.GetEvent(tmt.EventID),
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult ManageTeam(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanManageTeam)
            {
                return View("NotAuthorized");
            }

            var team = this.Db.Teams.Where(t => t.TournamentID == tmt.TournamentID && t.UsersTeams.Where(ut => ut.UserID == this.CurrentUser.UserID).Any()).Single();

            if (!values.AllKeys.Contains("Action"))
            {
                team.TeamName = values["TeamName"];
                team.TeamTagFormat = values["TeamTagFormat"];

                var violations = team.GetRuleViolations();
                if (violations.Count() > 0)
                {
                    foreach (var violation in violations)
                    {
                        this.ModelState.AddModelError("Form", violation.ErrorMessage);
                    }

                    return View("ManageTeam", new ManageTeamModel
                    {
                        Tournament = tmt,
                        Team = team,
                        Event = evt
                    });
                }

                this.Db.SubmitChanges();
            }
            else
            {
                long userId = long.Parse(values["User"]);
                if (userId != this.CurrentUser.UserID)
                {
                    var userTeam = team.UsersTeams.Where(u => u.UserID == userId).SingleOrDefault();

                    switch (values["Action"])
                    {
                        case "Grant":
                            userTeam.IsTeamCaptain = true;
                            break;
                        case "Revoke":
                            userTeam.IsTeamCaptain = false;
                            break;
                        case "Kick":
                            this.Db.UsersTeams.DeleteOnSubmit(userTeam);
                            break;
                    }

                    this.Db.SubmitChanges();
                }
            }

            return RedirectToAction("ManageTeam", new { id = id });
        }

        [CompressFilter]
        public ActionResult LockTeams(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanLockTeams)
            {
                return View("NotAuthorized");
            }

            return View("LockTeams", new TournamentActionModel
            {
                Tournament = tmt,
                Event = evt,
                Rounds = null,
                Teams = null
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult LockTeams(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanLockTeams)
            {
                return View("NotAuthorized");
            }

            var tournament = this.Db.Tournaments.Where(t => t.TournamentID == tmt.TournamentID).Single();

            tournament.IsLocked = true;
            this.Db.SubmitChanges();

            this.Events.InvalidateTournament(tmt.TournamentID);

            return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult UnlockTeams(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanUnlockTeams)
            {
                return View("NotAuthorized");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            return View("UnlockTeams", new TournamentActionModel
            {
                Tournament = tmt,
                Event = evt,
                Rounds = null,
                Teams = null
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult UnlockTeams(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanUnlockTeams)
            {
                return View("NotAuthorized");
            }

            var tournament = this.Db.Tournaments.Where(t => t.TournamentID == tmt.TournamentID).Single();

            tournament.IsLocked = false;
            this.Db.SubmitChanges();

            this.Events.InvalidateTournament(tmt.TournamentID);

            return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult StartNextRound(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanStartNextRound)
            {
                return View("NotAuthorized");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var rounds = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID).ToList();
            var teams = this.Db.Teams.Where(t => t.TournamentID == tmt.TournamentID).ToList();

            return View("StartNextRound", new TournamentActionModel
            {
                Tournament = tmt,
                Rounds = rounds,
                Teams = teams,
                Event = evt
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult StartNextRound(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanStartNextRound)
            {
                return View("NotAuthorized");
            }

            var rounds = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID).ToList();
            var teams = this.Db.Teams.Where(t => t.TournamentID == tmt.TournamentID).ToList();

            int? maxPairings = null;

            if (!string.IsNullOrEmpty(values["MaxPairings"]))
            {
                int mp;
                if (!int.TryParse(values["MaxPairings"], out mp) || mp < 0)
                {
                    this.ModelState.AddModelError("Form", "The value you entered for max pairings is invalid.");

                    return View("StartNextRound", new TournamentActionModel
                    {
                        Tournament = tmt,
                        Rounds = rounds,
                        Teams = teams,
                        Event = evt
                    });
                }
                
                maxPairings = mp;
            }

            TournamentRound round;

            try
            {
                IPairingsGenerator pg = this.GetInitializedTournamentPairingsGenerator(tmt);
                if (pg == null)
                {
                    throw new InvalidTournamentStateException("Failed to create the pairings generator.  This may be cause by a bad or missing plugin.");
                }

                round = pg.CreateNextRound(maxPairings);
                if (round == null)
                {
                    throw new InvalidTournamentStateException("The pairings generator returned null.  This is usually an indicator that the tournament is already finished, or cannot start due to too few participants.");
                }
            }
            catch (InvalidTournamentStateException ex)
            {
                ModelState.AddModelError("Form", ex.Message);

                return View("StartNextRound", new TournamentActionModel
                {
                    Tournament = tmt,
                    Rounds = rounds,
                    Teams = teams,
                    Event = evt
                });
            }

            var newRound = new Round
            {
                RoundNumber = rounds.Count() + 1,
                TournamentID = tmt.TournamentID
            };

            this.Db.Rounds.InsertOnSubmit(newRound);

            foreach (var pairing in round.Pairings)
            {
                var newPairing = new Pairing
                {
                    Round = newRound
                };

                this.Db.Pairings.InsertOnSubmit(newPairing);

                foreach (var teamScore in pairing.TeamScores)
                {
                    var newTeamPairing = new TeamsPairing
                    {
                        Team = this.Db.Teams.Where(t => t.TeamID == teamScore.Team.TeamId).Single(),
                        Pairing = newPairing,
                        Score = null
                    };

                    this.Db.TeamsPairings.InsertOnSubmit(newTeamPairing);
                }
            }

            this.Db.SubmitChanges();

            return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult RollBackRound(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanRollBackRound)
            {
                return View("NotAuthorized");
            }

            return View("RollBackRound", new TournamentActionModel
            {
                Tournament = tmt,
                Event = evt
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult RollBackRound(long? id, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanRollBackRound)
            {
                return View("NotAuthorized");
            }

            var round = (from r in this.Db.Rounds
                         where r.TournamentID == tmt.TournamentID
                         orderby r.RoundNumber descending
                         select r).First();

            foreach (var pairing in round.Pairings)
            {
                this.Db.TeamsPairings.DeleteAllOnSubmit(pairing.TeamsPairings);
                this.Db.Pairings.DeleteOnSubmit(pairing);
            }

            this.Db.Rounds.DeleteOnSubmit(round);
            this.Db.SubmitChanges();

            return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
        }

        [CompressFilter]
        public ActionResult InputScores(long? id, long? roundNumber)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanInputScores)
            {
                return View("NotAuthorized");
            }

            if (roundNumber.HasValue)
            {
                return View("InputScores", new InputScoresModel
                {
                    Tournament = tmt,
                    Round = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID && r.RoundNumber == roundNumber.Value).Single(),
                    Event = evt
                });
            }
            else
            {
                return View("InputScoresPickRound", new TournamentActionModel
                {
                    Tournament = tmt,
                    Rounds = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID).ToList(),
                    Event = evt
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult InputScores(long? id, int? roundNumber, FormCollection values)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            var tmt = this.Events.GetTournament(id.Value);

            if (tmt == null)
            {
                return View("NotFound");
            }

            var evt = this.Events.GetEvent(tmt.EventID);

            var userAccess = this.GetUserTournamentAccess(tmt, this.CurrentUser);

            if (!userAccess.CanInputScores)
            {
                return View("NotAuthorized");
            }

            if (roundNumber.HasValue)
            {
                var updates = from k in values.AllKeys
                              where k.IndexOf("score-") == 0
                              let split = k.Split("-".ToCharArray())
                              select new
                              {
                                  Key = k,
                                  PairingID = long.Parse(split[1]),
                                  TeamID = long.Parse(split[2]),
                              };

                foreach (var update in updates)
                {
                    var score = values[update.Key];

                    var pairing = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID && r.RoundNumber == roundNumber.Value).Single().Pairings.Where(p => p.PairingID == update.PairingID).Single().TeamsPairings.Where(tp => tp.TeamID == update.TeamID).Single();
                    pairing.Score = score;
                }

                this.Db.SubmitChanges();

                return RedirectToAction("ViewTournament", new { id = tmt.TournamentID });
            }

            int rnb;

            if (!int.TryParse(values["Round"], out rnb))
            {
                this.ModelState.AddModelError("Form", "Could not parse the round number.");

                return this.View("InputScoresPickRound", new TournamentActionModel
                                                    {
                                                        Tournament = tmt,
                                                        Rounds = this.Db.Rounds.Where(r => r.TournamentID == tmt.TournamentID).ToList(),
                                                        Event = evt
                                                    });
            }

            return this.RedirectToAction("InputScores", new { id = id, roundNumber = rnb });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Register(long? id)
        {
            if (!id.HasValue)
            {
                return View("NotAvailable");
            }

            if (this.CurrentUser != null)
            {
                this.Events.RegisterEventUser(id.Value, this.CurrentUser.UserID);
            }

            return RedirectToAction("Details", new { id = id.Value });
        }
    }
}
