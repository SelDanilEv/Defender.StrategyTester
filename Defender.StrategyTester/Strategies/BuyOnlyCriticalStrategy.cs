using Defender.StrategyTester.CVS;
using Defender.StrategyTester.Domain;

namespace Defender.StrategyTester.Strategies
{
    public class BuyOnlyCriticalStrategy : Strategy
    {
        public BuyOnlyCriticalStrategy() : base(
            nameof(BuyOnlyCriticalStrategy),
            new DateOnly(2014, 9, 21),
            new DateOnly(2024, 7, 21),
            new Dictionary<Currency, decimal>
            {
                { Currency.USD, 8000 },
                { Currency.PLN, 1500 },
            },
            new Dictionary<Currency, decimal>
            {
                { Currency.PLN, 2100 },
            },
            new Dictionary<Currency, decimal>
            {
                { Currency.USD, 5600 },
            })
        {
        }

        public override async Task<StrategyRun> Execute()
        {
            var run = new StrategyRun
            {
                Name = $"{Name}",
                CurrentDay = FromDate,
                LastDay = ToDate,
                Balance = new Dictionary<Currency, decimal>(InitialBalance),
                WeeklyExpensies = WeeklyExpensies,
                MonthlyIncome = MonthlyIncome,
            };
            do
            {
                if (run.Balance[Currency.PLN] < 0)
                {
                    var historicalRate =
                        await run.GetTodayRateAsync(Currency.USD, Currency.PLN);

                    var usdAmountToExchange = Math.Round(
                        -run.Balance[Currency.PLN] * historicalRate.Invert.Rate,
                        2, MidpointRounding.AwayFromZero);

                    run.Exchange(Currency.USD, Currency.PLN, usdAmountToExchange, historicalRate);
                }
            }
            while (run.NextDay());

            run.Exchange(Currency.PLN, Currency.USD);
            run.AddFinalRecord();

            return run;
        }
    }
}
