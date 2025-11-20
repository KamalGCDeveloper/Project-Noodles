using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Noodle.Api.Helpers
{
    /// <summary>
    /// Helper dùng để cache và lấy Best Yield (từ API llama.fi)
    /// </summary>
    public static class BestYieldHelper
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

        // Cache lưu map symbol -> yield info
        public static Dictionary<string, BestYieldItem> BestYieldMap { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
        private static DateTime _lastRefreshed = DateTime.MinValue;

        /// <summary>
        /// Model chứa dữ liệu Best Yield (APY, TVL, Project, Chain, Pool)
        /// </summary>
        public class BestYieldItem
        {
            public double Apy { get; set; }
            public double TvlUsd { get; set; }
            public string? Project { get; set; }
            public string? Chain { get; set; }
            public string? Pool { get; set; }
        }

        /// <summary>
        /// Gọi API từ llama.fi và cập nhật cache BestYieldMap
        /// </summary>
        public static async Task RefreshAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("https://yields.llama.fi/pools");
                res.EnsureSuccessStatusCode();

                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("data", out var dataElement))
                {
                    Console.WriteLine("[BestYieldHelper] API response invalid, missing 'data'.");
                    return;
                }

                var map = new Dictionary<string, BestYieldItem>(StringComparer.OrdinalIgnoreCase);

                foreach (var p in dataElement.EnumerateArray())
                {
                    var symbol = p.GetPropertyOrDefault("symbol")?.ToUpper() ?? "";
                    if (string.IsNullOrEmpty(symbol)) continue;

                    var apy = p.GetPropertyOrDefault("apy").ToDouble() ?? 0;
                    var tvlUsd = p.GetPropertyOrDefault("tvlUsd").ToDouble() ?? 0;
                    var project = p.GetPropertyOrDefault("project");
                    var chain = p.GetPropertyOrDefault("chain");
                    var pool = p.GetPropertyOrDefault("pool");

                    // Nếu symbol chưa có hoặc APY mới cao hơn, update
                    if (!map.ContainsKey(symbol) || apy > map[symbol].Apy)
                    {
                        map[symbol] = new BestYieldItem
                        {
                            Apy = apy,
                            TvlUsd = tvlUsd,
                            Project = project,
                            Chain = chain,
                            Pool = pool
                        };
                    }
                }

                BestYieldMap = map;
                _lastRefreshed = DateTime.UtcNow;

                Console.WriteLine($"[BestYieldHelper] Refreshed at {_lastRefreshed:O} ({map.Count} entries)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BestYieldHelper] Refresh failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Bắt đầu auto-refresh cache mỗi 5 phút (chạy async)
        /// Gọi ở Program.cs khi app khởi động
        /// </summary>
        public static void StartAutoRefresh()
        {
            _ = Task.Run(async () =>
            {
                await RefreshAsync(); // Refresh lần đầu
                while (true)
                {
                    await Task.Delay(RefreshInterval);
                    await RefreshAsync();
                }
            });
        }

        /// <summary>
        /// Lấy Best Yield cho 1 symbol cụ thể (VD: USDT, USDC)
        /// </summary>
        public static BestYieldItem? GetBestYield(string? symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return null;
            BestYieldMap.TryGetValue(symbol.ToUpper(), out var item);
            return item;
        }

        /// <summary>
        /// Kiểm tra lần cuối cùng cache được cập nhật
        /// </summary>
        public static DateTime LastUpdated => _lastRefreshed;
    }

    // ========================================================
    // 🔹 JsonElement extension helper (để đọc JSON an toàn)
    // ========================================================
    internal static class JsonExtensions
    {
        public static string? GetPropertyOrDefault(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.ToString() : null;
        }

        public static double? ToDouble(this string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;
            return double.TryParse(val, out var d) ? d : null;
        }
    }
}