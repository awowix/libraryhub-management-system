
using Microsoft.Data.SqlClient;

namespace LibraryHub
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Text = "LibraryHub — Библиотечная система";
            Width = 650;
            Height = 350;
            StartPosition = FormStartPosition.CenterScreen;

            Button button = new Button();

            button.Text = "Проверить подключение к БД";
            button.Width = 300;
            button.Height = 50;
            button.Left = 160;
            button.Top = 110;

            button.Click += CheckConnection;

            Controls.Add(button);
        }

        private void CheckConnection(object? sender, EventArgs e)
        {
            string connectionString =
                "Server=sheelpek;" +
                "Database=biblioteka;" +
                "Integrated Security=True;" +
                "Encrypt=True;" +
                "TrustServerCertificate=True;";

            try
            {
                using SqlConnection connection =
                    new SqlConnection(connectionString);

                connection.Open();

                MessageBox.Show(
                    "Подключение к biblioteka успешно!",
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка подключения:\n" + ex.Message,
                    "LibraryHub",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
