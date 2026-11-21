
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace LibraryHub
{
    public partial class BookForm : Form
    {
        private readonly TextBox nameBox = new TextBox();
        private readonly TextBox yearBox = new TextBox();
        private readonly ComboBox publisherBox = new ComboBox();
        private readonly ComboBox categoryBox = new ComboBox();

        private readonly int? bookId;

        private readonly BookRepository repository = new BookRepository();

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
            try
            {
                DataTable table = repository.GetPublishers();

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
            try
            {
                DataTable table = repository.GetCategories();

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
            try
            {
                DataTable details = repository.GetBookDetails(bookId!.Value);
                if (details.Rows.Count == 0)
                {
                    MessageBox.Show("Книга не найдена.", "LibraryHub",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataRow reader = details.Rows[0];

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

            try
            {
                int affectedRows = repository.SaveBook(
                    bookId, name, year,
                    Convert.ToInt32(publisherBox.SelectedValue),
                    categoryBox.SelectedValue.ToString()!);

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
