
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public class MovementRepository
    {
        public DataTable GetMovements()
        {
            const string query = @"
                SELECT
                    д.[код_движения] AS [Код выдачи],
                    к.[наименование] AS [Книга],
                    ч.[ФИО] AS [Читатель],
                    б.[ФИО] AS [Библиотекарь],
                    д.[код_стеллажа] AS [Стеллаж],
                    д.[дата_выдачи] AS [Дата выдачи],

                    DATEADD(day, д.[на_сколько_дней],
                        д.[дата_выдачи]) AS [Вернуть до],

                    д.[дата_возврата] AS [Дата возврата],

                    CASE
                        WHEN д.[дата_возврата] IS NOT NULL
                            THEN N'Возвращена'
                        WHEN DATEADD(day, д.[на_сколько_дней],
                            д.[дата_выдачи]) < CAST(GETDATE() AS date)
                            THEN N'Просрочена'
                        ELSE N'На руках'
                    END AS [Статус],

                    CASE
                        WHEN д.[дата_возврата] IS NULL
                        AND DATEADD(day, д.[на_сколько_дней],
                            д.[дата_выдачи]) < CAST(GETDATE() AS date)
                        THEN DATEDIFF(day,
                            DATEADD(day, д.[на_сколько_дней],
                                д.[дата_выдачи]),
                            CAST(GETDATE() AS date))
                        ELSE 0
                    END AS [Дней просрочки]

                FROM [Движение] д

                INNER JOIN [Книги] к
                    ON д.[код_книги] = к.[код_книги]

                INNER JOIN [Читатели] ч
                    ON д.[код_читателя] = ч.[код_читателя]

                INNER JOIN [Библиотекари] б
                    ON д.[код_библиотекаря] = б.[код_библиотекаря]

                ORDER BY д.[код_движения] DESC";

            return GetTable(query);
        }

        // Список читателей

        public DataTable GetReaders()
        {
            const string query = @"
                SELECT [код_читателя], [ФИО]
                FROM [Читатели]
                ORDER BY [ФИО]";

            return GetTable(query);
        }

        // Поиск читателя

        public DataTable SearchReaders(string search)
        {
            const string query = @"
                SELECT
                    [код_читателя],
                    [ФИО],
                    [телефон]
                FROM [Читатели]
                WHERE [ФИО] LIKE @search
                    OR [телефон] LIKE @search
                    OR CONVERT(nvarchar(20),
                        [код_читателя]) = @exact
                ORDER BY [ФИО]";

            return GetTable(
                query,
                new SqlParameter(
                    "@search", SqlDbType.NVarChar, 152)
                {
                    Value = "%" + search.Trim() + "%"
                },
                new SqlParameter(
                    "@exact", SqlDbType.NVarChar, 150)
                {
                    Value = search.Trim()
                });
        }

        // Карточка читателя

        public DataTable GetReaderDetails(int readerId)
        {
            const string query = @"
                SELECT
                    [код_читателя],
                    [ФИО],
                    [телефон],
                    [адрес_проживания],
                    [дата_рождения]
                FROM [Читатели]
                WHERE [код_читателя] = @readerId";

            return GetTable(
                query,
                IntParameter("@readerId", readerId));
        }

        // Активные выдачи конкретного читателя

        public DataTable GetReaderLoans(int readerId)
        {
            const string query = @"
                SELECT
                    д.[код_движения] AS [Код выдачи],
                    к.[наименование] AS [Книга],
                    д.[код_стеллажа] AS [Стеллаж],
                    д.[дата_выдачи] AS [Дата выдачи],

                    DATEADD(day, д.[на_сколько_дней],
                        д.[дата_выдачи]) AS [Вернуть до],

                    CASE
                        WHEN DATEADD(day, д.[на_сколько_дней],
                            д.[дата_выдачи]) < CAST(GETDATE() AS date)
                        THEN DATEDIFF(day,
                            DATEADD(day, д.[на_сколько_дней],
                                д.[дата_выдачи]),
                            CAST(GETDATE() AS date))
                        ELSE 0
                    END AS [Дней просрочки],

                    CASE
                        WHEN DATEADD(day, д.[на_сколько_дней],
                            д.[дата_выдачи]) < CAST(GETDATE() AS date)
                            THEN N'Просрочена'
                        ELSE N'На руках'
                    END AS [Статус]

                FROM [Движение] д

                INNER JOIN [Книги] к
                    ON д.[код_книги] = к.[код_книги]

                WHERE д.[код_читателя] = @readerId
                    AND д.[дата_возврата] IS NULL

                ORDER BY [Вернуть до]";

            return GetTable(
                query,
                IntParameter("@readerId", readerId));
        }

        // Все активные выдачи

        public DataTable GetActiveLoans()
        {
            DataTable table = GetMovements();

            DataView view = new DataView(table);

            view.RowFilter =
                "[Статус] <> 'Возвращена'";

            view.Sort = "[Вернуть до] ASC";

            return view.ToTable();
        }

        // Просроченные выдачи

        public DataTable GetOverdueLoans()
        {
            DataTable table = GetMovements();

            DataView view = new DataView(table);

            view.RowFilter =
                "[Статус] = 'Просрочена'";

            view.Sort = "[Дней просрочки] DESC";

            return view.ToTable();
        }

        // Список библиотекарей

        public DataTable GetLibrarians()
        {
            const string query = @"
                SELECT [код_библиотекаря], [ФИО]
                FROM [Библиотекари]
                ORDER BY [ФИО]";

            return GetTable(query);
        }

        // Книги, доступные для выдачи

        public DataTable GetAvailableBooks()
        {
            return SearchAvailableBooks("");
        }

        // Поиск доступных книг

        public DataTable SearchAvailableBooks(string search)
        {
            const string query = @"
                SELECT
                    к.[код_книги],
                    к.[наименование],

                    SUM(н.[доступное_кол_во])
                        AS [доступно],

                    CONCAT(
                        к.[наименование],
                        N' — доступно: ',
                        SUM(н.[доступное_кол_во])
                    ) AS [описание]

                FROM [Книги] к

                INNER JOIN [Наличие] н
                    ON к.[код_книги] = н.[код_книги]

                WHERE к.[наименование] LIKE @search
                    OR CONVERT(nvarchar(20),
                        к.[код_книги]) = @exact

                GROUP BY
                    к.[код_книги],
                    к.[наименование]

                HAVING SUM(н.[доступное_кол_во]) > 0

                ORDER BY к.[наименование]";

            return GetTable(
                query,
                new SqlParameter(
                    "@search", SqlDbType.NVarChar, 152)
                {
                    Value = "%" + search.Trim() + "%"
                },
                new SqlParameter(
                    "@exact", SqlDbType.NVarChar, 150)
                {
                    Value = search.Trim()
                });
        }

        // Доступные стеллажи

        public DataTable GetAvailableShelves(int bookId)
        {
            const string query = @"
                SELECT
                    [код_стеллажа],
                    [доступное_кол_во],

                    CONCAT(
                        N'Стеллаж №', [код_стеллажа],
                        N' — доступно: ', [доступное_кол_во]
                    ) AS [описание]

                FROM [Наличие]

                WHERE [код_книги] = @bookId
                    AND [доступное_кол_во] > 0

                ORDER BY [код_стеллажа]";

            return GetTable(
                query,
                IntParameter("@bookId", bookId));
        }

        // Статистика читателя

        public DataTable GetReaderSummary(int readerId)
        {
            const string query = @"
                SELECT
                    COUNT(*) AS [на_руках],

                    COALESCE(SUM(
                        CASE
                            WHEN DATEADD(day, [на_сколько_дней],
                                [дата_выдачи]) < CAST(GETDATE() AS date)
                            THEN 1
                            ELSE 0
                        END
                    ), 0) AS [просрочено]

                FROM [Движение]

                WHERE [код_читателя] = @readerId
                    AND [дата_возврата] IS NULL";

            return GetTable(
                query,
                IntParameter("@readerId", readerId));
        }

        // Выдача через хранимую процедуру

        public void IssueBook(
            int bookId,
            int readerId,
            int librarianId,
            int shelfId,
            int days = 14)
        {
            using SqlConnection connection =
                Database.CreateConnection();

            using SqlCommand command =
                new SqlCommand("dbo.выдать_книгу", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@код_книги", SqlDbType.Int).Value = bookId;

            command.Parameters.Add(
                "@код_читателя", SqlDbType.Int).Value = readerId;

            command.Parameters.Add(
                "@код_библиотекаря", SqlDbType.Int).Value =
                librarianId;

            command.Parameters.Add(
                "@код_стеллажа", SqlDbType.Int).Value = shelfId;

            command.Parameters.Add(
                "@на_сколько_дней", SqlDbType.Int).Value = days;

            connection.Open();
            command.ExecuteNonQuery();
        }

        // Возврат через хранимую процедуру

        public void ReturnBook(int movementId)
        {
            using SqlConnection connection =
                Database.CreateConnection();

            using SqlCommand command =
                new SqlCommand("dbo.вернуть_книгу", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.Add(
                "@код_движения", SqlDbType.Int).Value =
                movementId;

            connection.Open();
            command.ExecuteNonQuery();
        }

        // Вспомогательные методы

        private static SqlParameter IntParameter(
            string name, int value)
        {
            return new SqlParameter(name, SqlDbType.Int)
            {
                Value = value
            };
        }

        private static DataTable GetTable(
            string query,
            params SqlParameter[] parameters)
        {
            using SqlConnection connection =
                Database.CreateConnection();

            using SqlCommand command =
                new SqlCommand(query, connection);

            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            using SqlDataAdapter adapter =
                new SqlDataAdapter(command);

            DataTable table = new DataTable();

            adapter.Fill(table);

            return table;
        }
    }
}
