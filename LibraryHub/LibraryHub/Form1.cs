
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

        private readonly BookRepository repository = new BookRepository();

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
            Size = new Size(1100, 650);
            MinimumSize = new Size(1000, 500);
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

            searchBox.Width = 200;
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
            refreshButton.Size = new Size(110, 35);
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
            addButton.Size = new Size(155, 35);
            addButton.BackColor = sageColor;
            addButton.ForeColor = Color.White;
            addButton.FlatStyle = FlatStyle.Flat;
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Cursor = Cursors.Hand;
            addButton.Margin = new Padding(0, 3, 10, 0);
            addButton.Click += AddBook;

            toolbar.Controls.Add(addButton);

            Button editButton = new Button();
            editButton.Text = "Редактировать";
            editButton.Size = new Size(145, 35);
            editButton.BackColor =
                Color.FromArgb(229, 237, 229);
            editButton.ForeColor = darkColor;
            editButton.FlatStyle = FlatStyle.Flat;
            editButton.FlatAppearance.BorderSize = 0;
            editButton.Cursor = Cursors.Hand;
            editButton.Margin = new Padding(0, 3, 10, 0);
            editButton.Click += EditBook;

            toolbar.Controls.Add(editButton);

            Button deleteButton = new Button();
            deleteButton.Text = "Удалить";
            deleteButton.Size = new Size(100, 35);
            deleteButton.BackColor =
                Color.FromArgb(245, 225, 225);
            deleteButton.ForeColor =
                Color.FromArgb(150, 55, 55);
            deleteButton.FlatStyle = FlatStyle.Flat;
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Cursor = Cursors.Hand;
            deleteButton.Margin = new Padding(0, 3, 0, 0);
            deleteButton.Click += DeleteBook;

            toolbar.Controls.Add(deleteButton);

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

            booksTable.DefaultCellStyle.BackColor = Color.White;
            booksTable.DefaultCellStyle.ForeColor = darkColor;
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

        private void EditBook(object? sender, EventArgs e)
        {
            if (booksTable.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Сначала выберите книгу в таблице.",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DataGridViewRow row = booksTable.SelectedRows[0];

            int bookId = Convert.ToInt32(
                row.Cells["Код книги"].Value);

            using BookForm form = new BookForm(bookId);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadBooks();
            }
        }

        private void DeleteBook(object? sender, EventArgs e)
        {
            if (booksTable.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Сначала выберите книгу в таблице.",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DataGridViewRow row = booksTable.SelectedRows[0];

            int bookId = Convert.ToInt32(
                row.Cells["Код книги"].Value);

            string bookName =
                row.Cells["Название книги"].Value?.ToString() ?? "";

            DialogResult result = MessageBox.Show(
                $"Вы действительно хотите удалить книгу «{bookName}»?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            try
            {
                int affectedRows = repository.DeleteBook(bookId);

                if (affectedRows == 0)
                {
                    MessageBox.Show(
                        "Книга не найдена. Возможно, она уже удалена.",
                        "LibraryHub",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    LoadBooks();
                    return;
                }

                MessageBox.Show(
                    "Книга успешно удалена!",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadBooks();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Нельзя удалить книгу, поскольку она связана " +
                    "с другими записями библиотеки.",
                    "Удаление невозможно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка удаления книги:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
            try
            {
                DataTable table = repository.GetBooks(searchBox.Text);

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
