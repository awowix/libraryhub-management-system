
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public class ReaderRepository
    {
        private readonly string connectionString =
            "Server=sheelpek;" +
            "Database=biblioteka;" +
            "Integrated Security=True;" +
            "Encrypt=True;" +
            "TrustServerCertificate=True;";

        // Получение списка читателей и поиск
        public DataTable GetReaders(string search)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            string query = @"
                SELECT
                    [код_читателя] AS [Код],
                    [ФИО],
                    [телефон] AS [Телефон],
                    [адрес_проживания] AS [Адрес],
                    [дата_рождения] AS [Дата рождения],
                    [пол] AS [Пол],
                    [место_работы_учебы] AS [Место работы/учёбы],
                    [профессия] AS [Профессия]
                FROM [Читатели]
                WHERE [ФИО] LIKE @search
                   OR [телефон] LIKE @search
                ORDER BY [код_читателя] ASC";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@search", SqlDbType.NVarChar, 250)
                .Value = "%" + search.Trim() + "%";

            using SqlDataAdapter adapter =
                new SqlDataAdapter(command);

            DataTable table = new DataTable();
            adapter.Fill(table);

            return table;
        }

        // Получение одного читателя по коду
        public DataRow? GetReaderById(int readerId)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            string query = @"
                SELECT
                    [код_читателя] AS [Код],
                    [ФИО],
                    [телефон] AS [Телефон],
                    [адрес_проживания] AS [Адрес],
                    [дата_рождения] AS [Дата рождения],
                    [пол] AS [Пол],
                    [место_работы_учебы] AS [Место работы/учёбы],
                    [профессия] AS [Профессия]
                FROM [Читатели]
                WHERE [код_читателя] = @readerId";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@readerId", SqlDbType.Int).Value = readerId;

            using SqlDataAdapter adapter =
                new SqlDataAdapter(command);

            DataTable table = new DataTable();
            adapter.Fill(table);

            return table.Rows.Count > 0
                ? table.Rows[0]
                : null;
        }

        // Добавление нового читателя
        public int AddReader(
            string fullName,
            string? phone,
            string? address,
            DateTime? birthDate,
            string? gender,
            string? workplace,
            string? profession)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            string query = @"
                INSERT INTO [Читатели]
                (
                    [ФИО],
                    [телефон],
                    [адрес_проживания],
                    [дата_рождения],
                    [пол],
                    [место_работы_учебы],
                    [профессия]
                )
                VALUES
                (
                    @fullName,
                    @phone,
                    @address,
                    @birthDate,
                    @gender,
                    @workplace,
                    @profession
                )";

            using SqlCommand command =
                new SqlCommand(query, connection);

            AddReaderParameters(
                command,
                fullName,
                phone,
                address,
                birthDate,
                gender,
                workplace,
                profession);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        // Редактирование существующего читателя
        public int UpdateReader(
            int readerId,
            string fullName,
            string? phone,
            string? address,
            DateTime? birthDate,
            string? gender,
            string? workplace,
            string? profession)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            string query = @"
                UPDATE [Читатели]
                SET
                    [ФИО] = @fullName,
                    [телефон] = @phone,
                    [адрес_проживания] = @address,
                    [дата_рождения] = @birthDate,
                    [пол] = @gender,
                    [место_работы_учебы] = @workplace,
                    [профессия] = @profession
                WHERE [код_читателя] = @readerId";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add(
                "@readerId", SqlDbType.Int).Value = readerId;

            AddReaderParameters(
                command,
                fullName,
                phone,
                address,
                birthDate,
                gender,
                workplace,
                profession);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        // Общие параметры для INSERT и UPDATE
        private void AddReaderParameters(
            SqlCommand command,
            string fullName,
            string? phone,
            string? address,
            DateTime? birthDate,
            string? gender,
            string? workplace,
            string? profession)
        {
            command.Parameters.Add(
                "@fullName", SqlDbType.NVarChar, 100)
                .Value = fullName.Trim();

            command.Parameters.Add(
                "@phone", SqlDbType.VarChar, 20)
                .Value = DbText(phone);

            command.Parameters.Add(
                "@address", SqlDbType.NVarChar, 200)
                .Value = DbText(address);

            command.Parameters.Add(
                "@birthDate", SqlDbType.Date)
                .Value = birthDate.HasValue
                    ? (object)birthDate.Value.Date
                    : DBNull.Value;

            command.Parameters.Add(
                "@gender", SqlDbType.NVarChar, 10)
                .Value = DbText(gender);

            command.Parameters.Add(
                "@workplace", SqlDbType.NVarChar, 150)
                .Value = DbText(workplace);

            command.Parameters.Add(
                "@profession", SqlDbType.NVarChar, 100)
                .Value = DbText(profession);
        }

        private object DbText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? DBNull.Value
                : value.Trim();
        }

        public int DeleteReader(int readerId)
        {
            using SqlConnection connection =
                new SqlConnection(connectionString);

            connection.Open();

            string query = @"
        delete from [Читатели]
        where [код_читателя] = @readerId
          and not exists (
              select 1 from [Движение]
              where [код_читателя] = @readerId
          )
          and not exists (
              select 1 from [Штраф]
              where [код_читателя] = @readerId
          )";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.Add("@readerId", SqlDbType.Int).Value =
                readerId;

            return command.ExecuteNonQuery();
        }

    }
}
