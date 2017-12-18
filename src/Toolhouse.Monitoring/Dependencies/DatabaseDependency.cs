using System;
using System.Data.SqlClient;

namespace Toolhouse.Monitoring.Dependencies
{
    public class DatabaseDependency : IDependency
    {
        public DatabaseDependency(string name, string connectionString)
        {
            this.Name = name;
            this.ConnectionString = connectionString;
        }

        public string ConnectionString
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
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            using (var conn = new SqlConnection(ConnectionString))
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
