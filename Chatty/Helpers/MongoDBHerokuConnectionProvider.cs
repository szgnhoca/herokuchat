using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatty.Helpers
{
    public class MongoDBHerokuConnectionProvider
    {
        public IConfiguration Configuration { get; }

        public MongoDBHerokuConnectionProvider(IConfiguration configuration) => Configuration = configuration;

        public (string url, string dbName) GetConnectionString()
        {
            string name = AvailableMongoConnectionStrings.Default;
            string envValue = Environment.GetEnvironmentVariable(name);
            if (envValue == null)
                return ExtractDbNameFromUrl(Configuration[name]);
            return ExtractDbNameFromUrl(envValue);
        }

        private (string url, string dbName) ExtractDbNameFromUrl(string connectionString)
        {
            MongoUrl uri = new MongoUrl(connectionString);
            string dbName = uri.DatabaseName;
            return (connectionString, dbName);
        }
    }

    public class AvailableMongoConnectionStrings
    {
        public const string Default = "MONGODB_URI";
    }
}
