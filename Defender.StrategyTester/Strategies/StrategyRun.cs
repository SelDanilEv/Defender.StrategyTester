using Defender.StrategyTester.DB;
using Defender.StrategyTester.Domain;
using System.Text;

namespace Defender.StrategyTester.Strategies
{
    public class StrategyRun
    {
        public string Name { get; init; }
        public int RunNumber { get; init; }

        public int DayCounter { get; set; } = 0;
        public DateOnly CurrentDay { get; set; }
        public DateOnly LastDay { get; init; }

        public Dictionary<Currency, decimal> WeeklyExpensies { get; init; } = [];
        public Dictionary<Currency, decimal> MonthlyIncome { get; init; } = [];
        public Dictionary<Currency, decimal> Balance { get; init; } = [];

        public HistoricalData LatestRate { get; set; } = null;

        public Dictionary<DateOnly, Dictionary<Currency, decimal>> History { get; init; } = [];

        public void Exchange(
            Currency from, 
            Currency to, 
            decimal amount = decimal.MaxValue, 
            HistoricalData? rate = null)
        {
            if(amount <= 0)
            {
                return;
            }

            rate ??= LatestRate ?? 
                GetTodayRateAsync(from, to).Result ??
                throw new ArgumentNullException(nameof(rate));

            LatestRate = rate;

            ValidateExchange(from, to, amount, rate);

            amount = Math.Min(amount, Balance[from]);

            rate = rate.Currency1 == from ? rate : rate.Invert;

            Balance[from] -= amount;
            Balance[to] += amount * rate.Rate;
        }

        public bool NextDay()
        {
            DayCounter++;
            CurrentDay = CurrentDay.AddDays(1);
            if (CurrentDay > LastDay)
            {
                return false;
            }

            if (DayCounter % 7 == 0)
            {
                foreach (var (currency, value) in WeeklyExpensies)
                {
                    Balance[currency] -= value;
                }
            }

            if (DayCounter % 30 == 0)
            {
                foreach (var (currency, value) in MonthlyIncome)
                {
                    Balance[currency] += value;
                }
            }

            var lastHistoryRecord = History.LastOrDefault();

            if (History.Count == 0 || !DictionariesAreEqual(lastHistoryRecord.Value, Balance))
            {
                History.Add(CurrentDay, new Dictionary<Currency, decimal>(Balance));
            }

            return true;
        }

        public async Task<HistoricalData> GetTodayRateAsync(
            Currency currency1, Currency currency2)
        {
            var targetDay = CurrentDay;

            HistoricalData historicalRate = null;
            do
            {
                historicalRate = LiteDBService
                    .Instance
                    .SearchData(Currency.USD, Currency.PLN, targetDay);

                targetDay = targetDay.AddDays(-1);
            }
            while (historicalRate is null);

            return historicalRate;
        }

        public void AddFinalRecord()
        {
            CurrentDay = CurrentDay.AddDays(1);
            History.Add(CurrentDay, new Dictionary<Currency, decimal>(Balance));

            var result = new StringBuilder();

            result.AppendLine($"Final balances for {Name} are: ");

            foreach (var (currency, value) in Balance)
            {
                result.Append($"[{currency}: {value}] ");
            }

            Console.WriteLine(result);
        }

        private bool DictionariesAreEqual(
            Dictionary<Currency, decimal> dict1,
            Dictionary<Currency, decimal> dict2)
        {
            if (dict1 == null || dict2 == null)
            {
                return false;
            }

            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (var kvp in dict1)
            {
                if (!dict2.TryGetValue(kvp.Key, out var value) || kvp.Value != value)
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateExchange(Currency from, Currency to, decimal amount, HistoricalData? rate)
        {
            if (rate == null)
            {
                throw new ArgumentNullException(nameof(rate));
            }

            if (!Balance.ContainsKey(from) || !Balance.ContainsKey(to))
            {
                throw new ArgumentException("Currency not found in balance", nameof(from));
            }

            if ((rate.Currency1 != from && rate.Currency2 != from) || (rate.Currency1 != to && rate.Currency2 != to))
            {
                throw new ArgumentException("Rate currency must match 'from' and 'to' currencies", nameof(rate));
            }

            if (rate.Currency1 == rate.Currency2)
            {
                throw new ArgumentException("Rate currency must be different", nameof(rate));
            }
        }

    }
}
