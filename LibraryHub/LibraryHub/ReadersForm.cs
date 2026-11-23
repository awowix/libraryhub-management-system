
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryHub
{
    public class ReadersForm : UserControl
    {
        private readonly ReaderRepository repository =
            new ReaderRepository();

        private readonly DataGridView readersTable =
            new DataGridView();

        private readonly TextBox searchBox =
            new TextBox();
        private readonly Label readerCountLabel = new Label();
        private readonly Color backgroundColor =
            Color.FromArgb(248, 248, 245);

        private readonly Color sageColor =
            Color.FromArgb(85, 119, 101);

        private readonly Color darkColor =
            Color.FromArgb(41, 59, 49);

        private readonly Color lightSage =
            Color.FromArgb(229, 237, 229);

        public ReadersForm()
        {
            Dock = DockStyle.Fill;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10);

            CreateInterface();
            LoadReaders();
        }

        private void CreateInterface()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(0)
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            Controls.Add(layout);

            // Заголовок
            Label title = new Label
            {
                Text = "Читатели",
                Font = new Font(
                    "Segoe UI", 23, FontStyle.Bold),
                ForeColor = darkColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(title, 0, 0);

            // Поиск и обновление
            FlowLayoutPanel searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 5, 0, 0)
            };

            layout.Controls.Add(searchPanel, 0, 1);

            Label searchLabel = new Label
            {
                Text = "Поиск читателя",
                AutoSize = true,
                ForeColor = darkColor,
                Margin = new Padding(0, 10, 15, 0)
            };

            searchPanel.Controls.Add(searchLabel);

            searchBox.Width = 300;
            searchBox.Font = new Font("Segoe UI", 11);
            searchBox.PlaceholderText = "ФИО или телефон...";
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.Margin = new Padding(0, 5, 15, 0);

            searchBox.TextChanged += (sender, e) =>
                LoadReaders();

            searchPanel.Controls.Add(searchBox);

            Button refreshButton =
                CreateButton("Обновить", 115, false);

            refreshButton.Click += (sender, e) =>
                LoadReaders();

            searchPanel.Controls.Add(refreshButton);

            // Кнопки управления читателями
            FlowLayoutPanel actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 3, 0, 0)
            };

            layout.Controls.Add(actionsPanel, 0, 2);

            Button addButton =
                CreateButton("+ Добавить читателя", 190, true);

            addButton.Click += AddReader;
            actionsPanel.Controls.Add(addButton);

            Button editButton =
                CreateButton("Редактировать", 155, false);

            editButton.Margin = new Padding(12, 3, 0, 0);
            editButton.Click += EditReader;
            actionsPanel.Controls.Add(editButton);

            Button deleteButton =
                CreateButton("Удалить", 120, false);

            deleteButton.Margin = new Padding(12, 3, 0, 0);
            deleteButton.BackColor = Color.FromArgb(250, 232, 232);
            deleteButton.ForeColor = Color.FromArgb(150, 50, 50);

            deleteButton.Click += DeleteReader;

            actionsPanel.Controls.Add(deleteButton);


            // Таблица читателей
            ConfigureReadersTable();

            readersTable.CellDoubleClick += ShowReaderDetails;

            layout.Controls.Add(readersTable, 0, 3);

            // Нижняя подпись
            Label footer = new Label
            {
                Text = "LibraryHub | Управление читателями",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            readerCountLabel.AutoSize = true;
            readerCountLabel.ForeColor = sageColor;
            readerCountLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            footer.Controls.Add(readerCountLabel);
            readerCountLabel.Dock = DockStyle.Right;
            layout.Controls.Add(footer, 0, 4);
        }

        private Button CreateButton(
            string text, int width, bool primary)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(width, 37),
                BackColor = primary ? sageColor : lightSage,
                ForeColor = primary ? Color.White : darkColor,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 3, 0, 0)
            };

            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        private void ConfigureReadersTable()
        {
            readersTable.Dock = DockStyle.Fill;
            readersTable.BackgroundColor = Color.White;
            readersTable.BorderStyle = BorderStyle.None;
            readersTable.ReadOnly = true;

            readersTable.AllowUserToAddRows = false;
            readersTable.AllowUserToDeleteRows = false;
            readersTable.AllowUserToResizeRows = false;

            readersTable.RowHeadersVisible = false;
            readersTable.MultiSelect = false;

            readersTable.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            readersTable.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            readersTable.RowTemplate.Height = 40;
            readersTable.ColumnHeadersHeight = 45;

            readersTable.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            readersTable.EnableHeadersVisualStyles = false;

            readersTable.ColumnHeadersDefaultCellStyle.BackColor =
                lightSage;

            readersTable.ColumnHeadersDefaultCellStyle.ForeColor =
                darkColor;

            readersTable.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 11, FontStyle.Bold);

            readersTable.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                lightSage;

            readersTable.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                darkColor;

            readersTable.DefaultCellStyle.BackColor =
                Color.White;

            readersTable.DefaultCellStyle.ForeColor =
                darkColor;

            readersTable.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            readersTable.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(177, 205, 185);

            readersTable.DefaultCellStyle.SelectionForeColor =
                darkColor;

            readersTable.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(250, 251, 249);

            readersTable.AlternatingRowsDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(177, 205, 185);

            readersTable.AlternatingRowsDefaultCellStyle.SelectionForeColor =
                darkColor;

            readersTable.GridColor =
                Color.FromArgb(230, 235, 230);

            readersTable.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            readersTable.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None;
        }

        private void LoadReaders()
        {
            try
            {
                DataTable table =
                    repository.GetReaders(searchBox.Text);

                readersTable.DataSource = table;

                string[] hiddenColumns =
                {
                    "Адрес",
                    "Дата рождения",
                    "Пол",
                    "Место работы/учёбы",
                    "Профессия"
                };

                foreach (string columnName in hiddenColumns)
                {
                    if (readersTable.Columns.Contains(columnName))
                    {
                        readersTable.Columns[columnName]!.Visible = false;
                    }
                }

                if (readersTable.Columns.Contains("Код"))
                {
                    readersTable.Columns["Код"]!.FillWeight = 15;
                }

                if (readersTable.Columns.Contains("ФИО"))
                {
                    readersTable.Columns["ФИО"]!.FillWeight = 55;
                }

                if (readersTable.Columns.Contains("Телефон"))
                {
                    readersTable.Columns["Телефон"]!.FillWeight = 30;
                }

                readersTable.ClearSelection();
                readerCountLabel.Text = $"Найдено читателей: {table.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки читателей:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Добавление нового читателя
        private void AddReader(object? sender, EventArgs e)
        {
            using ReaderForm form = new ReaderForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadReaders();
            }
        }

        // Редактирование выбранного читателя
        private void EditReader(object? sender, EventArgs e)
        {
            OpenSelectedReader(false);
        }

        // Открытие карточки выбранного читателя
        private void OpenSelectedReader(bool viewOnly)
        {
            if (readersTable.CurrentRow == null ||
                readersTable.CurrentRow.IsNewRow)
            {
                MessageBox.Show(
                    "Сначала выберите читателя в таблице.",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            int id = Convert.ToInt32(
                readersTable.CurrentRow.Cells["Код"].Value);

            using ReaderForm form =
                new ReaderForm(id, viewOnly);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadReaders();
            }
        }

        private void ShowReaderDetails(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int id = Convert.ToInt32(
                readersTable.Rows[e.RowIndex].Cells["Код"].Value);

            using ReaderForm form =
                new ReaderForm(id, true);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadReaders();
            }
        }

        private void DeleteReader(object? sender, EventArgs e)
        {
            if (readersTable.CurrentRow == null ||
                readersTable.CurrentRow.IsNewRow)
            {
                MessageBox.Show(
                    "Сначала выберите читателя.",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(
                readersTable.CurrentRow.Cells["Код"].Value);

            string name = readersTable.CurrentRow
                .Cells["ФИО"].Value?.ToString() ?? "";

            DialogResult answer = MessageBox.Show(
                $"Удалить читателя «{name}»?\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (answer != DialogResult.Yes)
                return;

            try
            {
                int deleted = repository.DeleteReader(id);

                if (deleted > 0)
                {
                    MessageBox.Show(
                        "Читатель успешно удалён.",
                        "LibraryHub",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Удаление невозможно.\n\n" +
                        "Читатель не найден либо у него есть " +
                        "история выдачи книг или штрафы.\n\n" +
                        "Для сохранения истории библиотеки " +
                        "такие записи не удаляются.",
                        "LibraryHub",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                LoadReaders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить читателя:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}
