
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

        private readonly Color lightSage =
            Color.FromArgb(229, 237, 229);

        private readonly Color selectedSage =
            Color.FromArgb(177, 205, 185);

        private readonly Panel contentPanel = new Panel();
        private readonly FlowLayoutPanel sidebarMenu = new FlowLayoutPanel();

        private Panel? booksPage;
        private Button? activeMenuButton;

        public Form1()
        {
            InitializeComponent();

            Text = "LibraryHub — Система управления библиотекой";
            Size = new Size(1350, 750);
            MinimumSize = new Size(1050, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10);

            CreateInterface();
            LoadBooks();
        }

        private void CreateInterface()
        {
            Controls.Clear();

            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 1;
            mainLayout.Margin = new Padding(0);
            mainLayout.Padding = new Padding(0);
            mainLayout.BackColor = backgroundColor;

            mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 235));

            mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            Controls.Add(mainLayout);

            CreateSidebar(mainLayout);

            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = backgroundColor;
            contentPanel.Padding = new Padding(25);
            contentPanel.Margin = new Padding(0);

            mainLayout.Controls.Add(contentPanel, 1, 0);

            CreateBooksPage();
            ShowBooksPage();
        }

        private void CreateSidebar(TableLayoutPanel mainLayout)
        {
            Panel sidebar = new Panel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = lightSage;
            sidebar.Margin = new Padding(0);

            mainLayout.Controls.Add(sidebar, 0, 0);

            TableLayoutPanel sidebarLayout = new TableLayoutPanel();
            sidebarLayout.Dock = DockStyle.Fill;
            sidebarLayout.ColumnCount = 1;
            sidebarLayout.RowCount = 3;
            sidebarLayout.Padding = new Padding(15, 15, 15, 15);

            sidebarLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            sidebarLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            sidebarLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            sidebar.Controls.Add(sidebarLayout);

            Label logo = new Label();
            logo.Text = "LibraryHub";
            logo.Font = new Font(
                "Segoe UI", 19, FontStyle.Bold);
            logo.ForeColor = darkColor;
            logo.Dock = DockStyle.Fill;
            logo.TextAlign = ContentAlignment.MiddleCenter;

            sidebarLayout.Controls.Add(logo, 0, 0);

            sidebarMenu.Dock = DockStyle.Fill;
            sidebarMenu.FlowDirection = FlowDirection.TopDown;
            sidebarMenu.WrapContents = false;
            sidebarMenu.AutoScroll = true;
            sidebarMenu.BackColor = lightSage;
            sidebarMenu.Margin = new Padding(0);

            sidebarLayout.Controls.Add(sidebarMenu, 0, 1);

            AddMenuHeader("Обзор");
            AddMenuButton("Главная", "Главная");

            AddMenuHeader("Библиотека");
            AddMenuButton("Каталог книг", "Книги");
            AddMenuButton("Читатели", "Читатели");
            AddMenuButton("Выдача и возврат", "Выдача и возврат");
            AddMenuButton("Книжный фонд", "Книжный фонд");

            AddMenuHeader("Управление");
            AddMenuButton("Отчёты", "Отчёты");
            AddMenuButton("Сотрудники", "Сотрудники");

            Button profileButton = new Button();
            profileButton.Text = "Мой профиль";
            profileButton.Dock = DockStyle.Fill;
            profileButton.BackColor = sageColor;
            profileButton.ForeColor = Color.White;
            profileButton.FlatStyle = FlatStyle.Flat;
            profileButton.FlatAppearance.BorderSize = 0;
            profileButton.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);
            profileButton.Cursor = Cursors.Hand;
            profileButton.Margin = new Padding(0, 15, 0, 0);

            profileButton.Click += (sender, e) =>
            {
                SetActiveMenu(null);
                ShowPlaceholderPage("Мой профиль");
            };

            sidebarLayout.Controls.Add(profileButton, 0, 2);
        }

        private void AddMenuHeader(string text)
        {
            Label header = new Label();
            header.Text = text.ToUpper();
            header.ForeColor = Color.FromArgb(105, 125, 112);
            header.Font = new Font(
                "Segoe UI", 8, FontStyle.Bold);
            header.Size = new Size(185, 30);
            header.TextAlign = ContentAlignment.BottomLeft;
            header.Margin = new Padding(12, 10, 0, 8);

            sidebarMenu.Controls.Add(header);
        }

        private void AddMenuButton(string text, string section)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(195, 43);
            button.Margin = new Padding(0, 0, 0, 7);
            button.BackColor = lightSage;
            button.ForeColor = darkColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 0, 0);
            button.Cursor = Cursors.Hand;

            button.Click += (sender, e) =>
            {
                SetActiveMenu(button);

                if (section == "Книги")
                {
                    ShowBooksPage();
                }

                else if (section == "Читатели")
                {
                    contentPanel.Controls.Clear();

                    ReadersForm readersPage = new ReadersForm();
                    readersPage.Dock = DockStyle.Fill;

                    contentPanel.Controls.Add(readersPage);
                }
                else if (section == "Выдача и возврат")
                {
                    contentPanel.Controls.Clear();

                    MovementsForm movementsPage = new MovementsForm();
                    movementsPage.Dock = DockStyle.Fill;

                    contentPanel.Controls.Add(movementsPage);
                }
                else
                {
                    ShowPlaceholderPage(section);
                }

            };

            sidebarMenu.Controls.Add(button);

            if (section == "Книги")
            {
                SetActiveMenu(button);
            }
        }

        private void SetActiveMenu(Button? button)
        {
            if (activeMenuButton != null)
            {
                activeMenuButton.BackColor = lightSage;
                activeMenuButton.ForeColor = darkColor;
                activeMenuButton.Font =
                    new Font("Segoe UI", 10);
            }

            activeMenuButton = button;

            if (activeMenuButton != null)
            {
                activeMenuButton.BackColor = sageColor;
                activeMenuButton.ForeColor = Color.White;
                activeMenuButton.Font =
                    new Font(
                        "Segoe UI", 10, FontStyle.Bold);
            }
        }

        private void CreateBooksPage()
        {
            booksPage = new Panel();
            booksPage.Dock = DockStyle.Fill;
            booksPage.BackColor = backgroundColor;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.Margin = new Padding(0);

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 115));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            booksPage.Controls.Add(layout);

            Label title = new Label();
            title.Text = "Каталог книг";
            title.Font = new Font(
                "Segoe UI", 23, FontStyle.Bold);
            title.ForeColor = darkColor;
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleLeft;

            layout.Controls.Add(title, 0, 0);

            TableLayoutPanel toolsLayout = new TableLayoutPanel();
            toolsLayout.Dock = DockStyle.Fill;
            toolsLayout.ColumnCount = 1;
            toolsLayout.RowCount = 2;
            toolsLayout.Margin = new Padding(0);

            toolsLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            toolsLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            layout.Controls.Add(toolsLayout, 0, 1);

            FlowLayoutPanel searchPanel = new FlowLayoutPanel();
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.WrapContents = false;
            searchPanel.Padding = new Padding(0, 5, 0, 0);

            toolsLayout.Controls.Add(searchPanel, 0, 0);

            Label searchLabel = new Label();
            searchLabel.Text = "Поиск книги";
            searchLabel.AutoSize = true;
            searchLabel.ForeColor = darkColor;
            searchLabel.Margin = new Padding(0, 10, 15, 0);

            searchPanel.Controls.Add(searchLabel);

            searchBox.Width = 300;
            searchBox.Font = new Font("Segoe UI", 11);
            searchBox.BackColor = Color.White;
            searchBox.ForeColor = darkColor;
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.Margin = new Padding(0, 5, 15, 0);
            searchBox.PlaceholderText = "Введите название...";
            searchBox.TextChanged += SearchBooks;

            searchPanel.Controls.Add(searchBox);

            Button refreshButton = CreateActionButton(
                "Обновить", 115, lightSage, darkColor);

            refreshButton.Click += RefreshBooks;
            searchPanel.Controls.Add(refreshButton);

            FlowLayoutPanel actionsPanel = new FlowLayoutPanel();
            actionsPanel.Dock = DockStyle.Fill;
            actionsPanel.WrapContents = false;
            actionsPanel.Padding = new Padding(0, 5, 0, 0);

            toolsLayout.Controls.Add(actionsPanel, 0, 1);

            Button addButton = CreateActionButton(
                "+ Добавить книгу", 170,
                sageColor, Color.White);

            addButton.Click += AddBook;
            actionsPanel.Controls.Add(addButton);

            Button editButton = CreateActionButton(
                "Редактировать", 155,
                lightSage, darkColor);

            editButton.Click += EditBook;
            actionsPanel.Controls.Add(editButton);

            Button deleteButton = CreateActionButton(
                "Удалить", 120,
                Color.FromArgb(245, 225, 225),
                Color.FromArgb(150, 55, 55));

            deleteButton.Click += DeleteBook;
            actionsPanel.Controls.Add(deleteButton);

            ConfigureBooksTable();
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

        private Button CreateActionButton(
            string text,
            int width,
            Color background,
            Color foreground)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(width, 37);
            button.BackColor = background;
            button.ForeColor = foreground;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(0, 3, 12, 0);
            button.Font = new Font("Segoe UI", 10);

            return button;
        }

        private void ConfigureBooksTable()
        {
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
                lightSage;

            booksTable.ColumnHeadersDefaultCellStyle.ForeColor =
                darkColor;

            booksTable.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            booksTable.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                lightSage;

            booksTable.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                darkColor;

            booksTable.DefaultCellStyle.BackColor = Color.White;
            booksTable.DefaultCellStyle.ForeColor = darkColor;
            booksTable.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            booksTable.DefaultCellStyle.SelectionBackColor =
                selectedSage;

            booksTable.DefaultCellStyle.SelectionForeColor =
                darkColor;

            booksTable.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(250, 251, 249);

            booksTable.AlternatingRowsDefaultCellStyle.SelectionBackColor =
                selectedSage;

            booksTable.AlternatingRowsDefaultCellStyle.SelectionForeColor =
                darkColor;

            booksTable.GridColor =
                Color.FromArgb(230, 235, 230);

            booksTable.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            booksTable.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;
        }

        private void ShowBooksPage()
        {
            contentPanel.Controls.Clear();

            if (booksPage != null)
            {
                contentPanel.Controls.Add(booksPage);
            }
        }

        private void ShowPlaceholderPage(string section)
        {
            contentPanel.Controls.Clear();

            Panel page = new Panel();
            page.Dock = DockStyle.Fill;
            page.BackColor = backgroundColor;

            Label title = new Label();
            title.Text = section;
            title.Font = new Font(
                "Segoe UI", 23, FontStyle.Bold);
            title.ForeColor = darkColor;
            title.Dock = DockStyle.Top;
            title.Height = 80;
            title.TextAlign = ContentAlignment.MiddleLeft;

            Label description = new Label();
            description.Text =
                $"Раздел «{section}» находится в разработке.";
            description.Font = new Font("Segoe UI", 11);
            description.ForeColor = Color.Gray;
            description.Dock = DockStyle.Top;
            description.Height = 55;

            page.Controls.Add(description);
            page.Controls.Add(title);

            contentPanel.Controls.Add(page);
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
                DataTable table =
                    repository.GetBooks(searchBox.Text);

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
