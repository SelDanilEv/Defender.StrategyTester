using CsvHelper;
using CsvHelper.Configuration;
using Defender.StrategyTester.Config;
using Defender.StrategyTester.DB;
using Defender.StrategyTester.Domain;
using Defender.StrategyTester.Strategies;
using System.Globalization;

namespace Defender.StrategyTester.CVS
{
    public static class CSVFileProcessor
    {
        public static async Task ProcessCsvFile(string currencyPair, string filePath)
        {
            if (currencyPair.Length != 6)
            {
                throw new ArgumentException("Invalid currency pair format. Expected format: 'CUR1CUR2' with 6 characters.");
            }

            var currency1Str = currencyPair.Substring(0, 3);
            var currency2Str = currencyPair.Substring(3, 3);

            if (!Enum.TryParse(currency1Str, out Currency currency1) || !Enum.TryParse(currency2Str, out Currency currency2))
            {
                throw new ArgumentException("Invalid currency values in currency pair.");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            };

            var db = LiteDBService.Instance;
            var semaphore = new SemaphoreSlim(10);
            var tasks = new List<Task>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    await semaphore.WaitAsync();

                    var date = DateOnly.ParseExact(csv.GetField<string>("Date"), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    var price = decimal.Parse(csv.GetField<string>("Price"), CultureInfo.InvariantCulture);

                    var data = new HistoricalData
                    {
                        Date = date,
                        Rate = price,
                        Currency1 = currency1,
                        Currency2 = currency2
                    };

                    var task = Task.Run(() =>
                    {
                        try
                        {
                            db.Insert(data);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
        }

        public static void ExportHistoryToCsv(StrategyRun strategyRun)
        {
            if (GlobalConfig.SkipFileExportStep) return;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            var filePath = $"results/{strategyRun.Name}.csv";

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);
            // Write the header
            csv.WriteField("Date");
            foreach (var currency in Enum.GetValues(typeof(Currency)))
            {
                csv.WriteField(currency.ToString());
            }
            csv.NextRecord();

            // Write the records
            foreach (var record in strategyRun.History)
            {
                csv.WriteField(record.Key.ToString("MM/dd/yyyy"));
                foreach (var currency in Enum.GetValues(typeof(Currency)))
                {
                    if (record.Value.TryGetValue((Currency)currency, out var value))
                    {
                        csv.WriteField(value);
                    }
                    else
                    {
                        csv.WriteField(0);
                    }
                }
                csv.NextRecord();
            }
        }
    }
}
