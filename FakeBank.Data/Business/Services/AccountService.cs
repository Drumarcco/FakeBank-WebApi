using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeBank.Data.Business.Repositories;
using FakeBank.Data.Entities;

namespace FakeBank.Data.Business.Services
{
    public class AccountService : IService<Account>
    {
        public bool Save(Account account)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var accountRepository = new AccountRepository(db);
                    db.Accounts.Attach(account);
                    accountRepository.Insert(account);
                    return db.SaveChanges() >= 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Delete(Account account)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var accountRepository = new AccountRepository(db);
                    accountRepository.Delete(account);
                    return db.SaveChanges() >= 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Account GetByCardNumber(string cardNumber)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var accountRepository = new AccountRepository(db);
                    return accountRepository.SearchOne(a => a.Card.CardNumber.Equals(cardNumber));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Account GetByToken(Guid token)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var accountRepository = new AccountRepository(db);
                    return accountRepository.SearchOne(a => a.Token.Token1.Equals(token));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Account GetById(string id)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var Id = Guid.Parse(id);
                    var accountRepository = new AccountRepository(db);
                    return accountRepository.SearchOne(a => a.Id == Id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Account> GetAllByUserId(string id)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var accountRepository = new AccountRepository(db);
                    return accountRepository.Search(a => a.IdCustomer == id, null, account => account.Card);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Account GetByUserId(string id)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var accountRespository = new AccountRepository(db);
                    return accountRespository.SearchOne(a => a.IdCustomer == id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateBalance(Account accountSource, Account accountDestination, double amount)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var balanceSource = accountSource.Balance - amount;
                    var balanceDestination = accountDestination.Balance + amount;
                    db.Accounts.Attach(accountSource);
                    db.Accounts.Attach(accountDestination);
                    accountSource.Balance = balanceSource;
                    accountDestination.Balance = balanceDestination;
                    db.Entry(accountSource).Property(p => p.Balance).IsModified = true;
                    db.Entry(accountDestination).Property(p => p.Balance).IsModified = true;
                    return db.SaveChanges() >= 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public double GetBalanceByCardNumber(string cardNumber)
        {

            using (var db = new FAKE_BANKEntities())
            {
                return db.Accounts.Where(a => a.Card.CardNumber == cardNumber)
                    .Select(a => a.Balance).FirstOrDefault();
            }

        }

        public Account GetByAccountId(string accountId)
        {
            using (var db = new FAKE_BANKEntities())
            {
                return db.Accounts.FirstOrDefault(a => a.Id.ToString() == accountId);
            }
        }
    }
}
