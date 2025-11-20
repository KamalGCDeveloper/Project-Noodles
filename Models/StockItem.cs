using System;
using System.Collections.Generic;

namespace Noodle.Api.Models
{
    public class StockItem
    {
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public string? Logo { get; set; }
        public string? Description { get; set; }
        public double? MarketCapBasic { get; set; }
        public double? Volume { get; set; }
        public double? Change { get; set; }
        public double? Change1w { get; set; }
        public double? RelativeVolume10dCalc { get; set; }
        public double? EarningsPerShareDilutedTtm { get; set; }
        public double? EarningsPerShareDilutedYoyGrowthTtm { get; set; }
        public double? DividendsYield { get; set; }
        public double? Close { get; set; }
        public double? PriceEarningsTtm { get; set; }
        public int Rank { get; set; }
    }

    public class StockPagination
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
    }

    public class StockMetadata
    {
        public object? Filters { get; set; }
        public StockPagination? Pagination { get; set; }
        public string? Timestamp { get; set; }
        public string? Source { get; set; }
        public string? TrendChartTimespan { get; set; }
        public string? TrendChartUnit { get; set; }
    }

    public class StockList
    {
        public List<StockItem> Items { get; set; } = new();
        public StockMetadata Metadata { get; set; } = new();
    }
}