using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace DemAppZharnikova
{
    public partial class LoginWindow : Window
    {
        // Список для хранения пользователей
        public static List<Dictionary<string, object>> Users = new List<Dictionary<string, object>>();

        // Строка подключения к вашей БД (проверьте имя сервера, если нужно)
        private string connString = @"Data Source=HONOR\SQLEXPRESS;Initial Catalog=DemApp;Integrated Security=True;";

        public LoginWindow()
        {
            InitializeComponent();
            LoadUsersFromDb();
        }

        private void LoadUsersFromDb()
        {
            Users.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    // Запрос к таблице user_import
                    string sql = "SELECT Role, FIO, Login, Password FROM user_import";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Users.Add(new Dictionary<string, object>
                            {
                                {"Role", reader["Role"].ToString()},
                                {"FullName", reader["FIO"].ToString()},
                                {"Login", reader["Login"].ToString()},
                                {"Password", reader["Password"].ToString()}
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей из БД: " + ex.Message, "Ошибка");
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = TbLogin.Text.Trim();
            var password = PbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                TbError.Text = "Введите логин и пароль!";
                return;
            }

            var user = Users.FirstOrDefault(u =>
                (string)u["Login"] == login && (string)u["Password"] == password);

            if (user == null)
            {
                TbError.Text = "Неверный логин или пароль!";
                return;
            }

            new MainWindow(user).Show();
            Close();
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            var guest = new Dictionary<string, object>
            {
                {"Role","Гость"},{"FullName","Гость"},{"Login",""},{"Password",""}
            };
            new MainWindow(guest).Show();
            Close();
        }
    }
}