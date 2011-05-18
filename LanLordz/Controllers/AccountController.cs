//-----------------------------------------------------------------------
// <copyright file="AccountController.cs" company="LAN Lordz, inc.">
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
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using LanLordz.Models;
    using LanLordz.SiteTools;

    public class AccountController : LanLordzBaseController
    {
        [CompressFilter]
        public ActionResult Index()
        {
            return View();
        }

        [CompressFilter]
        public ActionResult LogOn()
        {
            if (CurrentUser != null)
            {
                RedirectToAction("ViewProfile", new { id = CurrentUser.UserID });
            }

            return View("LogOn", new LogOnAttemptModel());
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult LogOn(FormCollection values)
        {
            if (CurrentUser != null)
            {
                RedirectToAction("ViewProfile", new { id = CurrentUser.UserID });
            }

            var ip = Request.UserHostAddress;

            var hasFailedTooMuch = !IsLogOnAttemptUnderThreshold(ip);

            var attempt = new LogOnAttemptModel
            {
                ReturnUrl = values["ReturnUrl"],
                Username = values["Username"],
                Password = values["Password"],
                RememberMe = values["RememberMe"] == "on",
                RequireCaptcha = hasFailedTooMuch && this.Config.UseRecaptcha,
            };

            var user = GetUser(values["Username"]);

            if (user == null)
            {
                ModelState.AddModelError("Form", "A user with the specified credentials could not be found in the database.");
                return View("LogOn", attempt);
            }

            hasFailedTooMuch = hasFailedTooMuch || !IsLogOnAttemptUnderThreshold(user.UserID);
            attempt.RequireCaptcha = hasFailedTooMuch && this.Config.UseRecaptcha;

            // TODO:  If we have failed to much, we need to reject the logon attempt if the last attempt happened within ten seconds.

            if (hasFailedTooMuch && !attempt.RequireCaptcha)
            {
                ModelState.AddModelError("Form", "You have been locked out due to too many failed login attempts.  Please try again later.");
                return View("LogOn", attempt);
            }
            else if (hasFailedTooMuch && attempt.RequireCaptcha && !this.IsCaptchaValid(this.Config.RecaptchaPrivateKey))
            {
                ModelState.AddModelError("Form", "You must use complete the captcha correctly to continue.");
                return View("LogOn", attempt);
            }

            string passwordHash = ComputeHash(attempt.Password);

            if (user.PasswordHash != passwordHash)
            {
                AddAuthFailure(user.UserID, ip);
                hasFailedTooMuch = !IsLogOnAttemptUnderThreshold(ip) || !IsLogOnAttemptUnderThreshold(user.UserID);
                attempt.RequireCaptcha = hasFailedTooMuch && this.Config.UseRecaptcha;

                ModelState.AddModelError("Form", "A user with the specified credentials could not be found in the database.");
                return View("LogOn", attempt);
            }

            if (this.Config.ConfirmEmail && !user.IsEmailConfirmed)
            {
                ModelState.AddModelError("Form", "You must confirm your email before you may log in to this site.");
                return View("LogOn", attempt);
            }

            this.CurrentUser = user;

            if (string.IsNullOrEmpty(attempt.ReturnUrl))
            {
                attempt.ReturnUrl = Url.Action("ViewProfile", new { id = user.UserID });
            }

            if (attempt.RememberMe)
            {
                string key = AppManager.RememberUser(this.CurrentUser);
                Response.Cookies["AutoLogOnKey"].Value = key;
                Response.Cookies["AutoLogOnKey"].Expires = DateTime.Now.AddDays(10);
                Response.Cookies["AutoLogOnKey"].HttpOnly = true;
            }

            return Redirect(attempt.ReturnUrl);
        }

        [CompressFilter]
        public ActionResult SearchUsers(string q)
        {
            var users = from u in this.Db.Users
                        where u.Username.Contains(q)
                        select new { userId = u.UserID, username = u.Username };

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult LogOff(FormCollection values)
        {
            string returnUrl = values["ReturnUrl"];

            if (this.Request.Cookies["AutoLogOnKey"] != null && !string.IsNullOrEmpty(this.Request.Cookies["AutoLogOnKey"].Value))
            {
                this.AppManager.ForgetUser(this.Request.Cookies["AutoLogOnKey"].Value);
            }

            Response.Cookies["AutoLogOnKey"].Value = "";
            Response.Cookies["AutoLogOnKey"].Expires = DateTime.Now.AddDays(-10);
            Response.Cookies["AutoLogOnKey"].HttpOnly = true;

            this.CurrentUser = null;

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }

            return Redirect(returnUrl);
        }

        [CompressFilter]
        public ActionResult Register()
        {
            if (CurrentUser != null)
            {
                RedirectToAction("ViewProfile", new { id = CurrentUser.UserID });
            }

            return View("Register", new RegistrationModel());
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Register(FormCollection values)
        {
            if (CurrentUser != null)
            {
                RedirectToAction("ViewProfile", new { id = CurrentUser.UserID });
            }

            var registration = new RegistrationModel
            {
                Username = values["Username"],
                Password = values["Password"],
                PasswordConfirm = values["PasswordConfirm"],
                Email = values["Email"],
                SecurityQuestion = values["SecurityQuestion"],
                SecurityAnswer = values["SecurityAnswer"],
                Gender = values["Gender"] != null ? values["Gender"][0] : '\0',
                ShowEmail = values["ShowEmail"] == "on",
                ShowGender = values["ShowGender"] == "on",
            };

            if (!registration.IsValid)
            {
                foreach (var violation in registration.GetRuleViolations())
                {
                    this.ModelState.AddModelError(violation.PropertyName, violation.ErrorMessage);
                }

                return View("Register", registration);
            }

            if (this.Config.UseRecaptcha && !this.IsCaptchaValid(Config.RecaptchaPrivateKey))
            {
                this.ModelState.AddModelError("Captcha", "Captcha validation failed.");

                return View("Register", registration);
            }

            User newlyRegisteredUser;

            try
            {
                newlyRegisteredUser = this.Register(
                    registration.Username,
                    registration.Password,
                    registration.Email,
                    registration.SecurityQuestion,
                    registration.SecurityAnswer,
                    registration.Gender,
                    registration.ShowEmail,
                    registration.ShowGender,
                    Request,
                    Response);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("Form", ex.Message);
                return View("Register", registration);
            }

            if (!this.Config.ConfirmEmail && this.Config.LoginAfterRegister)
            {
                this.CurrentUser = newlyRegisteredUser;
            }

            return View("RegistrationSuccessful");
        }

        [CompressFilter]
        public ActionResult Confirm(string key)
        {
            return View("ConfirmEmail", new ConfirmEmailModel
            {
                Key = key
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult Confirm(FormCollection values)
        {
            var key = values["Key"];

            if (this.AppManager.ConfirmKey(key))
            {
                return View("ConfirmSuccess");
            }
            else
            {
                this.ModelState.AddModelError("Form", "That key could not be found.");
                return View("ConfirmEmail", new ConfirmEmailModel
                {
                    Key = key
                });
            }
        }

        [CompressFilter]
        public ActionResult ViewProfile(long id)
        {
            var u = this.AppManager.GetUserInformation(id, false);
            return View("ViewProfile", new UserInformationModel
            {
                UserInfo = u
            });
        }

        [CompressFilter]
        public ActionResult EditProfile()
        {
            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            return View("EditProfile", new EditProfileModel
            {
                UserInfo = this.AppManager.GetUserInformation(CurrentUser.UserID, true),
                AvailableThemes = this.AppManager.GetAvailableThemes(true),
                AvailableTimezones = this.AppManager.GetAvailableTimezones(true)
            });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult EditProfile(FormCollection values)
        {
            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            UserAttributesSet attrs = new UserAttributesSet();
            UpdateModel(attrs);

            if (!attrs.IsValid)
            {
                foreach (var violation in attrs.GetRuleViolations())
                {
                    this.ModelState.AddModelError(violation.PropertyName, violation.ErrorMessage);
                }

                UserInformation info = this.AppManager.GetUserInformation(CurrentUser.UserID, true);
                info.Biography = attrs.Biography;
                info.Interests = attrs.Interests;
                info.Location = attrs.Location;
                info.Occupation = attrs.Occupation;
                info.Signature = attrs.Signature;
                info.Theme = attrs.Theme;
                info.TimeZone = attrs.TimeZone;
                info.Website = attrs.Website;
                info.ShowEmail = (values["ShowEmail"] == "on");
                info.ShowGender = (values["ShowGender"] == "on");
                info.ReceiveAdminEmail = (values["ReceiveAdminEmail"] == "on");

                return View("EditProfile", new EditProfileModel
                {
                    UserInfo = info,
                    AvailableThemes = this.AppManager.GetAvailableThemes(true),
                    AvailableTimezones = this.AppManager.GetAvailableTimezones(true)
                });
            }
            else
            {
                User u = this.GetUser(CurrentUser.UserID);
                u.ShowEmail = (values["ShowEmail"] == "on");
                u.ShowGender = (values["ShowGender"] == "on");
                u.ReceiveAdminEmail = (values["ReceiveAdminEmail"] == "on");

                this.Db.SubmitChanges();

                long userId = u.UserID;
                AppManager.SetUserConfigProperty("Location", userId, attrs.Location);
                AppManager.SetUserConfigProperty("Website", userId, attrs.Website);
                AppManager.SetUserConfigProperty("Occupation", userId, attrs.Occupation);
                AppManager.SetUserConfigProperty("Interests", userId, attrs.Interests);
                AppManager.SetUserConfigProperty("Signature", userId, attrs.Signature);
                AppManager.SetUserConfigProperty("Biography", userId, attrs.Biography);
                AppManager.SetUserConfigProperty("Theme", userId, attrs.Theme);
                AppManager.SetUserConfigProperty("TimeZone", userId, attrs.TimeZone);

                UserInformation info = this.AppManager.GetUserInformation(u.UserID, true);

                return RedirectToAction("ViewProfile", new { id = CurrentUser.UserID });
            }
        }

        [CompressFilter]
        public ActionResult ChangeAvatar()
        {
            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            var info = this.AppManager.GetUserInformation(CurrentUser.UserID, false);

            return View("ChangeAvatar", new ChangeAvatarModel
            {
                UserInfo = info
            });
        }
        
        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult ChangeAvatar(FormCollection values)
        {
            if (CurrentUser == null)
            {
                return View("NotAuthorized");
            }

            if (Request.Files["NewAvatar"] == null)
            {
                this.ModelState.AddModelError("Form", "You must choose a file.");
                var info = this.AppManager.GetUserInformation(CurrentUser.UserID, false);
                return View("ChangeAvatar", new ChangeAvatarModel
                {
                    UserInfo = info
                });
            }
            else
            {
                HttpPostedFileBase hpf = Request.Files["NewAvatar"];
                if (hpf.ContentLength == 0)
                {
                    this.ModelState.AddModelError("Form", "The file you have chosen is empty.");
                    var info = this.AppManager.GetUserInformation(CurrentUser.UserID, false);
                    return View("ChangeAvatar", new ChangeAvatarModel
                    {
                        UserInfo = info
                    });
                }
                else
                {
                    var violations = new List<RuleViolation>(this.CheckAvatar(hpf.InputStream));
                    if (violations.Count > 0)
                    {
                        foreach (var violation in violations)
                        {
                            this.ModelState.AddModelError("Form", violation.ErrorMessage);
                        }
                        var info = this.AppManager.GetUserInformation(CurrentUser.UserID, false);
                        return View("ChangeAvatar", new ChangeAvatarModel
                        {
                            UserInfo = info
                        });
                    }
                    else
                    {
                        byte[] newAvatar = new byte[hpf.InputStream.Length];
                        hpf.InputStream.Seek(0, SeekOrigin.Begin);
                        hpf.InputStream.Read(newAvatar, 0, newAvatar.Length);
                        this.AppManager.UpdateUserAvatar(CurrentUser.UserID, newAvatar);
                    }
                }
            }

            return RedirectToAction("ViewProfile", new { id = CurrentUser.UserID });
        }

        private IEnumerable<RuleViolation> CheckAvatar(Stream avatarStream)
        {
            System.Drawing.Image i = null;

            if (avatarStream.Length > 153600)
            {
                yield return new RuleViolation("The file you uploaded was too large. (Max 150KB)");
            }

            var valid = false;
            try
            {
                i = System.Drawing.Image.FromStream(avatarStream);
                valid = true;
            }
            catch (ArgumentException)
            {
            }

            if (!valid)
            {
                yield return new RuleViolation("The file you uploaded was not a valid image.");
            }
            else if (i.Width > 100 || i.Height > 100)
            {
                yield return new RuleViolation("The image you uploaded was too large. (Max 100x100)");
            }

            yield break;
        }

        [CompressFilter]
        public ActionResult ChangePassword(long? id)
        {
            var ip = Request.UserHostAddress;

            var hasFailedTooMuch = !IsLogOnAttemptUnderThreshold(ip);

            var u = this.CurrentUser;

            if (u == null && id.HasValue)
            {
                u = this.AppManager.GetUserByUserID(id.Value);

                if (u == null)
                {
                    this.ModelState.AddModelError("Username", "The user you specified could not be found.");
                }
            }

            if (u != null)
            {
                hasFailedTooMuch = hasFailedTooMuch || !IsLogOnAttemptUnderThreshold(u.UserID);
            }

            return View(
                "ChangePassword",
                new ChangePasswordModel
                {
                    User = u,
                    RequireCaptcha = hasFailedTooMuch && this.Config.UseRecaptcha,
                });
        }

        [HttpPost, ValidateAntiForgeryToken, CompressFilter]
        public ActionResult ChangePassword(long? id, FormCollection values)
        {
            var ip = Request.UserHostAddress;

            var hasFailedTooMuch = !IsLogOnAttemptUnderThreshold(ip);

            var u = this.CurrentUser;
            
            var cpm = new ChangePasswordModel
            {
                User = u,
                RequireCaptcha = hasFailedTooMuch && this.Config.UseRecaptcha
            };

            if (u != null || id.HasValue)
            {
                if (u == null)
                {
                    u = this.AppManager.GetUserByUserID(id.Value);

                    if (u == null)
                    {
                        this.ModelState.AddModelError("Username", "The user you specified could not be found.");
                        return View("ChangePassword", cpm);
                    }
                }

                hasFailedTooMuch = hasFailedTooMuch || !this.IsLogOnAttemptUnderThreshold(u.UserID);
                cpm.RequireCaptcha = hasFailedTooMuch && this.Config.UseRecaptcha;

                if (hasFailedTooMuch && !cpm.RequireCaptcha)
                {
                    this.ModelState.AddModelError("Username", "You have been locked out due to too many failed attempts.");
                    return View("ChangePassword", cpm);
                }

                if (hasFailedTooMuch && !this.IsCaptchaValid(this.Config.RecaptchaPrivateKey))
                {
                    this.ModelState.AddModelError("Username", "You must complete the captcha correctly to continue.");
                    return View("ChangePassword", cpm);
                }

                if (this.CurrentUser != null)
                {
                    var hash = LanLordzBaseController.ComputeHash(values["CurrentPassword"]);

                    if (!string.Equals(u.PasswordHash, hash))
                    {
                        this.AddAuthFailure(u.UserID, ip);

                        this.ModelState.AddModelError("Username", "The current password you provided was incorrect.");
                        return View("ChangePassword", cpm);
                    }
                }
                else if (u.IsEmailConfirmed)
                {
                    var key = (from s in this.Db.SecurityKeys
                               where s.UserID == u.UserID
                               where s.ExpirationDate > DateTime.UtcNow
                               select s).SingleOrDefault();

                    if (key == null)
                    {
                        this.ModelState.AddModelError("Username", "Your security key has expired.  Please try again.");
                        return View("ChangePassword", cpm);
                    }

                    var keyGuid = new Guid();
                    bool keyFound;
                    try
                    {
                        keyGuid = new Guid(values["SecurityKey"]);
                        keyFound = true;
                    }
                    catch (FormatException)
                    {
                        keyFound = false;
                    }

                    if (!keyFound || keyGuid != key.Key)
                    {
                        this.ModelState.AddModelError("SecurityKey", "The security key you specified was either not valid or incorrect.");
                        return View("ChangePassword", cpm);
                    }
                }

                if (!string.Equals(u.SecurityAnswer, values["SecurityAnswer"], StringComparison.CurrentCultureIgnoreCase))
                {
                    this.AddAuthFailure(u.UserID, ip);

                    this.ModelState.AddModelError("Username", "The security answer you provided did not match the answer on file.");
                    return View("ChangePassword", cpm);
                }

                if (!string.Equals(values["Password"], values["ConfirmPassword"]))
                {
                    this.ModelState.AddModelError("Username", "The new passwords you typed did not match.");
                    return View("ChangePassword", cpm);
                }

                if (string.IsNullOrEmpty(values["Password"]) || values["Password"].Length < 6)
                {
                    this.ModelState.AddModelError("Username", "Your new password must be at least 6 characters long.");
                    return View("ChangePassword", cpm);
                }

                this.AppManager.UpdateUserPassword(u.UserID, values["Password"]);

                return View("ChangeSucceeded");
            }
            else
            {
                if (string.IsNullOrEmpty(values["Username"]))
                {
                    this.ModelState.AddModelError("Username", "You must specify a username.");
                }
                else
                {
                    u = this.GetUser(values["Username"]);
                    cpm = new ChangePasswordModel
                    {
                        User = u
                    };

                    if (u == null)
                    {
                        this.ModelState.AddModelError("Username", "The username you specified could not be found.");
                    }
                    else
                    {
                        if (u.IsEmailConfirmed)
                        {
                            this.AppManager.CreateSecurityChangeKey(u);
                        }

                        return RedirectToAction("ChangePassword", new { id = u.UserID });
                    }
                }

                return View("ChangePassword", cpm);
            }
        }
    }
}
