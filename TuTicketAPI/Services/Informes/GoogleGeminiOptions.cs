namespace TuTicketAPI.Services.Informes
{
    public class GoogleGeminiOptions
    {
        public const string SectionName = "GoogleGemini";

        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
        public string Modelo { get; set; } = "gemini-3.1-flash-lite";
        public string Proveedor { get; set; } = "Google";
        public decimal Temperatura { get; set; } = 0.2m;
        public int MaxOutputTokens { get; set; } = 4096;
        public int TimeoutSeconds { get; set; } = 120;
    }
}
