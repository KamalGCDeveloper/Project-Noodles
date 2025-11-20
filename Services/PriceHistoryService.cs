using System.Text.Json;
using Websocket.Client;
using System.Net.WebSockets;
using Noodle.Api.Models;
using Noodle.Api.Repositories;
using System.Text.RegularExpressions;

namespace Noodle.Api.Services
{
    public interface IPriceHistoryService
    {
        Task<List<PriceHistoryResponse>> GetPriceHistory(PriceHistoryRequest req);
    }

    public class PriceHistoryService : IPriceHistoryService
    {
        private readonly IPriceHistoryRepository _repo;

        public PriceHistoryService(IPriceHistoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<PriceHistoryResponse>> GetPriceHistory(PriceHistoryRequest req)
        {
            string prefix = "CRYPTO";
            string resolvedSymbol = req.Symbol;

            // Select collection by type
            if (req.Type == "crypto")
            {
                await _repo.GetCrypto(req.Symbol);
            }
            else if (req.Type == "stock")
            {
                var decoded = Uri.UnescapeDataString(req.Symbol);
                var stock = await _repo.GetStock(decoded);

                prefix = "NASDAQ";
                resolvedSymbol = stock?.Name ?? req.Symbol;
            }
            else if (req.Type == "commodity")
            {
                var cmd = await _repo.GetCommodity(req.Symbol.ToLower());
                prefix = cmd?.Exchange ?? "CMD";
                resolvedSymbol = cmd?.Symbol ?? req.Symbol;
            }

            // Websocket
            var session = GenSession();
            var results = new List<PriceHistoryResponse>();
            var socketUrl = new Uri("wss://data.tradingview.com/socket.io/websocket");

            using var client = new WebsocketClient(socketUrl)
            {
                ReconnectTimeout = null, // disable auto-reconnect
            };

            var done = new TaskCompletionSource<bool>();
            var regex = new Regex("~m~(\\d+)~m~");

            client.MessageReceived.Subscribe(msg =>
            {
                Console.WriteLine("WS RAW => " + msg.Text);

                if (msg.Text == null) return;

                var matches = regex.Matches(msg.Text);

                foreach (Match m in matches)
                {
                    int len = int.Parse(m.Groups[1].Value);
                    int start = m.Index + m.Length;

                    if (start + len > msg.Text.Length) continue;

                    string json = msg.Text.Substring(start, len);

                    try
                    {
                        var payload = JsonSerializer.Deserialize<JsonElement>(json);
                        Console.WriteLine("WS JSON => " + payload.ToString());
                        if (payload.GetProperty("m").GetString() == "timescale_update")
                        {
                            var arr = payload.GetProperty("p")[1]
                                             .GetProperty("sds_1")
                                             .GetProperty("s");

                            foreach (var item in arr.EnumerateArray())
                            {
                                var v = item.GetProperty("v");

                                results.Add(new PriceHistoryResponse
                                {
                                    C = v[1].GetDouble(),
                                    H = v[2].GetDouble(),
                                    L = v[3].GetDouble(),
                                    O = v[4].GetDouble(),
                                    UnixTime = v[0].GetInt64(),
                                    Symbol = resolvedSymbol,
                                    Type = req.Interval
                                });
                            }

                            done.TrySetResult(true);
                        }
                    }
                    catch
                    {
                        // Skip non-JSON chunks
                    }
                }
            });

            // Start websocket
            await client.Start();

            // IMPORTANT: Await all Sends để tránh warning CS4014
            client.Send(WrapMessage(new { m = "chart_create_session", p = new object[] { session, "" } }));
            client.Send(WrapMessage(new { m = "switch_timezone", p = new object[] { session, "Etc/UTC" } }));
            client.Send(WrapMessage(new { m = "resolve_symbol", p = new object[] { session, "sds_sym_1", $"{prefix}:{resolvedSymbol}" } }));
            client.Send(WrapMessage(new { m = "create_series", p = new object[] { session, "sds_1", "s1", "sds_sym_1", "30", 300, req.Interval } }));

            var timeoutTask = Task.Delay(4000);
            var finished = await Task.WhenAny(done.Task, timeoutTask);

            if (finished == timeoutTask)
            {
                Console.WriteLine("⚠ Timeout: TradingView did not send timescale_update");
                client.Stop(WebSocketCloseStatus.NormalClosure, "timeout");

                return results; // always return!
            }

            client.Stop(WebSocketCloseStatus.NormalClosure, "done");
            return results;
        }

        private string GenSession()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rand = new Random();
            return "cs_" + new string(Enumerable.Range(0, 12)
                .Select(_ => chars[rand.Next(chars.Length)]).ToArray());
        }

        private string WrapMessage(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return $"~m~{json.Length}~m~{json}";
        }
    }
}