using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatty.Helpers
{
    public class PostgreHerokuConnectionProvider
    {
        public IConfiguration Configuration { get; }

        public PostgreHerokuConnectionProvider(IConfiguration configuration) => Configuration = configuration;

        public string GetConnectionString(string name)
        {
            string envValue = Environment.GetEnvironmentVariable(name) ?? Configuration[name];
            bool isUrl = Uri.TryCreate(envValue, UriKind.Absolute, out Uri url);
            if (isUrl)
            {
                string host = url.Host;
                string port = url.Port.ToString();
                string dbName = url.LocalPath.Substring(1);
                string userId = url.UserInfo.Split(':')[0];
                string password = url.UserInfo.Split(':')[1];
                return $"User ID={userId};Password={password};Server={host};Port={port};Database={dbName};Pooling=true;Trust Server Certificate=true;SslMode=Prefer";
            }
            throw new ArgumentNullException();
        }
    }

    public class AvailablePostgreConnectionStrings
    {
        public const string Default = "DATABASE_URL";
    }
}
