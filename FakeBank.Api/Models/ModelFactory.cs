using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using FakeBank.Data.Entities;

namespace FakeBank.Api.Models
{
    public class ModelFactory
    {
        private UrlHelper _urlHelper;
        private ApplicationUserManager _appUserManager;

        public ModelFactory(HttpRequestMessage request, ApplicationUserManager appUserManager)
        {
            _urlHelper = new UrlHelper(request);
            _appUserManager = appUserManager;
        }

        public List<TransactionsModel> Create(List<Transaction> transactions)
        {
            return transactions.Select(t => new TransactionsModel
            {
                Id = t.Id,
                Date = t.Date,
                IdSourceAccount = t.IdSourceAccount,
                IdDestinationAccount = t.IdDestinationAccount,
                Amount = t.Amount
            }).ToList();
        }

        public List<AccountModel> Create(List<Account> accounts)
        {
            return accounts.Select(a => new AccountModel
            {
                Id = a.Id,
                BeginDate = a.BeginDate,
                Balance = a.Balance,
                Active = a.Active,
                CardNumber = a.Card.CardNumber
            }).ToList();
        }

        public AccountModel Create(Account account)
        {
            return new AccountModel
            {
                Id = account.Id,
                BeginDate = account.BeginDate,
                Balance = account.Balance,
                Active = account.Active
            };
        }

        public UserModel Create(AspNetUser user)
        {
            return new UserModel
            {
                Id = user.Id,
                FirstSurname = user.FirstSurname,
                SecondSurname = user.SecondSurname,
                Rfc = user.Rfc,
                BirthDate = user.BirthDate,
                Active = user.Active,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
        }

        public List<PreRegistrationModel> Create(List<PreRegistration> preRegistrations)
        {
            return preRegistrations.Select(p => new PreRegistrationModel
            {
                Id = p.Id.ToString(),
                UserName = p.UserName,
                Email = p.Email,
                AccountType = p.AccountType.AccountType1,
                FirstSurname = p.FirstSurname,
                SecondSurname = p.SecondSurname
            }).ToList();
        }

        public CardModel Create(Card card)
        {
            return new CardModel
            {
                Id = card.Id.ToString(),
                Nip = card.Nip,
                ExpirationDate = card.ExpirationDate,
                SecurityCode = card.SecurityCode,
                CardType = card.CardType,
                CardNumber = card.CardNumber,
                Active = card.Active
            };
        }

        public PreRegistrationModel Create(PreRegistration preRegistration)
        {
            return new PreRegistrationModel
            {
                Id = preRegistration.Id.ToString(),
                AccountType = preRegistration.AccountType.AccountType1,
                UserName = preRegistration.UserName,
                FirstSurname = preRegistration.FirstSurname,
                SecondSurname = preRegistration.SecondSurname,
                Email = preRegistration.Email
            };
        }

        public TokenModel Create(Token token)
        {
            return new TokenModel
            {
                Id = token.Id.ToString(),
                Token = token.Token1.ToString(),
                Active = token.Active
            };
        }

        public class TokenModel
        {
            public string Id { get; set; }
            public string Token { get; set; }
            public bool Active { get; set; }
        }

        public class CardModel
        {
            public string Id { get; set; }
            public string Nip { get; set; }
            public string ExpirationDate { get; set; }
            public int SecurityCode { get; set; }
            public int CardType { get; set; }
            public string CardNumber { get; set; }
            public bool Active { get; set; }
        }

        public class UserModel
        {
            public string Id { get; set; }
            public string FirstSurname { get; set; }
            public string SecondSurname { get; set; }
            public string Rfc { get; set; }
            public DateTime BirthDate { get; set; }
            public bool Active { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string UserName { get; set; }
        }

        public class PreRegistrationModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public bool AccountType { get; set; }
            public string FirstSurname { get; set; }
            public string SecondSurname { get; set; }
        }

        public class TransactionsModel
        {
            public Guid Id { get; set; }
            public DateTime Date { get; set; }
            public Guid IdSourceAccount { get; set; }
            public Guid IdDestinationAccount { get; set; }
            public double Amount { get; set; }
        }

        public class AccountModel
        {
            public Guid Id { get; set; }
            public string CardNumber { get; set; }
            public DateTime BeginDate { get; set; }
            public double Balance { get; set; }
            public bool Active { get; set; }
        }
    }
}