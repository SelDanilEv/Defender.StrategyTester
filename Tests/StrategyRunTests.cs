using Defender.StrategyTester.Strategies;
using Defender.StrategyTester.Domain;

namespace Defender.StrategyTester.Tests
{
    [TestFixture]
    public class StrategyRunTests
    {
        [Test]
        public void Exchange_USDToPLN_UpdatesBalancesCorrectly()
        {
            // Arrange
            var initialBalance = new Dictionary<Currency, decimal>
            {
                { Currency.USD, 1000 },
                { Currency.PLN, 0 }
            };
            var run = new StrategyRun
            {
                Balance = new Dictionary<Currency, decimal>(initialBalance)
            };
            decimal amountToExchange = 100;
            var rate = new HistoricalData
            {
                Currency1 = Currency.USD,
                Currency2 = Currency.PLN,
                Rate = 4.0m // 1 USD = 4 PLN
            };

            // Act
            run.Exchange(Currency.USD, Currency.PLN, amountToExchange, rate);

            // Assert
            Assert.AreEqual(900, run.Balance[Currency.USD]);
            Assert.AreEqual(400, run.Balance[Currency.PLN]);
        }
        [Test]

        public void Exchange_USDToPLN_UpdatesBalancesCorrectlyWithInversedRate()
        {
            // Arrange
            var initialBalance = new Dictionary<Currency, decimal>
            {
                { Currency.USD, 1000 },
                { Currency.PLN, 0 }
            };
            var run = new StrategyRun
            {
                Balance = new Dictionary<Currency, decimal>(initialBalance)
            };
            decimal amountToExchange = 100;
            var rate = new HistoricalData
            {
                Currency1 = Currency.PLN,
                Currency2 = Currency.USD,
                Rate = 0.25m // 1 USD = 4 PLN
            };

            // Act
            run.Exchange(Currency.USD, Currency.PLN, amountToExchange, rate);

            // Assert
            Assert.AreEqual(900, run.Balance[Currency.USD]);
            Assert.AreEqual(400, run.Balance[Currency.PLN]);
        }

        [Test]
        public void Exchange_PLNToUSD_UpdatesBalancesCorrectly()
        {
            // Arrange
            var initialBalance = new Dictionary<Currency, decimal>
            {
                { Currency.USD, 0 },
                { Currency.PLN, 1000 }
            };
            var run = new StrategyRun
            {
                Balance = new Dictionary<Currency, decimal>(initialBalance)
            };
            decimal amountToExchange = 400;
            var rate = new HistoricalData
            {
                Currency1 = Currency.PLN,
                Currency2 = Currency.USD,
                Rate = 0.25m // 1 PLN = 0.25 USD
            };

            // Act
            run.Exchange(Currency.PLN, Currency.USD, amountToExchange, rate);

            // Assert
            Assert.AreEqual(100, run.Balance[Currency.USD]);
            Assert.AreEqual(600, run.Balance[Currency.PLN]);
        }

        [Test]
        public void Exchange_ZeroAmount_NoChangeInBalances()
        {
            // Arrange
            var initialBalance = new Dictionary<Currency, decimal>
            {
                { Currency.USD, 1000 },
                { Currency.PLN, 1000 }
            };
            var run = new StrategyRun
            {
                Balance = new Dictionary<Currency, decimal>(initialBalance)
            };
            decimal amountToExchange = 0;
            var rate = new HistoricalData
            {
                Currency1 = Currency.USD,
                Currency2 = Currency.PLN,
                Rate = 4.0m // 1 USD = 4 PLN
            };

            // Act
            run.Exchange(Currency.USD, Currency.PLN, amountToExchange, rate);

            // Assert
            Assert.AreEqual(1000, run.Balance[Currency.USD]);
            Assert.AreEqual(1000, run.Balance[Currency.PLN]);
        }

        [Test]
        public void Exchange_NegativeAmount_NoChangeInBalances()
        {
            // Arrange
            var initialBalance = new Dictionary<Currency, decimal>
            {
                { Currency.USD, 1000 },
                { Currency.PLN, 1000 }
            };
            var run = new StrategyRun
            {
                Balance = new Dictionary<Currency, decimal>(initialBalance)
            };
            decimal amountToExchange = -100;
            var rate = new HistoricalData
            {
                Currency1 = Currency.USD,
                Currency2 = Currency.PLN,
                Rate = 4.0m // 1 USD = 4 PLN
            };

            // Act
            run.Exchange(Currency.USD, Currency.PLN, amountToExchange, rate);

            // Assert
            Assert.AreEqual(1000, run.Balance[Currency.USD]);
            Assert.AreEqual(1000, run.Balance[Currency.PLN]);
        }
    }
}
