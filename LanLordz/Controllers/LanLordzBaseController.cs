//-----------------------------------------------------------------------
// <copyright file="LanLordzBaseController.cs" company="LAN Lordz, inc.">
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
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using LanLordz.Controllers.CachedRepositories;
    using LanLordz.Models;
    using LanLordz.SiteTools;
    using Microsoft.Practices.EnterpriseLibrary.Caching;

    [ValidateInput(enableValidation: false)]
    public class LanLordzBaseController : Controller
    {
        private const string UserKey = "User";
        private const string AutoLogOnKey = "AutoLogOnKey";

        private static ICacheManager pluginCache = CacheFactory.GetCacheManager("PluginFactories");
        private static ICacheManager skinsCache = CacheFactory.GetCacheManager("SiteSkins");
        private static ICacheManager dataCache = CacheFactory.GetCacheManager("DataCache");

        private bool userLoaded;
        private User currentUser;
        private TimeZoneInfo userTimeZone;

        private Regex invalidUrlCharacters = new Regex(@"[^\w]+", RegexOptions.Compiled);

        private PluginRepository pluginRepo;
        private SkinRepository skinRepo;
        private GoogleGeocoder geocoder;

        public LanLordzBaseController()
            : base()
        {
            this.Db = new LanLordzDataContext();

            this.AppManager = new LanLordzApplicationManager(this, this.Db);

            var cache = LanLordzBaseController.dataCache;
            this.Config = new ConfigurationRepository(this.Db, cache);
            this.Security = new SecurityRepository(this.Db, cache, this.Config);
            this.Events = new EventRepository(this.Db, cache, this.Config);
            this.Polls = new PollRepository(this.Db, cache, this.Config);
            this.Users = new UserRepository(this.Db, cache, this.Config);
            this.Forums = new ForumRepository(this.Db, cache, this.Config, this.Security);
        }

        public LanLordzApplicationManager AppManager
        {
            get;
            private set;
        }

        public ConfigurationRepository Config
        {
            get;
            private set;
        }

        public SecurityRepository Security
        {
            get;
            private set;
        }

        public EventRepository Events
        {
            get;
            private set;
        }

        public PollRepository Polls
        {
            get;
            private set;
        }

        public ForumRepository Forums
        {
            get;
            private set;
        }

        public UserRepository Users
        {
            get;
            private set;
        }

        public PluginRepository Plugins
        {
            get
            {
                if (this.pluginRepo == null)
                {
                    this.pluginRepo = new PluginRepository(this.Server.MapPath("~/Plugins"), LanLordzBaseController.pluginCache, this.Config);
                }

                return this.pluginRepo;
            }
        }

        public SkinRepository Skins
        {
            get
            {
                if (this.skinRepo == null)
                {
                    this.skinRepo = new SkinRepository(this.Server.MapPath("~/Skins"), LanLordzBaseController.skinsCache, this.Config);
                }

                return this.skinRepo;
            }
        }

        public User CurrentUser
        {
            get
            {
                if (!this.userLoaded)
                {
                    this.currentUser = this.LoadUser();
                    this.userLoaded = true;
                }

                return this.currentUser;
            }
            protected set
            {
                this.Session[UserKey] = value;
                this.currentUser = value;
                this.userTimeZone = null;
            }
        }

        protected LanLordzDataContext Db
        {
            get;
            private set;
        }

        protected string Theme
        {
            get
            {
                var theme = string.Empty;
                if (this.CurrentUser != null)
                {
                    theme = this.GetUserInformation(this.CurrentUser.UserID, false).Theme;
                }

                if (string.IsNullOrEmpty(theme))
                {
                    theme = this.Config.DefaultTheme;
                }

                return theme;
            }
        }

        public GoogleGeocoder Geocoder
        {
            get
            {
                if (this.geocoder == null)
                {
                    this.geocoder = new GoogleGeocoder(this.Config.GoogleMapsKey);
                }

                return this.geocoder;
            }
        }

        public DateTime ConvertDateTime(DateTime dateTime)
        {
            if (this.userTimeZone == null)
            {
                this.userTimeZone = this.Config.DefaultTimeZoneInfo;

                if (this.CurrentUser != null)
                {
                    var ui = this.GetUserInformation(this.CurrentUser.UserID, false);
                    if (!string.IsNullOrEmpty(ui.TimeZone))
                    {
                        this.userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ui.TimeZone);
                    }
                }
            }

            return this.ConvertDateTime(dateTime, this.userTimeZone);
        }

        public DateTime ConvertDateTime(DateTime utcDateTime, TimeZoneInfo targetTimeZone)
        {
            TimeSpan ts = targetTimeZone.GetUtcOffset(utcDateTime);
            return utcDateTime.Add(ts);
        }

        public DateTime ConvertDateTimeToUtc(DateTime utcDateTime, TimeZoneInfo sourceTimeZone)
        {
            TimeSpan ts = sourceTimeZone.GetUtcOffset(utcDateTime);
            return utcDateTime.Subtract(ts);
        }

        public Dictionary<string, string> GetAvailableTimezones(bool includeSiteDefault)
        {
            var timezones = new Dictionary<string, string>();

            if (includeSiteDefault)
            {
                timezones.Add("", "(Site Default)");
            }

            foreach (var timezone in TimeZoneInfo.GetSystemTimeZones())
            {
                timezones.Add(timezone.Id, timezone.DisplayName);
            }

            return timezones;
        }

        public Dictionary<string, string> GetAvailableThemes(bool includeSiteDefault)
        {
            var themes = new Dictionary<string, string>();

            if (includeSiteDefault)
            {
                themes.Add("", "(Site Default)");
            }

            foreach (string theme in this.Skins.GetSkinsList())
            {
                themes.Add(theme, theme);
            }

            return themes;
        }

        public static string ComputeHash(string data)
        {
            var sha = new SHA512Managed();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty)));
        }

        public string FormatPostText(string postText)
        {
            var bbi = this.Skins.GetBBCodeInterpreter();
            lock (bbi)
            {
                return bbi.Interpret(postText);
            }
        }

        public string ApplyCoralCdn(string relativeUri)
        {
            if (this.Config.UseCoralCdn && this.Request.Url.Host != "localhost" && this.Request.Url.Host != "127.0.0.1")
            {
                var fullUri = new Uri(this.Request.Url, relativeUri);
                var builder = new UriBuilder(fullUri);
                builder.Host = builder.Host + ".nyud.net";
                return builder.Uri.ToString();
            }
            else
            {
                return relativeUri;
            }
        }

        public string CreateUrlTitle(string title)
        {
            title = this.invalidUrlCharacters.Replace(title, "-");
            return title.Trim('-').ToLower();
        }

        public UserInformation GetUserInformation(long userId, bool forceFresh)
        {
            if (forceFresh)
            {
                this.Users.InvalidateUserInformation(userId);
            }

            return this.Users.GetUserInformation(userId);
        }

        public static string CalculateScrapeBuster(string scrapeBusterKey, object additionalData)
        {
            var topic = Encoding.ASCII.GetBytes(scrapeBusterKey + "." + (additionalData == null ? string.Empty : additionalData.ToString()));
            return CalculateMd5(topic).Substring(0, 8);
        }

        public static string CalculateMd5(byte[] data)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(data);
            return Convert.ToBase64String(result);
        }

        protected void AddAuthFailure(long userId, string originatingHostIp)
        {
            AuthFailure af = new AuthFailure()
            {
                UserID = userId,
                OriginatingHostIP = originatingHostIp,
                CreateDate = DateTime.UtcNow
            };

            this.Db.AuthFailures.InsertOnSubmit(af);
            this.Db.SubmitChanges();
        }

        protected bool IsLogOnAttemptUnderThreshold(string originatingHostIp)
        {
            return !(GetHostFailures(originatingHostIp) >= 3);
        }

        protected bool IsLogOnAttemptUnderThreshold(long userId)
        {
            return !(GetUserFailures(userId) >= 3);
        }

        protected long GetHostFailures(string originatingHostIp)
        {
            var hf = from authFailure in this.Db.AuthFailures
                     where authFailure.OriginatingHostIP.Equals(originatingHostIp) && authFailure.CreateDate > DateTime.UtcNow.AddMinutes(-30d)
                     select authFailure;

            return hf.Count();
        }

        protected long GetUserFailures(long userId)
        {
            var uf = from authFailure in this.Db.AuthFailures
                     where authFailure.UserID == userId && authFailure.CreateDate > DateTime.UtcNow.AddMinutes(-30d)
                     select authFailure;

            return uf.Count();
        }

        protected User GetUser(long userId)
        {
            return this.Db.Users.SingleOrDefault(u => u.UserID == userId);
        }

        protected User GetUser(string username)
        {
            return this.Db.Users.SingleOrDefault(u => u.Username.Equals(username));
        }

        protected User Register(string username, string password, string email, string question, string answer, char gender, bool showEmail, bool showGender, HttpRequestBase request, HttpResponseBase response)
        {
            var validEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            var whiteSpace = new Regex(@"\s");

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Your username must not be blank.");
            }

            if (string.IsNullOrEmpty(password) || password.Length < 5)
            {
                throw new ArgumentException("Your password must be at least five characters.");
            }

            if (string.IsNullOrEmpty(email) || !validEmail.IsMatch(email))
            {
                throw new ArgumentException("The email you entered is invalid.");
            }

            if (string.IsNullOrEmpty(question) || question.Length < 3)
            {
                throw new ArgumentException("Your security question must be at least three characters.");
            }

            if (string.IsNullOrEmpty(answer) || answer.Length < 3)
            {
                throw new ArgumentException("Your security answer must be at least three characters.");
            }

            if (!gender.Equals('M') && !gender.Equals('F'))
            {
                throw new ArgumentException("Your gender must be 'M' or 'F'.");
            }

            if (whiteSpace.IsMatch(username))
            {
                throw new ArgumentException("Your username may not contain any whitespace.");
            }

            if (username.Contains("[") || username.Contains("]"))
            {
                throw new ArgumentException("Your username may not contain square brackets. (They will interfere with BBCode.)");
            }

            var passwordHash = ComputeHash(password);

            var existingUsers = from u in this.Db.Users
                                where u.Username == username || u.Email == email
                                select u;

            if (existingUsers.Count() > 0)
            {
                throw new UserAlreadyRegisteredException("There is already a user registered using that username or email address.");
            }

            var totalUsers = this.Db.Users.Count();

            var newUser = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Email = email,
                SecurityQuestion = question,
                SecurityAnswer = answer,
                Gender = gender,
                ShowEmail = showEmail,
                ShowGender = showGender,
                IsEmailConfirmed = false,
                CreateDate = DateTime.UtcNow,
                ReceiveAdminEmail = true,
            };

            if (!newUser.IsValid)
            {
                throw new InvalidOperationException("Blah!");
            }

            this.Db.Users.InsertOnSubmit(newUser);

            foreach (var role in from r in this.Db.Roles
                                 where r.IsDefault || (totalUsers == 0 && r.IsAdministrator)
                                 select r)
            {
                this.Db.UsersRoles.InsertOnSubmit(new UsersRole
                {
                    User = newUser,
                    Role = role
                });
            }

            this.Db.SubmitChanges();

            this.AppManager.SendConfirmationEmail(email, request, response);

            return newUser;
        }

        protected User RecallUser(string key, string originatingHostIp)
        {
            Guid keyId;
            try
            {
                keyId = new Guid(key);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            try
            {
                this.Db.AutoLogins.DeleteAllOnSubmit(from al in this.Db.AutoLogins
                                                     where al.ExpirationDate < DateTime.UtcNow
                                                     select al);
                this.Db.SubmitChanges();

                var a = (from al in this.Db.AutoLogins
                         where al.Key == keyId
                         select al).SingleOrDefault();

                return this.Login(a.UserID, originatingHostIp);
            }
            catch
            {
                return null;
            }
        }

        #region New View() implementations.
        protected new ViewResult View()
        {
            ViewBag.Controller = this;
            return base.View();
        }

        protected new ViewResult View(IView view)
        {
            ViewBag.Controller = this;
            return base.View(view);
        }

        protected new ViewResult View(object model)
        {
            ViewBag.Controller = this;
            return base.View(model);
        }

        protected new ViewResult View(string viewName)
        {
            ViewBag.Controller = this;
            return base.View(viewName);
        }

        protected new ViewResult View(IView view, object model)
        {
            ViewBag.Controller = this;
            return base.View(view, model);
        }

        protected new ViewResult View(string viewName, object model)
        {
            ViewBag.Controller = this;
            return base.View(viewName, model);
        }

        protected new ViewResult View(string viewName, string masterName)
        {
            ViewBag.Controller = this;
            return base.View(viewName, masterName);
        }

        protected new ViewResult View(string viewName, string masterName, object model)
        {
            ViewBag.Controller = this;
            return base.View(viewName, masterName, model);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Config != null)
                {
                    this.Config.Dispose();
                    this.Config = null;
                }
                if (this.Db != null)
                {
                    this.Db.Dispose();
                    this.Db = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            this.ValidateRequest = false;
            base.OnAuthorization(filterContext);
        }

        private User Login(long userId, string originatingHostIp)
        {
            if (!this.IsLogOnAttemptUnderThreshold(userId) || !this.IsLogOnAttemptUnderThreshold(originatingHostIp))
            {
                throw new HostLockedOutException("You have been locked out due to too many failed login attempts.  Please try again later.");
            }

            var user = this.GetUser(userId);

            if (user == null)
            {
                throw new UserNotFoundException("A user with that UserID could not be found in the database.");
            }

            if (this.Config.ConfirmEmail && !user.IsEmailConfirmed)
            {
                throw new EmailNotConfirmedException(
                    "You must confirm your email before you may log in to this site.");
            }

            return user;
        }

        private User LoadUser()
        {
            var user = (User)Session[UserKey];

            if (user == null)
            {
                var autoLogOnCookie = this.Request.Cookies[AutoLogOnKey];

                if (autoLogOnCookie != null && !string.IsNullOrEmpty(autoLogOnCookie.Value))
                {
                    user = this.RecallUser(autoLogOnCookie.Value, Request.UserHostAddress);

                    if (user == null)
                    {
                        Response.Cookies[AutoLogOnKey].Value = string.Empty;
                        Response.Cookies[AutoLogOnKey].Expires = DateTime.Now.AddDays(-10);
                    }
                    else
                    {
                        Trace.WriteLine("Session Miss: " + UserKey);
                    }
                }

                Session[UserKey] = user;
            }
            else
            {
                Trace.WriteLine("Session Hit: " + UserKey);
            }

            return user;
        }
    }
}
