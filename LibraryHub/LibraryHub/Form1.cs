
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public partial class Form1 : Form
    {
        private readonly DataGridView booksTable = new DataGridView();
        private readonly TextBox searchBox = new TextBox();

        private readonly string connectionString =
            "Server=sheelpek;" +
            "Database=biblioteka;" +
            "Integrated Security=True;" +
            "Encrypt=True;" +
            "TrustServerCertificate=True;";

        private readonly Color backgroundColor =
            Color.FromArgb(248, 248, 245);

        private readonly Color sageColor =
            Color.FromArgb(85, 119, 101);

        private readonly Color darkColor =
            Color.FromArgb(41, 59, 49);

        public Form1()
        {
            InitializeComponent();

            Text = "LibraryHub — Каталог книг";
            Size = new Size(1050, 650);
            MinimumSize = new Size(950, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10);

            CreateInterface();
            LoadBooks();
        }

        private void CreateInterface()
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(25);
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.BackColor = backgroundColor;

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            Controls.Add(layout);

            Label title = new Label();
            title.Text = "LibraryHub";
            title.Font = new Font(
                "Segoe UI", 24, FontStyle.Bold);
            title.ForeColor = darkColor;
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleLeft;

            layout.Controls.Add(title, 0, 0);

            FlowLayoutPanel toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.FlowDirection = FlowDirection.LeftToRight;
            toolbar.WrapContents = false;
            toolbar.Padding = new Padding(0, 8, 0, 0);

            layout.Controls.Add(toolbar, 0, 1);

            Label searchLabel = new Label();
            searchLabel.Text = "Поиск книги";
            searchLabel.ForeColor = darkColor;
            searchLabel.AutoSize = true;
            searchLabel.Margin = new Padding(0, 9, 15, 0);

            toolbar.Controls.Add(searchLabel);

            searchBox.Width = 300;
            searchBox.Font = new Font("Segoe UI", 11);
            searchBox.BackColor = Color.White;
            searchBox.ForeColor = darkColor;
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.Margin = new Padding(0, 3, 15, 0);
            searchBox.PlaceholderText = "Введите название...";
            searchBox.TextChanged += SearchBooks;

            toolbar.Controls.Add(searchBox);

            Button refreshButton = new Button();
            refreshButton.Text = "Обновить";
            refreshButton.Size = new Size(120, 35);
            refreshButton.BackColor =
                Color.FromArgb(229, 237, 229);
            refreshButton.ForeColor = darkColor;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Cursor = Cursors.Hand;
            refreshButton.Margin = new Padding(0, 3, 10, 0);
            refreshButton.Click += RefreshBooks;

            toolbar.Controls.Add(refreshButton);

            Button addButton = new Button();
            addButton.Text = "+ Добавить книгу";
            addButton.Size = new Size(165, 35);
            addButton.BackColor = sageColor;
            addButton.ForeColor = Color.White;
            addButton.FlatStyle = FlatStyle.Flat;
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Cursor = Cursors.Hand;
            addButton.Margin = new Padding(0, 3, 0, 0);
            addButton.Click += AddBook;

            toolbar.Controls.Add(addButton);

            booksTable.Dock = DockStyle.Fill;
            booksTable.BackgroundColor = Color.White;
            booksTable.BorderStyle = BorderStyle.None;
            booksTable.ReadOnly = true;
            booksTable.AllowUserToAddRows = false;
            booksTable.AllowUserToDeleteRows = false;
            booksTable.AllowUserToResizeRows = false;
            booksTable.RowHeadersVisible = false;
            booksTable.MultiSelect = false;

            booksTable.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            booksTable.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            booksTable.RowTemplate.Height = 40;
            booksTable.ColumnHeadersHeight = 45;

            booksTable.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            booksTable.EnableHeadersVisualStyles = false;

            booksTable.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(229, 237, 229);

            booksTable.ColumnHeadersDefaultCellStyle.ForeColor =
                darkColor;

            booksTable.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            booksTable.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(229, 237, 229);

            booksTable.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                darkColor;

            booksTable.DefaultCellStyle.BackColor =
                Color.White;

            booksTable.DefaultCellStyle.ForeColor =
                darkColor;

            booksTable.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            booksTable.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(177, 205, 185);

            booksTable.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(25, 48, 35);

            booksTable.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(250, 251, 249);

            booksTable.AlternatingRowsDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(177, 205, 185);

            booksTable.AlternatingRowsDefaultCellStyle.SelectionForeColor =
                Color.FromArgb(25, 48, 35);

            booksTable.GridColor =
                Color.FromArgb(230, 235, 230);

            booksTable.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            booksTable.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;

            layout.Controls.Add(booksTable, 0, 2);

            Label footer = new Label();
            footer.Text =
                "LibraryHub | Система управления библиотекой";
            footer.ForeColor = Color.Gray;
            footer.Font = new Font("Segoe UI", 9);
            footer.Dock = DockStyle.Fill;
            footer.TextAlign = ContentAlignment.MiddleLeft;

            layout.Controls.Add(footer, 0, 3);
        }

        private void AddBook(object? sender, EventArgs e)
        {
            using BookForm form = new BookForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadBooks();
            }
        }

        private void RefreshBooks(object? sender, EventArgs e)
        {
            LoadBooks();
        }

        private void SearchBooks(object? sender, EventArgs e)
        {
            LoadBooks();
        }

        private void LoadBooks()
        {
            string query = @"
                SELECT
                    [код_книги] AS [Код книги],
                    [наименование] AS [Название книги],
                    [год_издания] AS [Год издания]
                FROM [Книги]
                WHERE [наименование] LIKE @search
                ORDER BY [код_книги] ASC";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                using SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.Add(
                    "@search", SqlDbType.NVarChar, 150
                ).Value = "%" + searchBox.Text + "%";

                using SqlDataAdapter adapter =
                    new SqlDataAdapter(command);

                DataTable table = new DataTable();
                adapter.Fill(table);

                booksTable.DataSource = table;
                booksTable.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки книг:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
