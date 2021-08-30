namespace ApiAuth.Models.AppSettings
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public TokenConfigurations TokenConfigurations { get; set; }
    }

    public struct ConnectionStrings
    {
        public string Name => GetType().Name;

        public string Auth { get; set; }
    }

    public struct TokenConfigurations
    {
        public string Name => GetType().Name;

        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int TokenExpirationSeconds { get; set; }
        public int RefreshTokenExpirationSeconds { get; set; }
        public string Secret { get; set; }
    }
}
