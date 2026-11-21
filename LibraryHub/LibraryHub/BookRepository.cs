using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public class BookRepository
    {
        public DataTable GetBooks(string search)
        {
            const string query = @"
                SELECT [код_книги] AS [Код книги],
                       [наименование] AS [Название книги],
                       [год_издания] AS [Год издания]
                FROM [Книги]
                WHERE [наименование] LIKE @search
                ORDER BY [код_книги] ASC";

            using SqlConnection connection = Database.CreateConnection();
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value =
                "%" + search + "%";
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public DataTable GetPublishers()
        {
            const string query = @"
                SELECT [код_издательства], [наименование]
                FROM [Издательства]
                ORDER BY [наименование]";
            return GetTable(query);
        }

        public DataTable GetCategories()
        {
            const string query = @"
                SELECT [раздел_классификатора], [наименование]
                FROM [Классификатор]
                ORDER BY [раздел_классификатора]";
            return GetTable(query);
        }

        public DataTable GetBookDetails(int bookId)
        {
            const string query = @"
                SELECT [наименование], [год_издания],
                       [код_издательства], [раздел_классификатора]
                FROM [Книги]
                WHERE [код_книги] = @bookId";
            using SqlConnection connection = Database.CreateConnection();
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@bookId", SqlDbType.Int).Value = bookId;
            using SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public int SaveBook(int? bookId, string name, int? year,
            int publisherId, string categoryId)
        {
            string query = bookId.HasValue
                ? @"UPDATE [Книги]
                    SET [наименование] = @name,
                        [код_издательства] = @publisher,
                        [год_издания] = @year,
                        [раздел_классификатора] = @category
                    WHERE [код_книги] = @bookId"
                : @"INSERT INTO [Книги]
                        ([наименование], [код_издательства],
                         [год_издания], [раздел_классификатора])
                    VALUES (@name, @publisher, @year, @category)";

            using SqlConnection connection = Database.CreateConnection();
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = name;
            command.Parameters.Add("@publisher", SqlDbType.Int).Value = publisherId;
            command.Parameters.Add("@year", SqlDbType.Int).Value =
                year.HasValue ? (object)year.Value : DBNull.Value;
            command.Parameters.Add("@category", SqlDbType.NVarChar, 150).Value = categoryId;
            if (bookId.HasValue)
                command.Parameters.Add("@bookId", SqlDbType.Int).Value = bookId.Value;
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int DeleteBook(int bookId)
        {
            const string query = "DELETE FROM [Книги] WHERE [код_книги] = @bookId";
            using SqlConnection connection = Database.CreateConnection();
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.Add("@bookId", SqlDbType.Int).Value = bookId;
            connection.Open();
            return command.ExecuteNonQuery();
        }

        private static DataTable GetTable(string query)
        {
            using SqlConnection connection = Database.CreateConnection();
            using SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }
    }
}
