using Defender.StrategyTester.CVS;
using Defender.StrategyTester.Domain;

namespace Defender.StrategyTester.Strategies
{
    public class BuyEveryWeekFlexibleStrategy : Strategy
    {
        public BuyEveryWeekFlexibleStrategy(
            int uSDEveryWeek,
            int usdEveryWeekUpLimit,
            int usdEveryWeekDownLimit,
            int step
            ) : base(
            nameof(BuyEveryWeekFlexibleStrategy),
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
            USDEveryWeekUpLimit = usdEveryWeekUpLimit;
            USDEveryWeekDownLimit = usdEveryWeekDownLimit;
            Step = step;

            if (USDEveryWeek > USDEveryWeekUpLimit || USDEveryWeek < USDEveryWeekDownLimit)
                throw new ArgumentException("USDEveryWeek is out of range");
        }

        private int USDEveryWeek { get; set; }
        private int USDEveryWeekUpLimit { get; init; }
        private int USDEveryWeekDownLimit { get; init; }
        private int Step { get; init; }


        public override async Task<StrategyRun> Execute()
        {
            var run = new StrategyRun
            {
                Name = $"{Name}_{USDEveryWeek}_{USDEveryWeekUpLimit}_{USDEveryWeekDownLimit}_{Step}",
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

                if (run.DayCounter % 7 == 0)
                {
                    var historicalRate =
                        await run.GetTodayRateAsync(Currency.USD, Currency.PLN);

                    if (run.LatestRate == null)
                    {
                        run.LatestRate = historicalRate;
                    }

                    if (historicalRate.Rate != run.LatestRate.Rate)
                    {
                        if (historicalRate.Rate > run.LatestRate.Rate)
                        {
                            USDEveryWeek = Math.Min(USDEveryWeek + Step, USDEveryWeekUpLimit);
                        }
                        else
                        {
                            USDEveryWeek = Math.Max(USDEveryWeek - Step, USDEveryWeekDownLimit);
                        }
                    }

                    run.LatestRate = historicalRate;

                    run.Exchange(Currency.USD, Currency.PLN, USDEveryWeek, historicalRate);
                }
            }
            while (run.NextDay());

            run.Exchange(Currency.PLN, Currency.USD);
            run.AddFinalRecord();

            return run;
        }
    }
}
