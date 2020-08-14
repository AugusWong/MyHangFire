using Hangfire;
using Hangfire.MySql.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace JobServer
{

    public static class MySqlStorageExtensions
    {
        public static IGlobalConfiguration<MySqlStorage> UseMySqlStorage(
          [NotNull] this IGlobalConfiguration configuration,
          [NotNull] string nameOrConnectionString)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (nameOrConnectionString == null)
                throw new ArgumentNullException(nameof(nameOrConnectionString));
            MySqlStorage storage = new MySqlStorage(nameOrConnectionString);
            return configuration.UseStorage<MySqlStorage>(storage);
        }

        public static IGlobalConfiguration<MySqlStorage> UseMySqlStorage(
          [NotNull] this IGlobalConfiguration configuration,
          [NotNull] string nameOrConnectionString,
          [NotNull] MySqlStorageOptions options)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (nameOrConnectionString==null )
                throw new ArgumentNullException(nameof(nameOrConnectionString));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            MySqlStorage storage = new MySqlStorage(nameOrConnectionString, options);
            return configuration.UseStorage<MySqlStorage>(storage);
        }
    }
}
