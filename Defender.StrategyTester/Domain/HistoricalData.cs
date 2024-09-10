namespace Defender.StrategyTester.Domain
{
    public class HistoricalData
    {
        public Currency Currency1 { get; set; }
        public Currency Currency2 { get; set; }

        public decimal Rate { get; set; }
        public DateOnly Date { get; set; }

        public static HistoricalData Build(
            Currency currency1, 
            Currency currency2,
            decimal rate,
            DateOnly date)
        {
            return new HistoricalData
            {
                Currency1 = currency1,
                Currency2 = currency2,
                Rate = rate,
                Date = date
            };
        }

        public HistoricalData Invert => 
            HistoricalData.Build(Currency2, Currency1, 1 / Rate, Date);
    }
}
