using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public static class Database
    {
        private const string ConnectionString =
            "Server=sheelpek;" +
            "Database=biblioteka;" +
            "Integrated Security=True;" +
            "Encrypt=True;" +
            "TrustServerCertificate=True;";

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
