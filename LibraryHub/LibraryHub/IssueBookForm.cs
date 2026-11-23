
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public class IssueBookForm : Form
    {
        private readonly MovementRepository repository = new();

        private readonly Color backgroundColor =
            Color.FromArgb(248, 248, 245);

        private readonly Color sageColor =
            Color.FromArgb(85, 119, 101);

        private readonly Color darkColor =
            Color.FromArgb(41, 59, 49);

        private readonly Color lightSage =
            Color.FromArgb(229, 237, 229);

        private readonly Color errorColor =
            Color.FromArgb(170, 55, 55);

        private readonly ComboBox readerBox = new();
        private readonly ComboBox bookBox = new();
        private readonly ComboBox shelfBox = new();
        private readonly ComboBox librarianBox = new();

        private readonly TextBox bookSearch = new();

        private readonly NumericUpDown daysBox = new();

        private readonly Label readerInfo = new();
        private readonly Label readerStatus = new();
        private readonly Label availabilityLabel = new();
        private readonly Label dueDateLabel = new();

        private readonly Button issueButton = new();

        private readonly int? selectedReaderId;

        private bool loading = true;
        private bool readerAllowed = false;
        private bool submitting = false;

        public IssueBookForm(int? readerId = null)
        {
            selectedReaderId = readerId;

            Text = "LibraryHub — Выдача книги";

            ClientSize = new Size(640, 760);

            MinimumSize = new Size(600, 650);

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = false;
            MinimizeBox = false;

            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10);

            CreateInterface();
            LoadData();
        }

        private void CreateInterface()
        {
            Panel scrollPanel = new()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24),
                BackColor = backgroundColor
            };

            Controls.Add(scrollPanel);

            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                BackColor = backgroundColor,
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            scrollPanel.Controls.Add(layout);

            Label title = new()
            {
                Text = "Новая выдача",
                Font = new Font(
                    "Segoe UI", 23, FontStyle.Bold),
                ForeColor = darkColor,
                Dock = DockStyle.Fill,
                Height = 48,
                TextAlign = ContentAlignment.MiddleLeft
            };

            AddRow(layout, title);

            Label subtitle = new()
            {
                Text = "Оформление выдачи книги читателю",
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                Height = 32
            };

            AddRow(layout, subtitle);

            // Читатель

            AddRow(layout, CreateLabel("Читатель"));

            ConfigureComboBox(readerBox);

            readerBox.AutoCompleteMode =
                AutoCompleteMode.SuggestAppend;

            readerBox.AutoCompleteSource =
                AutoCompleteSource.ListItems;

            readerBox.SelectedIndexChanged +=
                (s, e) => UpdateReaderInfo();

            AddRow(layout, readerBox);

            // Карточка состояния читателя

            Panel readerCard = new()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Height = 95,
                Padding = new Padding(14),
                Margin = new Padding(0, 8, 0, 12)
            };

            readerInfo.Dock = DockStyle.Top;
            readerInfo.Height = 30;

            readerInfo.Font = new Font(
                "Segoe UI", 11, FontStyle.Bold);

            readerInfo.ForeColor = darkColor;

            readerStatus.Dock = DockStyle.Fill;
            readerStatus.ForeColor = sageColor;

            readerCard.Controls.Add(readerStatus);
            readerCard.Controls.Add(readerInfo);

            AddRow(layout, readerCard);

            // Поиск книги

            AddRow(layout, CreateLabel("Поиск книги"));

            bookSearch.Dock = DockStyle.Fill;

            bookSearch.Font =
                new Font("Segoe UI", 11);

            bookSearch.PlaceholderText =
                "Введите название или код книги";

            bookSearch.MaxLength = 150;

            bookSearch.Margin =
                new Padding(0, 0, 0, 8);

            bookSearch.TextChanged +=
                (s, e) =>
                {
                    if (!loading)
                        LoadBooks();
                };

            AddRow(layout, bookSearch);

            // Выбор книги

            AddRow(layout, CreateLabel("Книга"));

            ConfigureComboBox(bookBox);

            bookBox.SelectedIndexChanged +=
                (s, e) =>
                {
                    if (!loading)
                        LoadShelves();
                };

            AddRow(layout, bookBox);

            availabilityLabel.Dock = DockStyle.Fill;
            availabilityLabel.Height = 28;
            availabilityLabel.ForeColor = sageColor;

            AddRow(layout, availabilityLabel);

            // Стеллаж

            AddRow(layout, CreateLabel("Место хранения"));

            ConfigureComboBox(shelfBox);

            shelfBox.SelectedIndexChanged +=
                (s, e) => UpdateIssueButton();

            AddRow(layout, shelfBox);

            // Библиотекарь

            AddRow(layout, CreateLabel("Библиотекарь"));

            ConfigureComboBox(librarianBox);

            librarianBox.SelectedIndexChanged +=
                (s, e) => UpdateIssueButton();

            AddRow(layout, librarianBox);

            // Срок выдачи

            AddRow(layout, CreateLabel("Срок выдачи, дней"));

            daysBox.Minimum = 1;
            daysBox.Maximum = 30;
            daysBox.Value = 14;

            daysBox.Width = 130;

            daysBox.Font =
                new Font("Segoe UI", 11);

            daysBox.Margin =
                new Padding(0, 0, 0, 8);

            daysBox.ValueChanged +=
                (s, e) => UpdateDueDate();

            AddRow(layout, daysBox);

            dueDateLabel.Dock = DockStyle.Fill;
            dueDateLabel.Height = 34;

            dueDateLabel.ForeColor = darkColor;

            dueDateLabel.Font = new Font(
                "Segoe UI", 11, FontStyle.Bold);

            AddRow(layout, dueDateLabel);

            // Кнопки

            FlowLayoutPanel buttons = new()
            {
                Dock = DockStyle.Fill,
                Height = 58,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };

            issueButton.Text = "Выдать книгу";

            issueButton.Size = new Size(170, 42);

            issueButton.BackColor = sageColor;
            issueButton.ForeColor = Color.White;

            issueButton.FlatStyle = FlatStyle.Flat;

            issueButton.FlatAppearance.BorderSize = 0;

            issueButton.Cursor = Cursors.Hand;
            issueButton.Enabled = false;

            issueButton.Click += IssueBook;

            Button cancelButton = new()
            {
                Text = "Отмена",
                Size = new Size(120, 42),
                BackColor = lightSage,
                ForeColor = darkColor,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };

            cancelButton.FlatAppearance.BorderSize = 0;

            buttons.Controls.Add(issueButton);
            buttons.Controls.Add(cancelButton);

            AddRow(layout, buttons);

            CancelButton = cancelButton;

            UpdateDueDate();
        }

        private static void AddRow(
            TableLayoutPanel layout,
            Control control)
        {
            int row = layout.RowCount;

            layout.RowCount++;

            layout.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(control, 0, row);
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = darkColor,

                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),

                Dock = DockStyle.Fill,
                Height = 26,

                Margin = new Padding(0, 5, 0, 0),

                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private static void ConfigureComboBox(ComboBox box)
        {
            box.Dock = DockStyle.Fill;

            box.Font = new Font("Segoe UI", 11);

            box.DropDownStyle =
                ComboBoxStyle.DropDownList;

            box.Margin =
                new Padding(0, 0, 0, 8);

            box.MaxDropDownItems = 12;
            box.IntegralHeight = true;
        }

        // Загрузка данных

        private void LoadData()
        {
            loading = true;

            try
            {
                readerBox.DisplayMember = "ФИО";

                readerBox.ValueMember =
                    "код_читателя";

                readerBox.DataSource =
                    repository.GetReaders();

                librarianBox.DisplayMember = "ФИО";

                librarianBox.ValueMember =
                    "код_библиотекаря";

                librarianBox.DataSource =
                    repository.GetLibrarians();

                librarianBox.SelectedIndex = -1;

                readerBox.SelectedIndex = -1;

                if (selectedReaderId.HasValue)
                {
                    readerBox.SelectedValue =
                        selectedReaderId.Value;

                    readerBox.Enabled = false;
                }

                LoadBooks();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Ошибка загрузки данных:\n" +
                    ex.Message);
            }
            finally
            {
                loading = false;

                UpdateReaderInfo();
                LoadShelves();
                UpdateIssueButton();
            }
        }

        // Поиск и загрузка книг

        private void LoadBooks()
        {
            bool previousLoading = loading;

            loading = true;

            try
            {
                bookBox.DataSource = null;

                shelfBox.DataSource = null;

                bookBox.DisplayMember = "описание";

                bookBox.ValueMember = "код_книги";

                bookBox.DataSource =
                    repository.SearchAvailableBooks(
                        bookSearch.Text.Trim());

                bookBox.SelectedIndex = -1;

                availabilityLabel.Text =
                    bookBox.Items.Count == 0
                    ? "Доступные книги не найдены."
                    : "Выберите книгу из списка.";
            }
            catch (Exception ex)
            {
                ShowError(
                    "Ошибка поиска книг:\n" +
                    ex.Message);
            }
            finally
            {
                loading = previousLoading;
            }

            if (!loading)
                LoadShelves();
        }

        // Загрузка стеллажей выбранной книги

        private void LoadShelves()
        {
            if (loading)
                return;

            loading = true;

            try
            {
                shelfBox.DataSource = null;

                if (bookBox.SelectedValue is not int bookId)
                {
                    availabilityLabel.Text =
                        bookBox.Items.Count == 0
                        ? "Доступные книги не найдены."
                        : "Выберите книгу.";

                    return;
                }

                DataTable shelves =
                    repository.GetAvailableShelves(bookId);

                shelfBox.DisplayMember = "описание";

                shelfBox.ValueMember = "код_стеллажа";

                shelfBox.DataSource = shelves;

                shelfBox.SelectedIndex = -1;

                int available = 0;

                foreach (DataRow row in shelves.Rows)
                {
                    available += Convert.ToInt32(
                        row["доступное_кол_во"]);
                }

                availabilityLabel.Text =
                    $"Доступно экземпляров: {available}";

                availabilityLabel.ForeColor =
                    available > 0
                    ? sageColor
                    : errorColor;

                if (shelves.Rows.Count == 1)
                    shelfBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError(
                    "Ошибка загрузки мест хранения:\n" +
                    ex.Message);
            }
            finally
            {
                loading = false;

                UpdateIssueButton();
            }
        }

        // Проверка состояния читателя

        private void UpdateReaderInfo()
        {
            if (loading)
                return;

            readerAllowed = false;

            try
            {
                if (readerBox.SelectedValue is not int readerId)
                {
                    readerInfo.Text =
                        "Читатель не выбран";

                    readerStatus.Text =
                        "Выберите читателя для проверки.";

                    readerStatus.ForeColor = Color.Gray;

                    return;
                }

                DataTable summary =
                    repository.GetReaderSummary(readerId);

                if (summary.Rows.Count == 0)
                    return;

                int active = Convert.ToInt32(
                    summary.Rows[0]["на_руках"]);

                int overdue = Convert.ToInt32(
                    summary.Rows[0]["просрочено"]);

                readerInfo.Text =
                    $"На руках: {active} из 5   |   " +
                    $"Просрочено: {overdue}";

                if (overdue > 0)
                {
                    readerStatus.Text =
                        "Выдача запрещена: имеются просроченные книги.";

                    readerStatus.ForeColor = errorColor;
                }
                else if (active >= 5)
                {
                    readerStatus.Text =
                        "Выдача запрещена: достигнут лимит книг.";

                    readerStatus.ForeColor = errorColor;
                }
                else
                {
                    readerAllowed = true;

                    readerStatus.Text =
                        $"Выдача разрешена. Можно взять ещё {5 - active}.";

                    readerStatus.ForeColor = sageColor;
                }
            }
            catch (Exception ex)
            {
                readerStatus.Text =
                    "Не удалось проверить читателя.";

                readerStatus.ForeColor = errorColor;

                ShowError(ex.Message);
            }
            finally
            {
                UpdateIssueButton();
            }
        }

        // Предполагаемая дата возврата

        private void UpdateDueDate()
        {
            DateTime dueDate =
                DateTime.Today.AddDays(
                    (int)daysBox.Value);

            dueDateLabel.Text =
                $"Вернуть до: {dueDate:dd.MM.yyyy}";
        }

        // Доступность кнопки выдачи

        private void UpdateIssueButton()
        {
            issueButton.Enabled =
                !loading &&
                !submitting &&
                readerAllowed &&
                readerBox.SelectedValue is int &&
                bookBox.SelectedValue is int &&
                shelfBox.SelectedValue is int &&
                librarianBox.SelectedValue is int;
        }

        // Оформление выдачи

        private void IssueBook(object? sender, EventArgs e)
        {
            if (!readerAllowed || submitting)
                return;

            if (readerBox.SelectedValue is not int readerId ||
                bookBox.SelectedValue is not int bookId ||
                shelfBox.SelectedValue is not int shelfId ||
                librarianBox.SelectedValue is not int librarianId)
            {
                ShowError(
                    "Заполните все необходимые поля.");

                return;
            }

            int days = (int)daysBox.Value;

            string bookName = bookBox.Text;

            if (bookBox.SelectedItem is DataRowView bookRow)
            {
                bookName =
                    bookRow["наименование"].ToString() ?? "";
            }

            DateTime dueDate =
                DateTime.Today.AddDays(days);

            DialogResult confirmation =
                MessageBox.Show(
                    $"Читатель: {readerBox.Text}\n\n" +
                    $"Книга: {bookName}\n" +
                    $"Стеллаж: №{shelfId}\n" +
                    $"Библиотекарь: {librarianBox.Text}\n" +
                    $"Срок выдачи: {days} дней\n" +
                    $"Вернуть до: {dueDate:dd.MM.yyyy}\n\n" +
                    "Подтвердить выдачу книги?",
                    "Подтверждение выдачи",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
                return;

            submitting = true;

            UpdateIssueButton();

            try
            {
                repository.IssueBook(
                    bookId,
                    readerId,
                    librarianId,
                    shelfId,
                    days);

                MessageBox.Show(
                    "Книга успешно выдана!\n\n" +
                    $"Вернуть до: {dueDate:dd.MM.yyyy}",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;

                Close();
            }
            catch (SqlException ex)
                when (ex.Number >= 50000 &&
                      ex.Number <= 50011)
            {
                ShowError(ex.Message);

                UpdateReaderInfo();

                LoadBooks();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Ошибка оформления выдачи:\n" +
                    ex.Message);
            }
            finally
            {
                submitting = false;

                UpdateIssueButton();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                this,
                message,
                "LibraryHub",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
