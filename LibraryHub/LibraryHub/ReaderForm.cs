
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public class ReaderForm : Form
    {
        private readonly ReaderRepository repository = new ReaderRepository();
        private readonly int? readerId;
        private bool viewing;

        private readonly TextBox nameBox = new TextBox();
        private readonly TextBox phoneBox = new TextBox();
        private readonly TextBox addressBox = new TextBox();
        private readonly TextBox workplaceBox = new TextBox();
        private readonly TextBox professionBox = new TextBox();
        private readonly ComboBox genderBox = new ComboBox();
        private readonly DateTimePicker birthPicker = new DateTimePicker();
        private readonly CheckBox unknownBirth = new CheckBox();

        private readonly Panel content = new Panel();
        private readonly Label heading = new Label();

        private readonly Color sage = Color.FromArgb(85, 119, 101);
        private readonly Color dark = Color.FromArgb(41, 59, 49);
        private readonly Color light = Color.FromArgb(229, 237, 229);
        private readonly Color background = Color.FromArgb(248, 248, 245);

        public ReaderForm(int? id = null, bool viewOnly = false)
        {
            readerId = id;
            viewing = viewOnly && id.HasValue;

            Text = "LibraryHub — Читатель";
            Size = new Size(680, 690);
            MinimumSize = new Size(600, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = background;
            Font = new Font("Segoe UI", 10);

            genderBox.DropDownStyle = ComboBoxStyle.DropDownList;
            genderBox.Items.AddRange(new object[] { "Не указан", "М", "Ж" });
            genderBox.SelectedIndex = 0;

            birthPicker.Format = DateTimePickerFormat.Short;
            birthPicker.Width = 150;
            unknownBirth.Text = "Не указана";
            unknownBirth.AutoSize = true;
            unknownBirth.Checked = true;
            unknownBirth.CheckedChanged += (s, e) =>
                birthPicker.Enabled = !unknownBirth.Checked;

            if (readerId.HasValue)
                LoadReader();

            BuildInterface();
        }

        private void BuildInterface()
        {
            Controls.Clear();

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 20, 28, 20),
                ColumnCount = 1,
                RowCount = 3
            };

            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));

            Controls.Add(root);

            Panel header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = background
            };

            heading.Text = viewing
                ? nameBox.Text
                : readerId.HasValue
                    ? "Редактирование читателя"
                    : "Новый читатель";

            heading.Font = new Font("Segoe UI", 21, FontStyle.Bold);
            heading.ForeColor = dark;
            heading.Dock = DockStyle.Top;
            heading.Height = 55;
            heading.AutoEllipsis = true;

            Label subtitle = new Label
            {
                Text = readerId.HasValue
                    ? $"Читатель №{readerId.Value}"
                    : "Заполните данные нового читателя",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top,
                Height = 30
            };

            header.Controls.Add(subtitle);
            header.Controls.Add(heading);
            root.Controls.Add(header, 0, 0);

            content.Dock = DockStyle.Fill;
            content.BackColor = Color.White;
            content.Padding = new Padding(20);
            content.AutoScroll = true;
            root.Controls.Add(content, 0, 1);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 0)
            };

            Button close = MakeButton("Закрыть", false);
            close.Click += (s, e) => Close();
            buttons.Controls.Add(close);

            if (viewing)
            {
                Button edit = MakeButton("Редактировать", true);
                edit.Click += (s, e) =>
                {
                    viewing = false;
                    BuildInterface();
                };
                buttons.Controls.Add(edit);
            }
            else
            {
                Button save = MakeButton("Сохранить", true);
                save.Click += SaveReader;
                buttons.Controls.Add(save);
            }

            root.Controls.Add(buttons, 0, 2);

            if (viewing)
                ShowDetails();
            else
                ShowEditor();
        }

        private Button MakeButton(string text, bool primary)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(145, 40),
                BackColor = primary ? sage : light,
                ForeColor = primary ? Color.White : dark,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };

            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private TableLayoutPanel MakeGrid(int rows)
        {
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = rows,
                BackColor = Color.White
            };

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < rows; i++)
                grid.RowStyles.Add(
                    new RowStyle(SizeType.Absolute, 90));

            return grid;
        }

        private void AddDetail(
            TableLayoutPanel grid,
            string caption,
            string value,
            int column,
            int row)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 12, 5)
            };

            Label label = new Label
            {
                Text = caption,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Top,
                Height = 25
            };

            Label data = new Label
            {
                Text = string.IsNullOrWhiteSpace(value)
                    ? "Не указано" : value,
                ForeColor = dark,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };

            panel.Controls.Add(data);
            panel.Controls.Add(label);
            grid.Controls.Add(panel, column, row);
        }

        private void ShowDetails()
        {
            content.Controls.Clear();

            TableLayoutPanel grid = MakeGrid(4);

            AddDetail(grid, "ФИО", nameBox.Text, 0, 0);
            AddDetail(grid, "Телефон", phoneBox.Text, 1, 0);
            AddDetail(grid, "Адрес проживания", addressBox.Text, 0, 1);

            string date = unknownBirth.Checked
                ? "Не указано"
                : birthPicker.Value.ToString("dd.MM.yyyy");

            AddDetail(grid, "Дата рождения", date, 1, 1);
            AddDetail(grid, "Пол", genderBox.Text, 0, 2);
            AddDetail(grid, "Профессия", professionBox.Text, 1, 2);
            AddDetail(grid, "Место работы / учёбы",
                workplaceBox.Text, 0, 3);

            content.Controls.Add(grid);
        }

        private void AddEditor(
            TableLayoutPanel grid,
            string caption,
            Control input,
            int column,
            int row)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 12, 5)
            };

            Label label = new Label
            {
                Text = caption,
                ForeColor = dark,
                Dock = DockStyle.Top,
                Height = 25
            };

            input.Dock = DockStyle.Top;
            input.Height = 32;
            input.Font = new Font("Segoe UI", 10);

            panel.Controls.Add(input);
            panel.Controls.Add(label);
            grid.Controls.Add(panel, column, row);
        }

        private void ShowEditor()
        {
            content.Controls.Clear();

            TableLayoutPanel grid = MakeGrid(4);

            AddEditor(grid, "ФИО *", nameBox, 0, 0);
            AddEditor(grid, "Телефон", phoneBox, 1, 0);
            AddEditor(grid, "Адрес проживания", addressBox, 0, 1);

            FlowLayoutPanel birthPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false
            };

            birthPanel.Controls.Add(birthPicker);
            birthPanel.Controls.Add(unknownBirth);

            AddEditor(grid, "Дата рождения", birthPanel, 1, 1);
            AddEditor(grid, "Пол", genderBox, 0, 2);
            AddEditor(grid, "Профессия", professionBox, 1, 2);
            AddEditor(grid, "Место работы / учёбы",
                workplaceBox, 0, 3);

            content.Controls.Add(grid);
        }

        private void LoadReader()
        {
            try
            {
                DataRow? row =
                    repository.GetReaderById(readerId!.Value);

                if (row == null)
                {
                    MessageBox.Show("Читатель не найден.");
                    Shown += (s, e) => Close();
                    return;
                }

                nameBox.Text = row["ФИО"].ToString();
                phoneBox.Text = row["Телефон"].ToString();
                addressBox.Text = row["Адрес"].ToString();
                workplaceBox.Text =
                    row["Место работы/учёбы"].ToString();
                professionBox.Text = row["Профессия"].ToString();

                string gender = row["Пол"].ToString() ?? "";
                genderBox.SelectedItem =
                    gender == "М" || gender == "Ж"
                        ? gender : "Не указан";

                if (row["Дата рождения"] != DBNull.Value)
                {
                    birthPicker.Value =
                        Convert.ToDateTime(row["Дата рождения"]);
                    unknownBirth.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки:\n" + ex.Message);
            }
        }

        private void SaveReader(object? sender, EventArgs e)
        {
            string name = nameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите ФИО читателя.");
                nameBox.Focus();
                return;
            }

            if (name.Length > 100 ||
                phoneBox.Text.Trim().Length > 20 ||
                addressBox.Text.Trim().Length > 200 ||
                workplaceBox.Text.Trim().Length > 150 ||
                professionBox.Text.Trim().Length > 100)
            {
                MessageBox.Show("Превышена допустимая длина поля.");
                return;
            }

            DateTime? birthDate = unknownBirth.Checked
                ? null : birthPicker.Value.Date;

            string? gender = genderBox.SelectedIndex == 0
                ? null : genderBox.Text;

            try
            {
                int result = readerId.HasValue
                    ? repository.UpdateReader(
                        readerId.Value, name, phoneBox.Text,
                        addressBox.Text, birthDate, gender,
                        workplaceBox.Text, professionBox.Text)
                    : repository.AddReader(
                        name, phoneBox.Text,
                        addressBox.Text, birthDate, gender,
                        workplaceBox.Text, professionBox.Text);

                if (result == 0)
                {
                    MessageBox.Show("Запись не найдена.");
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex)
                when (ex.Number == 2601 || ex.Number == 2627)
            {
                MessageBox.Show("Такой телефон уже зарегистрирован.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения:\n" + ex.Message);
            }
        }
    }
}
