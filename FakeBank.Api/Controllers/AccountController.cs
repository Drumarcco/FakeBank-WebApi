using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.ModelBinding;
using FakeBank.Api.Models;
using FakeBank.Controllers;
using FakeBank.Data.Business.Repositories;
using FakeBank.Data.Business.Services;
using FakeBank.Data.Entities;
using FakeBank.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace FakeBank.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : BaseAPIController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }


        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
        
        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginBindingModel model)
        {
            try
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    var identity = await user.GenerateUserIdentityAsync(UserManager, "JWT");
                    Authentication.SignIn(identity);
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return new InternalServerErrorResult(this);
            }
        }

        [HttpGet]
        [Authorize(Roles = "employee,moralperson,physicalperson")]
        [Route("GetAll")]
        public IHttpActionResult GetuserByIdAccount()
        {
            var accountService = new AccountService();
            var userId = User.Identity.GetUserId();
            var accounts = accountService.GetAllByUserId(userId);
            return (accounts != null) ? (IHttpActionResult) Ok(TheModelFactory.Create(accounts)) : NotFound();
        }

        [HttpGet]
        [Authorize(Roles = "employee, moralperson, physicalperson")]
        [Route("Token/{accountId}")]
        public IHttpActionResult GetAccountToken(string accountId)
        {
            var tokenService = new TokenService();
            var accountService = new AccountService();
            var userId = User.Identity.GetUserId();
            var account = accountService.GetById(accountId);
            if (account.IdCustomer != userId) return BadRequest("La cuenta no pertenece al usuario loggeado.");
            var token = tokenService.GetById(account.Id);
            return (token != null) ? (IHttpActionResult) Ok(TheModelFactory.Create(token)) : NotFound();
        }

        [HttpGet]
        [Authorize(Roles = "employee, moralperson, physicalperson")]
        [Route("Balance/{accountId}")]
        public IHttpActionResult GetBalanceByCardNumber(string accountId)
        {
            var accountService = new AccountService();
            var userId = User.Identity.GetUserId();
            var account = accountService.GetByAccountId(accountId);
            return (account != null) ? (IHttpActionResult) Ok(TheModelFactory.Create(account)) : NotFound();
        }

        [HttpGet]
        [Authorize(Roles = "employee,moralperson,physicalperson")]
        [Route("GetCard/{accountId}")]
        public IHttpActionResult GetCardByAccountId(string accountId)
        {
            var cardService = new CardService();
            var card = cardService.GetByAccountId(accountId);
            return (card != null) ? (IHttpActionResult) Ok(TheModelFactory.Create(card)) : NotFound();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Authorize(Roles = "employee")]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var preRegistrationService = new PreRegistrationService();
            var preRegistration = preRegistrationService.GetById(model.IdPreRegistration);
            var user = new ApplicationUser
            {
                UserName = preRegistration.UserName,
                Email = preRegistration.Email,
                PhoneNumber = preRegistration.PhoneNumber,
                FirstSurname = preRegistration.FirstSurname,
                SecondSurname = preRegistration.SecondSurname,
                Gender = model.Gender,
                Rfc = model.Rfc,
                BirthDate = model.BirthDate
            };
            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return GetErrorResult(result);

            var modified = preRegistrationService.Active(preRegistration, false);
            if (!modified) return InternalServerError();

            var accountTypeService = new AccountTypeService();

            var cardService = new CardService();
            var accountService = new AccountService();
            var tokenService = new TokenService();

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Nip = new Random().Next(0000, 9999).ToString(),
                ExpirationDate = DateTime.Now.Date.AddYears(3).ToString("MM/yy"),
                SecurityCode = new Random().Next(000, 999),
                CardType = 1,
                CardNumber = Data.ExtensionMethods.Generate16DigitString(),
                Active = true
            };
            var account = new Account
            {
                Id = card.Id,
                BeginDate = DateTime.Now,
                Balance = 5000,
                IdAccountType = preRegistration.IdAccountType,
                IdCustomer = user.Id,
                Active = true
            };
            var accountType = accountTypeService.GetById(preRegistration.IdAccountType);
            if (accountType.AccountType1)
            {
                var roleService = new RoleService();
                var role = roleService.GetByName("moralperson");
                if (role == null)
                {
                    var roleIdentity = new IdentityRole("moralperson");
                    var aspNetRole = new AspNetRole { Id = roleIdentity.Id, Name = "moralperson" };

                    var saved = roleService.Save(aspNetRole);
                    if (!saved) return BadRequest();
                    var roleResult = UserManager.AddToRole(user.Id, aspNetRole.Name);
                    if (!roleResult.Succeeded) return BadRequest();

                    var token = new Token
                    {
                        Id = account.Id,
                        Token1 = Guid.NewGuid(),
                        Active = true
                    };
                    var cardResult = cardService.Save(card);
                    var accountResult = accountService.Save(account);
                    var tokenResult = tokenService.Save(token);
                    return (cardResult && accountResult && tokenResult) ? (IHttpActionResult)Ok() : InternalServerError();
                }
                else
                {
                    var roleResult = UserManager.AddToRole(user.Id, role.Name);
                    if (!roleResult.Succeeded) return InternalServerError();
                    var token = new Token
                    {
                        Id = account.Id,
                        Token1 = Guid.NewGuid(),
                        Active = true
                    };
                    var cardResult = cardService.Save(card);
                    var accountResult = accountService.Save(account);
                    var tokenResult = tokenService.Save(token);
                    return (cardResult && accountResult && tokenResult) ? (IHttpActionResult)Ok() : InternalServerError();
                }
            }
            else
            {
                var roleService = new RoleService();
                var role = roleService.GetByName("physicalperson");
                if (role == null)
                {
                    var roleIndetity = new IdentityRole("physicalperson");
                    var aspNetRole = new AspNetRole { Id = roleIndetity.Id, Name = "physicalperson" };
                    var saved = roleService.Save(aspNetRole);
                    if (!saved) return BadRequest();
                    var roleResult = UserManager.AddToRole(user.Id, aspNetRole.Name);
                    if (!roleResult.Succeeded) return BadRequest();
                    var token = new Token
                    {
                        Id = account.Id,
                        Token1 = Guid.NewGuid(),
                        Active = true
                    };
                    var cardResult = cardService.Save(card);
                    var accountResult = accountService.Save(account);
                    var tokenResult = tokenService.Save(token);
                    return (cardResult && accountResult && tokenResult) ? (IHttpActionResult) Ok() : InternalServerError();
                }
                else
                {
                    var roleResult = UserManager.AddToRole(user.Id, role.Name);
                    if (!roleResult.Succeeded) return InternalServerError();
                    var token = new Token
                    {
                        Id = account.Id,
                        Token1 = Guid.NewGuid(),
                        Active = true
                    };
                    var cardResult = cardService.Save(card);
                    var accountResult = accountService.Save(account);
                    var tokenResult = tokenService.Save(token);
                    return(cardResult && accountResult && tokenResult) ? (IHttpActionResult)Ok() : InternalServerError();
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
        #endregion
    }
}
