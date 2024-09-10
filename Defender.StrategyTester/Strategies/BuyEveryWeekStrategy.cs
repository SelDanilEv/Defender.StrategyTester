using Defender.StrategyTester.CVS;
using Defender.StrategyTester.Domain;

namespace Defender.StrategyTester.Strategies
{
    public class BuyEveryWeekStrategy : Strategy
    {
        public BuyEveryWeekStrategy(int uSDEveryWeek) : base(
            nameof(BuyEveryWeekStrategy),
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
            USDEveryWeek = uSDEveryWeek;
        }

        private int USDEveryWeek { get; init; }

        public override async Task<StrategyRun> Execute()
        {
            var run = new StrategyRun
            {
                Name = $"{Name}_{USDEveryWeek}",
                RunNumber = 0,
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
                    HistoricalData historicalRate =
                        await run.GetTodayRateAsync(Currency.USD, Currency.PLN);

                    var usdAmountToExchange = Math.Round(
                        -run.Balance[Currency.PLN] * historicalRate.Invert.Rate,
                        2, MidpointRounding.AwayFromZero);

                    run.Exchange(Currency.USD, Currency.PLN, usdAmountToExchange, historicalRate);
                }

                if (run.DayCounter % 7 == 0)
                {
                    HistoricalData historicalRate = 
                        await run.GetTodayRateAsync(Currency.USD, Currency.PLN);

                    run.Exchange(Currency.USD, Currency.PLN, USDEveryWeek, historicalRate);
                }
            }
            while (run.NextDay());

            run.Exchange(Currency.PLN, Currency.USD, int.MaxValue);
            run.AddFinalRecord();

            return run;
        }
    }
}
