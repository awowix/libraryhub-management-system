
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public class MovementsForm : UserControl
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

        private readonly Color selectedSage =
            Color.FromArgb(177, 205, 185);

        private readonly Color overdueColor =
            Color.FromArgb(170, 55, 55);

        private readonly DataGridView movementsTable = new();
        private readonly TextBox searchBox = new();
        private readonly ComboBox statusBox = new();

        private readonly Label activeCount = new();
        private readonly Label overdueCount = new();
        private readonly Label returnedCount = new();
        private readonly Label footer = new();

        private readonly Button returnButton = new();

        private DataTable? movementsData;
        private bool loading = true;

        public MovementsForm()
        {
            Dock = DockStyle.Fill;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10);

            CreateInterface();

            loading = false;
            LoadMovements();
        }

        private void CreateInterface()
        {
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Margin = new Padding(0),
                BackColor = backgroundColor
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            Controls.Add(layout);

            Label title = new()
            {
                Text = "Выдача и возврат",
                Font = new Font(
                    "Segoe UI", 23, FontStyle.Bold),
                ForeColor = darkColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(title, 0, 0);

            // Статистика

            TableLayoutPanel statistics = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0)
            };

            for (int i = 0; i < 3; i++)
            {
                statistics.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent, 100f / 3));
            }

            statistics.Controls.Add(
                CreateStatisticCard(
                    "На руках", activeCount, sageColor),
                0, 0);

            statistics.Controls.Add(
                CreateStatisticCard(
                    "Просрочено", overdueCount, overdueColor),
                1, 0);

            statistics.Controls.Add(
                CreateStatisticCard(
                    "Возвращено", returnedCount, darkColor),
                2, 0);

            layout.Controls.Add(statistics, 0, 1);

            // Поиск и фильтрация

            FlowLayoutPanel searchPanel = new()
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0),
                AutoScroll = true
            };

            searchBox.Width = 280;
            searchBox.Font = new Font("Segoe UI", 11);
            searchBox.PlaceholderText =
                "Название книги, читатель, № выдачи...";

            searchBox.BorderStyle =
                BorderStyle.FixedSingle;

            searchBox.ForeColor = darkColor;
            searchBox.Margin = new Padding(0, 5, 12, 0);

            searchBox.TextChanged +=
                (s, e) => ApplyFilters();

            searchPanel.Controls.Add(searchBox);

            statusBox.Width = 160;
            statusBox.Font = new Font("Segoe UI", 11);

            statusBox.DropDownStyle =
                ComboBoxStyle.DropDownList;

            statusBox.Margin = new Padding(0, 5, 12, 0);

            statusBox.Items.AddRange(new object[]
            {
                "Все операции",
                "На руках",
                "Просрочена",
                "Возвращена"
            });

            statusBox.SelectedIndex = 0;

            statusBox.SelectedIndexChanged +=
                (s, e) => ApplyFilters();

            searchPanel.Controls.Add(statusBox);

            Button refreshButton = CreateActionButton(
                "Обновить", 110, lightSage, darkColor);

            refreshButton.Click +=
                (s, e) => LoadMovements();

            searchPanel.Controls.Add(refreshButton);

            layout.Controls.Add(searchPanel, 0, 2);

            // Кнопки управления

            FlowLayoutPanel actionsPanel = new()
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0)
            };

            Button issueButton = CreateActionButton(
                "+ Выдать книгу",
                170, sageColor, Color.White);

            issueButton.Click += IssueBook;

            actionsPanel.Controls.Add(issueButton);

            returnButton.Text = "Оформить возврат";
            returnButton.Size = new Size(190, 37);
            returnButton.BackColor = lightSage;
            returnButton.ForeColor = darkColor;
            returnButton.FlatStyle = FlatStyle.Flat;
            returnButton.FlatAppearance.BorderSize = 0;
            returnButton.Cursor = Cursors.Hand;
            returnButton.Margin = new Padding(0, 3, 12, 0);
            returnButton.Enabled = false;

            returnButton.Click += ReturnBook;

            actionsPanel.Controls.Add(returnButton);

            layout.Controls.Add(actionsPanel, 0, 3);

            // Таблица операций

            ConfigureTable();
            ConfigureColumns();

            layout.Controls.Add(movementsTable, 0, 4);

            footer.Dock = DockStyle.Fill;
            footer.ForeColor = Color.Gray;
            footer.Font = new Font("Segoe UI", 9);

            footer.TextAlign =
                ContentAlignment.MiddleLeft;

            layout.Controls.Add(footer, 0, 5);
        }

        private Panel CreateStatisticCard(
            string text,
            Label valueLabel,
            Color valueColor)
        {
            Panel card = new()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 12, 10),
                Padding = new Padding(15, 10, 15, 10)
            };

            Label caption = new()
            {
                Text = text,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top,
                Height = 25
            };

            valueLabel.Text = "0";
            valueLabel.ForeColor = valueColor;

            valueLabel.Font = new Font(
                "Segoe UI", 23, FontStyle.Bold);

            valueLabel.Dock = DockStyle.Fill;

            valueLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            card.Controls.Add(valueLabel);
            card.Controls.Add(caption);

            return card;
        }

        private Button CreateActionButton(
            string text,
            int width,
            Color background,
            Color foreground)
        {
            Button button = new()
            {
                Text = text,
                Size = new Size(width, 37),
                BackColor = background,
                ForeColor = foreground,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 3, 12, 0),
                Font = new Font("Segoe UI", 10)
            };

            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        private void ConfigureTable()
        {
            movementsTable.Dock = DockStyle.Fill;
            movementsTable.BackgroundColor = Color.White;
            movementsTable.BorderStyle = BorderStyle.None;

            movementsTable.ReadOnly = true;
            movementsTable.AllowUserToAddRows = false;
            movementsTable.AllowUserToDeleteRows = false;
            movementsTable.AllowUserToResizeRows = false;
            movementsTable.RowHeadersVisible = false;
            movementsTable.MultiSelect = false;

            movementsTable.AutoGenerateColumns = false;

            movementsTable.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            movementsTable.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;

            movementsTable.ScrollBars = ScrollBars.Both;

            movementsTable.RowTemplate.Height = 42;
            movementsTable.ColumnHeadersHeight = 48;

            movementsTable.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode
                    .DisableResizing;

            movementsTable.EnableHeadersVisualStyles = false;

            movementsTable.ColumnHeadersDefaultCellStyle
                .BackColor = lightSage;

            movementsTable.ColumnHeadersDefaultCellStyle
                .ForeColor = darkColor;

            movementsTable.ColumnHeadersDefaultCellStyle
                .Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold);

            movementsTable.ColumnHeadersDefaultCellStyle
                .SelectionBackColor = lightSage;

            movementsTable.ColumnHeadersDefaultCellStyle
                .SelectionForeColor = darkColor;

            movementsTable.DefaultCellStyle.BackColor =
                Color.White;

            movementsTable.DefaultCellStyle.ForeColor =
                darkColor;

            movementsTable.DefaultCellStyle.SelectionBackColor =
                selectedSage;

            movementsTable.DefaultCellStyle.SelectionForeColor =
                darkColor;

            movementsTable.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            movementsTable.DefaultCellStyle.NullValue = "—";

            movementsTable.GridColor =
                Color.FromArgb(230, 235, 230);

            movementsTable.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            movementsTable.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;

            movementsTable.SelectionChanged +=
                (s, e) => UpdateReturnButton();

            movementsTable.CellFormatting += FormatCells;
        }

        // Явное создание столбцов таблицы

        private void ConfigureColumns()
        {
            movementsTable.AutoGenerateColumns = false;
            movementsTable.Columns.Clear();

            AddColumn("Код выдачи", "№", 50);
            AddColumn("Книга", "Книга", 200);
            AddColumn("Читатель", "Читатель", 180);
            AddColumn("Статус", "Статус", 120);

            AddColumn(
                "Дата выдачи", "Дата выдачи", 110, true);

            AddColumn(
                "Вернуть до", "Вернуть до", 110, true);

            AddColumn(
                "Дата возврата", "Дата возврата", 110, true);

            AddColumn(
                "Библиотекарь", "Библиотекарь", 160);

            AddColumn("Стеллаж", "Стеллаж", 85);
        }

        private void AddColumn(
            string name,
            string header,
            int width,
            bool isDate = false)
        {
            DataGridViewTextBoxColumn column = new()
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                Width = width,
                MinimumWidth = 40,
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode =
                    DataGridViewColumnSortMode.Automatic
            };

            if (isDate)
            {
                column.DefaultCellStyle.Format =
                    "dd.MM.yyyy";
            }

            movementsTable.Columns.Add(column);
        }

        // Загрузка данных

        private void LoadMovements()
        {
            loading = true;

            try
            {
                movementsData = repository.GetMovements();

                movementsTable.DataSource = null;

                movementsTable.DataSource =
                    movementsData.DefaultView;

                UpdateStatistics();

                loading = false;

                ApplyFilters();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Ошибка загрузки операций:\n" +
                    ex.Message);
            }
            finally
            {
                loading = false;
                UpdateReturnButton();
            }
        }

        // Обновление статистики

        private void UpdateStatistics()
        {
            if (movementsData == null)
                return;

            int active = 0;
            int overdue = 0;
            int returned = 0;

            foreach (DataRow row in movementsData.Rows)
            {
                string status =
                    row["Статус"].ToString() ?? "";

                if (status == "Возвращена")
                {
                    returned++;
                }
                else
                {
                    active++;

                    if (status == "Просрочена")
                        overdue++;
                }
            }

            activeCount.Text = active.ToString();
            overdueCount.Text = overdue.ToString();
            returnedCount.Text = returned.ToString();
        }

        // Поиск и фильтрация

        private void ApplyFilters()
        {
            if (loading || movementsData == null)
                return;

            string search = searchBox.Text.Trim();
            string filter = "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = EscapeFilter(search);

                filter =
                    $"([Книга] LIKE '%{search}%' OR " +
                    $"[Читатель] LIKE '%{search}%' OR " +
                    $"[Библиотекарь] LIKE '%{search}%' OR " +
                    $"CONVERT([Код выдачи], " +
                    $"'System.String') LIKE '%{search}%')";
            }

            string status =
                statusBox.SelectedItem?.ToString() ?? "";

            if (status != "Все операции" &&
                !string.IsNullOrEmpty(status))
            {
                if (filter.Length > 0)
                    filter += " AND ";

                if (status == "На руках")
                {
                    filter +=
                        "([Статус] = 'На руках' OR " +
                        "[Статус] = 'Просрочена')";
                }
                else
                {
                    filter += $"[Статус] = '{status}'";
                }
            }

            movementsData.DefaultView.RowFilter = filter;

            footer.Text =
                $"Найдено операций: " +
                movementsData.DefaultView.Count;

            movementsTable.ClearSelection();

            UpdateReturnButton();
        }

        private static string EscapeFilter(string text)
        {
            StringBuilder result = new();

            foreach (char symbol in text)
            {
                switch (symbol)
                {
                    case '\'':
                        result.Append("''");
                        break;

                    case '[':
                        result.Append("[[]");
                        break;

                    case ']':
                        result.Append("[]]");
                        break;

                    case '%':
                        result.Append("[%]");
                        break;

                    case '*':
                        result.Append("[*]");
                        break;

                    default:
                        result.Append(symbol);
                        break;
                }
            }

            return result.ToString();
        }

        // Цветовое выделение статусов

        private void FormatCells(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (movementsTable.Columns[e.ColumnIndex].Name
                != "Статус")
                return;

            if (e.CellStyle == null)
                return;

            string status = e.Value?.ToString() ?? "";

            if (status == "Просрочена")
            {
                e.CellStyle.ForeColor = overdueColor;
                e.CellStyle.SelectionForeColor = overdueColor;
            }
            else if (status == "Возвращена")
            {
                e.CellStyle.ForeColor = sageColor;
                e.CellStyle.SelectionForeColor = darkColor;
            }
            else
            {
                e.CellStyle.ForeColor = darkColor;
                e.CellStyle.SelectionForeColor = darkColor;
            }
        }

        // Проверка доступности возврата

        private void UpdateReturnButton()
        {
            returnButton.Enabled = false;

            if (loading ||
                movementsTable.SelectedRows.Count == 0 ||
                !movementsTable.Columns.Contains("Статус"))
                return;

            DataGridViewRow row =
                movementsTable.SelectedRows[0];

            string status =
                row.Cells["Статус"].Value?.ToString() ?? "";

            returnButton.Enabled =
                status == "На руках" ||
                status == "Просрочена";
        }

        // Выдача книги

        private void IssueBook(object? sender, EventArgs e)
        {
            using IssueBookForm form = new();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadMovements();
            }
        }

        // Возврат книги

        private void ReturnBook(object? sender, EventArgs e)
        {
            if (movementsTable.SelectedRows.Count == 0)
            {
                ShowError("Выберите выдачу в таблице.");
                return;
            }

            DataGridViewRow row =
                movementsTable.SelectedRows[0];

            int movementId = Convert.ToInt32(
                row.Cells["Код выдачи"].Value);

            string bookName =
                row.Cells["Книга"].Value?.ToString() ?? "";

            string readerName =
                row.Cells["Читатель"].Value?.ToString() ?? "";

            string status =
                row.Cells["Статус"].Value?.ToString() ?? "";

            if (status != "На руках" &&
                status != "Просрочена")
            {
                ShowError("Эта выдача уже закрыта.");
                return;
            }

            string message =
                $"Книга: {bookName}\n" +
                $"Читатель: {readerName}\n";

            if (status == "Просрочена")
            {
                message +=
                    "\nВнимание: срок возврата истёк.\n";
            }

            message += "\nПодтвердить возврат книги?";

            DialogResult confirmation = MessageBox.Show(
                message,
                "Подтверждение возврата",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
                return;

            returnButton.Enabled = false;

            try
            {
                repository.ReturnBook(movementId);

                MessageBox.Show(
                    "Книга успешно возвращена!",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadMovements();
            }
            catch (SqlException ex)
                when (ex.Number >= 50000 &&
                      ex.Number <= 50011)
            {
                ShowError(ex.Message);
                LoadMovements();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Ошибка оформления возврата:\n" +
                    ex.Message);
            }
            finally
            {
                UpdateReturnButton();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "LibraryHub",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
