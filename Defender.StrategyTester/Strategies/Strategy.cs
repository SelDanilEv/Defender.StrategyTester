using Defender.StrategyTester.Domain;

namespace Defender.StrategyTester.Strategies
{
    public abstract class Strategy
    {
        protected Strategy(
            string name,
            DateOnly fromDate,
            DateOnly toDate,
            Dictionary<Currency, decimal> initialBalance,
            Dictionary<Currency, decimal> weeklyExpensies,
            Dictionary<Currency, decimal> monthlyIncome)
        {
            this.Name = name;
            this.FromDate = fromDate;
            this.ToDate = toDate;
            this.InitialBalance = initialBalance;
            this.WeeklyExpensies = weeklyExpensies;
            this.MonthlyIncome = monthlyIncome;
        }

        public string Name { get; init; }
        public DateOnly FromDate { get; init; }
        public DateOnly ToDate { get; init; }

        public Dictionary<Currency, decimal> InitialBalance { get; init; } = [];
        public Dictionary<Currency, decimal> WeeklyExpensies { get; init; } = [];
        public Dictionary<Currency, decimal> MonthlyIncome { get; init; } = [];

        public abstract Task<StrategyRun> Execute();
    }
}
