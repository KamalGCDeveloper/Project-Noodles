namespace Noodle.Api.Constants
{
    public static class StablecoinConstants
    {
        public static readonly string[] ExcludedSymbols =
        {
            "ONCUSD", "EDLCUSD", "FLXUSD", "STNDUSD", "MTRUSD", "BACUSD",
            "FRAXUSD", "SPAUSD", "SDAIUSD", "HONEYUSD", "USUALUSD",
            "USDNNUSD", "USDAUSD", "FRXUSDUSD"
        };

        public static readonly (string base_currency, string base_currency_desc)[] ForceInclude =
        {
            ("PAXG", "PAX Gold"),
            ("USR", "Resolv USR"),
            ("USDZ", "Amzen Finance"),
            ("YU", "Yala"),
            ("USTC", "TerraClassicUSD"),
            ("USDM", "Mountain Protocol"),
            ("HONEY", "Honey"),
            ("HBD", "Hive Dollar"),
            ("EUSD", "Electronic USD"),
            ("USDA", "USDa (Avalon Labs)"),
            ("XUSD", "Straits USD"),
            ("MIM", "Magic Internet Money"),
            ("USDN", "Noble Dollar")
        };
    }
}