using Defender.StrategyTester.Domain;
using LiteDB;
using System.Collections.Concurrent;

namespace Defender.StrategyTester.DB
{
    public class LiteDBService : IDisposable
    {
        public static LiteDBService Instance { get; } = new LiteDBService();

        private const string _databasePath = @"local.db";
        private const string _dataCollectionName = @"dataCollection";
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<HistoricalData> _dataCollection;
        private readonly ConcurrentDictionary<(Currency, Currency, DateOnly), HistoricalData?> _cache;

        private LiteDBService()
        {
            _db = new LiteDatabase(_databasePath);
            _dataCollection = _db.GetCollection<HistoricalData>(_dataCollectionName);
            _cache = new ConcurrentDictionary<(Currency, Currency, DateOnly), HistoricalData?>();
        }

        public HistoricalData? SearchData(Currency currency1, Currency currency2, DateOnly date)
        {
            var key = (currency1, currency2, date);

            if (_cache.TryGetValue(key, out var cachedData))
            {
                return cachedData;
            }

            var data = _dataCollection.FindOne(x =>
                ((x.Currency1 == currency1 && x.Currency2 == currency2) ||
                 (x.Currency1 == currency2 && x.Currency2 == currency1)) &&
                x.Date == date);

            _cache[key] = data;

            return data;
        }

        public IEnumerable<HistoricalData> SearchData(Currency currency1, Currency currency2)
        {
            var data = _dataCollection.Find(x =>
                (x.Currency1 == currency1 && x.Currency2 == currency2) ||
                (x.Currency1 == currency2 && x.Currency2 == currency1));

            return data ?? [];
        }

        public bool Insert(HistoricalData historicalData)
        {
            var isAllowedToInsert = SearchData(
                historicalData.Currency1,
                historicalData.Currency2,
                historicalData.Date) is null;

            if (!isAllowedToInsert) return false;

            _dataCollection.Insert(historicalData);

            var key = (historicalData.Currency1, historicalData.Currency2, historicalData.Date);
            _cache.TryRemove(key, out _);

            return true;
        }

        public bool InsertMany(IEnumerable<HistoricalData> historicalData)
        {
            var first = historicalData.FirstOrDefault();

            var existingDates = SearchData(first.Currency1, first.Currency2)
                .Select(x => x.Date).ToList();

            historicalData = historicalData.Where(x =>
                !existingDates.Contains(x.Date));

            foreach (var data in historicalData)
            {
                var key = (data.Currency1, data.Currency2, data.Date);
                _cache.TryRemove(key, out _);
            }

            return _dataCollection.InsertBulk(historicalData) > 0;
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            _db.Dispose();
        }
    }
}
