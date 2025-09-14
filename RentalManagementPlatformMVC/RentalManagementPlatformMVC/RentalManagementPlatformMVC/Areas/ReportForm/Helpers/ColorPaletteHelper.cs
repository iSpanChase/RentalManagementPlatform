namespace RentalManagementPlatformMVC.Areas.ReportForm.Helpers
{
    public static class ColorPaletteHelper
    {
        private static readonly string[] ColorPalette = new[]
        {
            "rgba(249,168,81,1)",
            "rgba(241,127,70,1)",
            "rgba(235,125,108,1)",
            "rgba(198,59,40,1)",
            "rgba(147,82,60,1)",
            "rgba(139,104,61,1)",
            "rgba(134,141,61,1)",
            "rgba(109,134,109,1)",
            "rgba(140,138,118,1)",
            "rgba(206,163,132,1)"
        };

        public static IEnumerable<string> GenerateColors(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return (ColorPalette[i % ColorPalette.Length]);
            }
        }
    }
}
