using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace ErizosCF.Services
{
    public class DbService
    {
        private readonly string _connectionString;
        private MySqlConnection? _connection;
        public DbService()
        {
            var server = Environment.GetEnvironmentVariable("erizoscf_db_server");
            var user = Environment.GetEnvironmentVariable("erizoscf_db_user");
            var password = Environment.GetEnvironmentVariable("erizoscf_db_password");
            var database = Environment.GetEnvironmentVariable("erizoscf_db_name");

            _connectionString = $"server={server};user={user};password={password};database={database};";
        }
        public void OpenConnectionAsync()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = new MySqlConnection(_connectionString);
                }

                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
            catch (Exception)
            {
            }
        }
        public void CloseConnection()
        {
            try
            {
                if (_connection != null && _connection.State != System.Data.ConnectionState.Closed)
                {
                    _connection.Close();
                }
            }
            catch (Exception)
            {
            }
        }
        public MySqlConnection? Connection => _connection;
        public void Dispose()
        {
            try
            {
                CloseConnection();
                _connection?.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}