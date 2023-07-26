using HashidsNet;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ShortLinkGen.BusinessLogic
{
    public class ShortLinkProcessor
    {
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IDatabase redisDatabase;

        public ShortLinkProcessor(IConfiguration configuration, ConnectionMultiplexer redisConnection)
        {
            _configuration = configuration;
            this.redisConnection = redisConnection;
            this.redisDatabase = redisConnection.GetDatabase();
        }

        public ShortUri Get(string token)
        {
            if(string.IsNullOrEmpty(token) == false)
            {
                var element = redisDatabase.StringGet(token).ToString();
                if (string.IsNullOrEmpty(element) == false)
                {
                    var existKey = JsonConvert.DeserializeObject<ShortUri>(element);

                    return existKey;
                }
            }

            return null;
        }

        public async Task<string> Create(ShortUri shortUrl)
        {
            var hashids = new Hashids("this is my salt");
            var uri = new Uri(shortUrl.Source);
            var hashCode = uri.GetHashCode();

            if (hashCode < 0)
                hashCode = hashCode * -1;
            
            shortUrl.Token = hashids.Encode(hashCode);

            var element = redisDatabase.StringGet(shortUrl.Token).ToString();
            if(string.IsNullOrEmpty(element) == false)
            {
                var existKey = JsonConvert.DeserializeObject<ShortUri>(element);

                if (existKey != null)
                    return existKey.Destination;
            }

            var serviceUrl = _configuration["ServiceUrl"];

            if (serviceUrl.EndsWith("/") == false)
                serviceUrl = serviceUrl + "/";

            shortUrl.Destination = new Uri(serviceUrl + shortUrl.Token).OriginalString;

            redisDatabase.StringSet(shortUrl.Token, JsonConvert.SerializeObject(shortUrl, Formatting.Indented));

            return shortUrl.Destination;
        }
    }
}
