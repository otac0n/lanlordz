//-----------------------------------------------------------------------
// <copyright file="EventRepository.cs" company="LAN Lordz, inc.">
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

namespace LanLordz.Controllers.CachedRepositories
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using LanLordz.Models;
    using LanLordz.Models.CachedModels;
    using Microsoft.Practices.EnterpriseLibrary.Caching;
    using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

    public class EventRepository
    {
        private readonly LanLordzDataContext db;
        private readonly ICacheManager dataCache;
        private readonly ConfigurationRepository config;

        public EventRepository(LanLordzDataContext db, ICacheManager dataCache, ConfigurationRepository config)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (dataCache == null)
            {
                throw new ArgumentNullException("dataCache");
            }

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.db = db;
            this.dataCache = dataCache;
            this.config = config;
        }

        public static IEnumerable<CachedEvent> FilterFutureOnly(IEnumerable<CachedEvent> events)
        {
            return from evt in events
                   where evt.BeginDateTime > DateTime.UtcNow
                   select evt;
        }

        public static IEnumerable<CachedEvent> FilterCurrentOnly(IEnumerable<CachedEvent> events)
        {
            return from evt in events
                   where evt.EndDateTime >= DateTime.UtcNow && evt.BeginDateTime <= DateTime.UtcNow
                   select evt;
        }

        public static IEnumerable<CachedEvent> FilterPastOnly(IEnumerable<CachedEvent> events)
        {
            return from evt in events
                   where evt.EndDateTime < DateTime.UtcNow
                   select evt;
        }

        public IEnumerable<EventRegistrationInformation> GetEventRegistrations(long eventId)
        {
            return this.LoadEventRegistrations(eventId);
        }

        public void InvalidateEventRegistrations(long eventId)
        {
            string dataKey = "EventRegistrations." + eventId;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public void RegisterEventUser(long eventId, long userId)
        {
            var r = from registration in this.db.Registrations
                    where registration.EventID == eventId && registration.UserID == userId
                    select registration;

            if (!r.Any())
            {
                var reg = new Registration()
                {
                    UserID = userId,
                    EventID = eventId,
                    RegistrationDate = DateTime.UtcNow,
                    IsCheckedIn = false
                };

                this.db.Registrations.InsertOnSubmit(reg);
                this.db.SubmitChanges();

                this.InvalidateEventRegistrations(eventId);
            }
        }

        public IEnumerable<EventImage> GetEventImages(long eventId)
        {
            return this.LoadEventImages(eventId);
        }

        public void InvalidateEventImages(long eventId)
        {
            string dataKey = "EventImages." + eventId;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public CachedEvent GetEvent(long eventId)
        {
            return this.LoadEvents().Where(e => e.EventID == eventId).SingleOrDefault();
        }

        public IEnumerable<CachedEvent> GetAllEvents()
        {
            return this.LoadEvents();
        }

        public IEnumerable<CachedEvent> GetUpcomingEvents()
        {
            return FilterFutureOnly(this.LoadEvents());
        }

        public IEnumerable<CachedEvent> GetCurrentEvents()
        {
            return FilterCurrentOnly(this.LoadEvents());
        }

        public IEnumerable<CachedEvent> GetPastEvents()
        {
            return FilterPastOnly(this.LoadEvents());
        }

        public void InvalidateAllEvents()
        {
            string dataKey = "Events";

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public IEnumerable<CachedVenue> GetAllVenues()
        {
            return this.LoadVenues();
        }

        public CachedVenue GetVenue(long venueId)
        {
            return this.LoadVenues().Where(v => v.VenueID == venueId).SingleOrDefault();
        }

        public void InvalidateVenues()
        {
            string dataKey = "Venues";

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public IEnumerable<CachedTournament> GetEventTournaments(long eventId)
        {
            return this.LoadEventTournaments(eventId);
        }

        public void InvalidateEventTournaments(long eventId)
        {
            string dataKey = "EventTournaments." + eventId;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        public CachedTournament GetTournament(long tournamentId)
        {
            return this.LoadTournament(tournamentId);
        }

        public void InvalidateTournament(long tournamentId)
        {
            string dataKey = "Tournament." + tournamentId;

            lock (this.dataCache)
            {
                this.dataCache.Remove(dataKey);
            }
        }

        private IEnumerable<EventRegistrationInformation> LoadEventRegistrations(long eventId)
        {
            string dataKey = "EventRegistrations." + eventId;

            lock (this.dataCache)
            {
                var value = (IEnumerable<EventRegistrationInformation>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from registration in this.db.Registrations
                             join user in this.db.Users on registration.UserID equals user.UserID
                             orderby registration.RegistrationDate
                             where registration.EventID == eventId
                             select new EventRegistrationInformation
                             {
                                 Registration = registration,
                                 Username = user.Username,
                                 UserID = user.UserID
                             }).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private IEnumerable<EventImage> LoadEventImages(long eventId)
        {
            string dataKey = "EventImages." + eventId;

            lock (this.dataCache)
            {
                var value = (IEnumerable<EventImage>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from eventImage in this.db.EventImages
                             where eventImage.EventID == eventId
                             select eventImage).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private IEnumerable<CachedEvent> LoadEvents()
        {
            string dataKey = "Events";

            lock (this.dataCache)
            {
                var value = (IEnumerable<CachedEvent>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from e in this.db.Events
                             select new CachedEvent(e)).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private IEnumerable<CachedVenue> LoadVenues()
        {
            string dataKey = "Venues";

            lock (this.dataCache)
            {
                var value = (IEnumerable<CachedVenue>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from v in this.db.Venues
                             select new CachedVenue(v)).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private IEnumerable<CachedTournament> LoadEventTournaments(long eventId)
        {
            string dataKey = "EventTournaments." + eventId;

            lock (this.dataCache)
            {
                var value = (IEnumerable<CachedTournament>)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    value = (from t in this.db.Tournaments
                             where t.EventID == eventId
                             select new CachedTournament(t)).ToList();

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }

        private CachedTournament LoadTournament(long tournamentId)
        {
            string dataKey = "Tournament." + tournamentId;

            lock (this.dataCache)
            {
                var value = (CachedTournament)this.dataCache[dataKey];
                if (value == null)
                {
                    Trace.WriteLine(@"Cache Miss: " + dataKey);

                    var tournament = (from t in this.db.Tournaments
                                      where t.TournamentID == tournamentId
                                      select t).SingleOrDefault();

                    if (tournament == null)
                    {
                        return null;
                    }

                    value = new CachedTournament(tournament);

                    if (this.config.UseAggressiveCaching)
                    {
                        this.dataCache.Add(dataKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(new TimeSpan(6, 0, 0)), new SlidingTime(new TimeSpan(2, 0, 0)));
                    }
                }
                else
                {
                    Trace.WriteLine(@"Cache Hit: " + dataKey);
                }

                return value;
            }
        }
    }
}
