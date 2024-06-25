using BotElectricityTelegram.Data;
using BotElectricityTelegram.SQLLite;
using Microsoft.Data.Sqlite;

namespace BotElectricityTelegram.SqlLite
{
    public class LastIdsRepository
    {
        public static string connectionString = "Data Source=electroBot.db";
        public void CreateMainTable()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Create a table
                string createTableQuery = $"CREATE TABLE IF NOT EXISTS {SqlLiteConstants.TableName} (Id INTEGER PRIMARY KEY, Id_Message INTEGER, Date string)";
                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void InsertLastIds(LastIds lastIds)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string insertQuery = $"INSERT INTO {SqlLiteConstants.TableName}  (Id_Message,Date) VALUES (@Id_Message, @Date)";
                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id_Message", lastIds.IdMessage);
                    command.Parameters.AddWithValue("@Date", lastIds.Date);
                    command.ExecuteNonQuery();
                }
            }
        }
        public List<LastIds> SelectAllLastIds()
        {
            List<LastIds> lastIds = new List<LastIds>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                // Query data
                string selectQuery = $"SELECT * FROM {SqlLiteConstants.TableName} ";
                using (var command = new SqliteCommand(selectQuery, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lastIds.Add(new LastIds()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IdMessage = Convert.ToInt32(reader["Id_Message"]),
                            Date = reader["Date"].ToString(),
                        }
                        );
                    }
                }
            }
            return lastIds;
        }
        public LastIds SelectLastIdsByMessageId(int IdMessage)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                // Query data
                string selectQuery = $"SELECT * FROM {SqlLiteConstants.TableName}  where  Id_Message = " + IdMessage;
                using (var command = new SqliteCommand(selectQuery, connection))
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return new LastIds()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IdMessage = Convert.ToInt32(reader["Id_Message"]),
                            Date = reader["Date"].ToString(),
                        };

                    }
                }
            }
            return null;
        }
    }
}
