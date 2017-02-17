using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Toolhouse.Monitoring.Dependencies
{
    public class DatabaseDependency : IDependency
    {
        public DatabaseDependency(string name, string connectionStringName)
        {
            this.Name = name;
            this.ConnectionStringName = connectionStringName;
        }

        public string ConnectionStringName
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public DependencyStatus Check()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[this.ConnectionStringName].ConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString);

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 1+1";
                conn.Open();
                int result = (int)cmd.ExecuteScalar();

                if (result != 2)
                {
                    throw new Exception("1 + 1 apparently does not equal 2 in this universe.");
                }

                return new DependencyStatus(
                    this,
                    true,
                    string.Format("Using database '{0}'", builder.InitialCatalog)
                );
            }
        }
    }
}
