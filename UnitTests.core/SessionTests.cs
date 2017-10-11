using figo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.core
{
    public class SessionTests
    {
        private FigoSession sut = null;

        public SessionTests()
        {
            sut = new FigoSession { AccessToken = "ASHWLIkouP2O6_bgA2wWReRhletgWKHYjLqDaqb0LFfamim9RjexTo22ujRIP_cjLiRiSyQXyt2kM1eXU2XLFZQ0Hro15HikJQT_eNeT_9XQ" };
        }

        [Fact]
        public void testGetAccount()
        {
            Task<FigoAccount> task_a = sut.GetAccount("A1.2");
            FigoAccount a = task_a.Result;
            Assert.Equal("A1.2", a.AccountId);
            Assert.NotNull(a.Balance.Balance);
            Assert.NotNull(a.Balance.BalanceDate);

            Task<List<FigoTransaction>> task_b = sut.GetTransactions(a);
            task_b.Wait();
            List<FigoTransaction> ts = task_b.Result;
            Assert.True(ts.Count > 0);

            Task<List<FigoPayment>> task_c = sut.GetPayments(a);
            task_c.Wait();
            List<FigoPayment> ps = task_c.Result;
            Assert.True(ps.Count >= 0);
        }

        [Fact]
        public void testGetTransactions()
        {
            Task<List<FigoTransaction>> task_a = sut.GetTransactions();
            task_a.Wait();
            List<FigoTransaction> transactions = task_a.Result;
            Assert.True(transactions.Count > 0);
        }

        [Fact]
        public void testGetNotifications()
        {
            Task<List<FigoNotification>> task_a = sut.GetNotifications();
            task_a.Wait();
            List<FigoNotification> notifications = task_a.Result;
            Assert.True(notifications.Count > 0);
        }

        [Fact]
        public void testGetPayments()
        {
            Task<List<FigoPayment>> task_a = sut.GetPayments();
            task_a.Wait();
            List<FigoPayment> payments = task_a.Result;
            Assert.True(payments.Count > 0);
        }

        [Fact]
        public void testMissingHandling()
        {
            var task_a = sut.GetAccount("A1.5");
            task_a.Wait();
            Assert.Null(task_a.Result);
        }

        [Fact]
        public void testErrorHandling()
        {
            Assert.ThrowsAsync<FigoException>(async () =>
            await sut.GetSyncTaskToken("", "http://localhost:3003/"));
        }

        [Fact]
        public void testGetSync()
        {
            Task<string> task_a = sut.GetSyncTaskToken("test", "http://localhost:3000/callback");
            task_a.Wait();
            Assert.NotNull(task_a.Result);
        }

        [Fact]
        public void testUser()
        {
            Task<FigoUser> task_a = sut.GetUser();
            task_a.Wait();
            Assert.Equal("demo@figo.me", task_a.Result.Email);
        }

        [Fact]
        public void testCreateUpdateDeleteNotification()
        {
            FigoNotification notification = new FigoNotification { ObserveKey = "/rest/transactions", NotifyURI = "http://figo.me/test", State = "qwe" };
            Task<FigoNotification> task_add = sut.AddNotification(notification);
            task_add.Wait();
            FigoNotification addedNotification = task_add.Result;
            Assert.NotNull(addedNotification);
            Assert.NotNull(addedNotification.NotificationId);
            Assert.Equal("/rest/transactions", addedNotification.ObserveKey);
            Assert.Equal("http://figo.me/test", addedNotification.NotifyURI);
            Assert.Equal("qwe", addedNotification.State);

            addedNotification.State = "asd";
            Task<FigoNotification> task_update = sut.UpdateNotification(addedNotification);
            task_update.Wait();

            Task<FigoNotification> task_get = sut.GetNotification(addedNotification.NotificationId);
            task_get.Wait();
            FigoNotification updatedNotification = task_get.Result;
            Assert.NotNull(updatedNotification);
            Assert.Equal(addedNotification.NotificationId, updatedNotification.NotificationId);
            Assert.Equal("/rest/transactions", updatedNotification.ObserveKey);
            Assert.Equal("http://figo.me/test", updatedNotification.NotifyURI);
            Assert.Equal("asd", updatedNotification.State);

            Task<bool> task_delete = sut.RemoveNotification(updatedNotification);
            task_delete.Wait();

            Task<FigoNotification> task_test = sut.GetNotification(addedNotification.NotificationId);
            task_test.Wait();
            Assert.Null(task_test.Result);
        }

        [Fact]
        public void testCreateUpdateDeletePayment()
        {
            FigoPayment payment = new FigoPayment { Type = "Transfer", AccountNumber = "4711951501", BankCode = "90090042", Name = "figo", Purpose = "Thanks for all the fish.", Amount = 0.89F };
            Task<FigoPayment> task_add = sut.AddPayment("A1.1", payment);
            task_add.Wait();
            FigoPayment addedPayment = task_add.Result;
            Assert.NotNull(addedPayment);
            Assert.NotNull(addedPayment.PaymentId);
            Assert.Equal("A1.1", addedPayment.AccountId);
            Assert.Equal("Demobank", addedPayment.BankName);
            Assert.Equal(0.89F, addedPayment.Amount);

            addedPayment.Amount = 2.39F;
            Task<FigoPayment> task_update = sut.UpdatePayment(addedPayment);
            task_update.Wait();

            Task<FigoPayment> task_get = sut.GetPayment(addedPayment.AccountId, addedPayment.PaymentId);
            task_get.Wait();
            FigoPayment updatedPayment = task_get.Result;
            Assert.NotNull(updatedPayment);
            Assert.Equal(addedPayment.PaymentId, updatedPayment.PaymentId);
            Assert.Equal("A1.1", updatedPayment.AccountId);
            Assert.Equal("Demobank", updatedPayment.BankName);
            Assert.Equal(2.39F, updatedPayment.Amount);

            Task<bool> task_delete = sut.RemovePayment(updatedPayment);
            task_delete.Wait();

            Task<FigoPayment> task_test = sut.GetPayment(addedPayment.AccountId, addedPayment.PaymentId);
            task_test.Wait();
            Assert.Null(task_test.Result);
        }
    }
}
