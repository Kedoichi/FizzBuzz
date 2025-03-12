namespace FizzBuzzGame.Infrastructure.Data
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string GamesCollectionName { get; set; } = string.Empty;
        public string GameSessionsCollectionName { get; set; } = string.Empty;
    }
}
