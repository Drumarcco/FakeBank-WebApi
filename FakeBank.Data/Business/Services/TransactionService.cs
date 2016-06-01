using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeBank.Data.Business.Repositories;
using FakeBank.Data.Entities;

namespace FakeBank.Data.Business.Services
{
    public class TransactionService:IService<Transaction>
    {
        public bool Save(Transaction transaction)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var transactionRepository = new TransactionRepository(db);
                    db.Transactions.Attach(transaction);
                    transactionRepository.Insert(transaction);
                    return db.SaveChanges() >= 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Delete(Transaction transaction)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var transactionRepository = new TransactionRepository(db);
                    transactionRepository.Delete(transaction);
                    return db.SaveChanges() >= 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Transaction> GetByUserId(string id)
        {
            try
            {
                using (var db = new FAKE_BANKEntities())
                {
                    var transactionRepository = new TransactionRepository(db);
                    return transactionRepository.Search(u => u.Account.AspNetUser.Id == id).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Transaction> GetByAccountId(string id)
        {
            try
            {
                var Id = Guid.Parse(id);
                using (var db = new FAKE_BANKEntities())
                {
                    var transactionRepository = new TransactionRepository(db);
                    return transactionRepository.Search(
                        a => a.IdSourceAccount == Id,
                        a => a.OrderByDescending(transaction => transaction.Date),
                        a => a.Account.Card).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
