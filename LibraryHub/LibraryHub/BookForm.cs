
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public partial class BookForm : Form
    {
        private readonly TextBox nameBox = new TextBox();
        private readonly TextBox yearBox = new TextBox();
        private readonly ComboBox publisherBox = new ComboBox();
        private readonly ComboBox categoryBox = new ComboBox();

        private readonly int? bookId;

        private readonly string connectionString =
            "Server=sheelpek;" +
            "Database=biblioteka;" +
            "Integrated Security=True;" +
            "Encrypt=True;" +
            "TrustServerCertificate=True;";

        public BookForm() : this(null)
        {
        }

        public BookForm(int? selectedBookId)
        {
            InitializeComponent();

            bookId = selectedBookId;

            Text = bookId.HasValue
                ? "LibraryHub — Редактировать книгу"
                : "LibraryHub — Добавить книгу";

            Size = new Size(520, 540);
            MinimumSize = new Size(520, 540);
            MaximumSize = new Size(520, 540);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);
            BackColor = Color.FromArgb(248, 248, 245);

            CreateInterface();

            LoadPublishers();
            LoadCategories();

            if (bookId.HasValue)
            {
                LoadBookDetails();
            }
        }

        private void CreateInterface()
        {
            Label title = new Label();
            title.Text = bookId.HasValue
                ? "Редактирование книги"
                : "Новая книга";

            title.Font = new Font(
                "Segoe UI", 20, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(41, 59, 49);
            title.Location = new Point(30, 25);
            title.AutoSize = true;
            Controls.Add(title);

            AddLabel("Название книги", 30, 90);
            nameBox.SetBounds(30, 115, 440, 32);
            nameBox.Font = new Font("Segoe UI", 11);
            Controls.Add(nameBox);

            AddLabel("Год издания", 30, 160);
            yearBox.SetBounds(30, 185, 440, 32);
            yearBox.Font = new Font("Segoe UI", 11);
            Controls.Add(yearBox);

            AddLabel("Издательство", 30, 230);
            publisherBox.SetBounds(30, 255, 440, 32);
            publisherBox.DropDownStyle =
                ComboBoxStyle.DropDownList;
            publisherBox.Font = new Font("Segoe UI", 11);
            Controls.Add(publisherBox);

            AddLabel("Раздел классификатора", 30, 300);
            categoryBox.SetBounds(30, 325, 440, 32);
            categoryBox.DropDownStyle =
                ComboBoxStyle.DropDownList;
            categoryBox.Font = new Font("Segoe UI", 11);
            Controls.Add(categoryBox);

            Button saveButton = new Button();
            saveButton.Text = "Сохранить";
            saveButton.SetBounds(30, 400, 210, 42);
            saveButton.BackColor =
                Color.FromArgb(85, 119, 101);
            saveButton.ForeColor = Color.White;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Cursor = Cursors.Hand;
            saveButton.Click += SaveBook;
            Controls.Add(saveButton);

            Button cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.SetBounds(260, 400, 210, 42);
            cancelButton.BackColor =
                Color.FromArgb(229, 237, 229);
            cancelButton.ForeColor =
                Color.FromArgb(41, 59, 49);
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Cursor = Cursors.Hand;
            cancelButton.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private void AddLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.AutoSize = true;
            label.ForeColor = Color.FromArgb(41, 59, 49);
            Controls.Add(label);
        }

        private void LoadPublishers()
        {
            string query = @"
                SELECT
                    [код_издательства],
                    [наименование]
                FROM [Издательства]
                ORDER BY [наименование]";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection);

                DataTable table = new DataTable();
                adapter.Fill(table);

                publisherBox.DisplayMember = "наименование";
                publisherBox.ValueMember = "код_издательства";
                publisherBox.DataSource = table;
                publisherBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки издательств:\n" + ex.Message);
            }
        }

        private void LoadCategories()
        {
            string query = @"
                SELECT
                    [раздел_классификатора],
                    [наименование]
                FROM [Классификатор]
                ORDER BY [раздел_классификатора]";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(query, connection);

                DataTable table = new DataTable();
                adapter.Fill(table);

                categoryBox.DisplayMember = "наименование";
                categoryBox.ValueMember = "раздел_классификатора";
                categoryBox.DataSource = table;
                categoryBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки классификатора:\n" + ex.Message);
            }
        }

        private void LoadBookDetails()
        {
            string query = @"
                SELECT
                    [наименование],
                    [год_издания],
                    [код_издательства],
                    [раздел_классификатора]
                FROM [Книги]
                WHERE [код_книги] = @bookId";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@bookId", SqlDbType.Int
                ).Value = bookId!.Value;

                connection.Open();

                using SqlDataReader reader =
                    command.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show(
                        "Книга не найдена.",
                        "LibraryHub",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                nameBox.Text =
                    reader["наименование"].ToString();

                yearBox.Text =
                    reader["год_издания"] == DBNull.Value
                        ? ""
                        : reader["год_издания"].ToString();

                publisherBox.SelectedValue =
                    reader["код_издательства"];

                categoryBox.SelectedValue =
                    reader["раздел_классификатора"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки книги:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SaveBook(object? sender, EventArgs e)
        {
            string name = nameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название книги.");
                return;
            }

            if (name.Length > 150)
            {
                MessageBox.Show(
                    "Название не должно превышать 150 символов.");
                return;
            }

            if (publisherBox.SelectedValue == null ||
                categoryBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите издательство и раздел.");
                return;
            }

            int? year = null;

            if (!string.IsNullOrWhiteSpace(yearBox.Text))
            {
                if (!int.TryParse(yearBox.Text, out int value) ||
                    value < 1 || value > DateTime.Now.Year + 1)
                {
                    MessageBox.Show(
                        "Укажите корректный год издания.");
                    return;
                }

                year = value;
            }

            string query = bookId.HasValue
                ? @"
                    UPDATE [Книги]
                    SET
                        [наименование] = @name,
                        [код_издательства] = @publisher,
                        [год_издания] = @year,
                        [раздел_классификатора] = @category
                    WHERE [код_книги] = @bookId"
                : @"
                    INSERT INTO [Книги]
                        ([наименование],
                         [код_издательства],
                         [год_издания],
                         [раздел_классификатора])
                    VALUES
                        (@name, @publisher, @year, @category)";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@name", SqlDbType.NVarChar, 150
                ).Value = name;

                command.Parameters.Add(
                    "@publisher", SqlDbType.Int
                ).Value = Convert.ToInt32(
                    publisherBox.SelectedValue);

                command.Parameters.Add(
                    "@year", SqlDbType.Int
                ).Value = year.HasValue
                    ? year.Value
                    : DBNull.Value;

                command.Parameters.Add(
                    "@category", SqlDbType.NVarChar, 150
                ).Value = categoryBox.SelectedValue.ToString()!;

                if (bookId.HasValue)
                {
                    command.Parameters.Add(
                        "@bookId", SqlDbType.Int
                    ).Value = bookId.Value;
                }

                connection.Open();

                int affectedRows = command.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    MessageBox.Show(
                        "Запись не найдена. Изменения не сохранены.",
                        "LibraryHub",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    bookId.HasValue
                        ? "Книга успешно обновлена!"
                        : "Книга успешно добавлена!",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка сохранения книги:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
