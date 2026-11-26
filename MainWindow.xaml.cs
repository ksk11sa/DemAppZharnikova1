using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DemAppZharnikova
{
    public partial class MainWindow : Window
    {
        public static List<Dictionary<string, object>> Products = new List<Dictionary<string, object>>();
        public static List<Dictionary<string, object>> Orders = new List<Dictionary<string, object>>();

        private static bool _isDataLoaded = false;
        private static string connString = @"Data Source=HONOR\SQLEXPRESS;Initial Catalog=DemApp;Integrated Security=True;";

        private Dictionary<string, object> _user;

        public static Dictionary<string, object> MakeProduct(
            string art, string name, string unit, decimal price, string supplier,
            string maker, string category, int discount, int stock, string desc, string photo)
        {
            return new Dictionary<string, object>
            {
                {"Article", art}, {"Name", name}, {"Unit", unit}, {"Price", price},
                {"Supplier", supplier}, {"Manufacturer", maker}, {"Category", category},
                {"Discount", discount}, {"Stock", stock}, {"Description", desc}, {"ImagePath", photo},
                {"FinalPrice", Math.Round(price * (1 - discount / 100m), 2)},
                {"HasBigDiscount", discount > 15}
            };
        }

        public MainWindow(Dictionary<string, object> user)
        {
            InitializeComponent();

            _user = user;
            TbUserInfo.Text = $"Пользователь: {user["FullName"]} ({user["Role"]})";

            if (!_isDataLoaded)
            {
                LoadDataFromDatabase();
                _isDataLoaded = true;
            }

            ConfigureByRole();
            RefreshProducts();
        }

        private void LoadDataFromDatabase()
        {
            Products.Clear();
            Orders.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // 1. Загрузка товаров
                    string sqlProducts = "SELECT Article, Product_Name, Unit_of_Measure, Price, Supplier, Manufacturer, Product_Category, Current_Discount, Stock_Quantity, Product_Description, Photo FROM Tovar";
                    using (SqlCommand cmd = new SqlCommand(sqlProducts, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var photoRaw = reader["Photo"] == DBNull.Value ? null : reader["Photo"].ToString()?.Trim();

                            // Формируем правильный путь к папке photo
                            string imageUri = string.IsNullOrEmpty(photoRaw)
                                ? null
                                : "pack://application:,,,/photo/" + photoRaw;

                            Products.Add(MakeProduct(
                                reader["Article"].ToString(),
                                reader["Product_Name"].ToString(),
                                reader["Unit_of_Measure"].ToString(),
                                Convert.ToDecimal(reader["Price"]),
                                reader["Supplier"].ToString(),
                                reader["Manufacturer"].ToString(),
                                reader["Product_Category"].ToString(),
                                Convert.ToInt32(reader["Current_Discount"]),
                                Convert.ToInt32(reader["Stock_Quantity"]),
                                reader["Product_Description"].ToString(),
                                imageUri
                            ));
                        }
                    }

                    // 2. Загрузка заказов (с безопасным парсингом дат)
                    string sqlOrders = "SELECT Order_ID, Order_Article, Order_Date, Delivery_Date, Pickup_Address, Client_Name, Collection_Code, Order_Status FROM Заказ_import";
                    using (SqlCommand cmd = new SqlCommand(sqlOrders, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Безопасно обрабатываем дату (если она битая или null, выведется пустая строка)
                            string orderDateStr = "";
                            if (reader["Order_Date"] != DBNull.Value && reader["Order_Date"] != null)
                            {
                                if (DateTime.TryParse(reader["Order_Date"].ToString(), out DateTime od))
                                    orderDateStr = od.ToString("yyyy-MM-dd");
                                else
                                    orderDateStr = reader["Order_Date"].ToString(); // оставляем как есть (например, 30.02.2025)
                            }

                            string deliveryDateStr = "";
                            if (reader["Delivery_Date"] != DBNull.Value && reader["Delivery_Date"] != null)
                            {
                                if (DateTime.TryParse(reader["Delivery_Date"].ToString(), out DateTime dd))
                                    deliveryDateStr = dd.ToString("yyyy-MM-dd");
                                else
                                    deliveryDateStr = reader["Delivery_Date"].ToString();
                            }

                            Orders.Add(new Dictionary<string, object>
                            {
                                {"Number", Convert.ToInt32(reader["Order_ID"])},
                                {"Items", reader["Order_Article"].ToString()},
                                {"OrderDate", orderDateStr},
                                {"DeliveryDate", deliveryDateStr},
                                {"Pickup", reader["Pickup_Address"].ToString()},
                                {"Client", reader["Client_Name"].ToString()},
                                {"Code", reader["Collection_Code"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Collection_Code"])},
                                {"Status", reader["Order_Status"].ToString()}
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к БД: " + ex.Message, "Ошибка");
            }
        }

        private void ConfigureByRole()
        {
            var role = (string)_user["Role"];
            var isAdmin = role == "Администратор";
            var isManager = role == "Менеджер";

            FilterPanel.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            BtnOrders.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            BtnAdd.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnEdit.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnDelete.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            if (isAdmin || isManager)
            {
                CbCategory.SelectionChanged -= CbCategory_SelectionChanged;
                CbCategory.Items.Clear();
                CbCategory.Items.Add("Все");
                foreach (var c in Products.Select(p => (string)p["Category"]).Distinct().OrderBy(x => x))
                    CbCategory.Items.Add(c);
                CbCategory.SelectedIndex = 0;
                CbCategory.SelectionChanged += CbCategory_SelectionChanged;
            }
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => RefreshProducts();
        private void CbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshProducts();
        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshProducts();

        private void RefreshProducts()
        {
            if (DgProducts == null) return;

            IEnumerable<Dictionary<string, object>> data = Products;

            if (FilterPanel.Visibility == Visibility.Visible)
            {
                var q = TbSearch.Text?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(q))
                    data = data.Where(p => ((string)p["Name"]).ToLower().Contains(q)
                                        || ((string)p["Article"]).ToLower().Contains(q));

                if (CbCategory.SelectedItem is string cat && cat != "Все")
                    data = data.Where(p => (string)p["Category"] == cat);

                string sort = "";
                if (CbSort.SelectedItem is ComboBoxItem item) sort = item.Content.ToString();

                switch (sort)
                {
                    case "Цена ↑": data = data.OrderBy(p => (decimal)p["Price"]); break;
                    case "Цена ↓": data = data.OrderByDescending(p => (decimal)p["Price"]); break;
                    case "Скидка ↑": data = data.OrderBy(p => (int)p["Discount"]); break;
                    case "Скидка ↓": data = data.OrderByDescending(p => (int)p["Discount"]); break;
                }
            }

            DgProducts.ItemsSource = data.ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var p = MakeProduct("", "", "шт.", 0, "", "", "", 0, 0, "", "");
            if (ProductDialog.Show(this, p, true))
            {
                RecalculateProduct(p);
                Products.Add(p);
                SaveProductToDb(p);
                RefreshProducts();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgProducts.SelectedItem is Dictionary<string, object> p)
            {
                if (ProductDialog.Show(this, p, false))
                {
                    RecalculateProduct(p);
                    UpdateProductInDb(p);
                    RefreshProducts();
                }
            }
            else MessageBox.Show("Выберите товар");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgProducts.SelectedItem is Dictionary<string, object> p)
            {
                if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DeleteProductFromDb(p);
                    Products.Remove(p);
                    RefreshProducts();
                }
            }
            else MessageBox.Show("Выберите товар");
        }

        private static void RecalculateProduct(Dictionary<string, object> p)
        {
            var price = (decimal)p["Price"];
            var disc = (int)p["Discount"];
            p["FinalPrice"] = Math.Round(price * (1 - disc / 100m), 2);
            p["HasBigDiscount"] = disc > 15;
        }

        // --- CRUD ДЛЯ ТОВАРОВ В БД ---
        private void SaveProductToDb(Dictionary<string, object> p)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO Tovar (Article, Product_Name, Unit_of_Measure, Price, Supplier, Manufacturer, Product_Category, Current_Discount, Stock_Quantity, Product_Description, Photo) 
                                   VALUES (@art, @name, @unit, @price, @sup, @man, @cat, @disc, @stock, @desc, @photo)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@art", p["Article"]);
                        cmd.Parameters.AddWithValue("@name", p["Name"]);
                        cmd.Parameters.AddWithValue("@unit", p["Unit"]);
                        cmd.Parameters.AddWithValue("@price", p["Price"]);
                        cmd.Parameters.AddWithValue("@sup", p["Supplier"]);
                        cmd.Parameters.AddWithValue("@man", p["Manufacturer"]);
                        cmd.Parameters.AddWithValue("@cat", p["Category"]);
                        cmd.Parameters.AddWithValue("@disc", p["Discount"]);
                        cmd.Parameters.AddWithValue("@stock", p["Stock"]);
                        cmd.Parameters.AddWithValue("@desc", p["Description"]);

                        // Сохраняем только имя файла, без префикса
                        string photoName = p["ImagePath"]?.ToString()?.Replace("pack://application:,,,/photo/", "").Trim() ?? "";
                        cmd.Parameters.AddWithValue("@photo", photoName);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка добавления: " + ex.Message); }
        }

        private void UpdateProductInDb(Dictionary<string, object> p)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = @"UPDATE Tovar SET Product_Name=@name, Unit_of_Measure=@unit, Price=@price, Supplier=@sup, Manufacturer=@man, Product_Category=@cat, Current_Discount=@disc, Stock_Quantity=@stock, Product_Description=@desc, Photo=@photo 
                                   WHERE Article=@art";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@art", p["Article"]);
                        cmd.Parameters.AddWithValue("@name", p["Name"]);
                        cmd.Parameters.AddWithValue("@unit", p["Unit"]);
                        cmd.Parameters.AddWithValue("@price", p["Price"]);
                        cmd.Parameters.AddWithValue("@sup", p["Supplier"]);
                        cmd.Parameters.AddWithValue("@man", p["Manufacturer"]);
                        cmd.Parameters.AddWithValue("@cat", p["Category"]);
                        cmd.Parameters.AddWithValue("@disc", p["Discount"]);
                        cmd.Parameters.AddWithValue("@stock", p["Stock"]);
                        cmd.Parameters.AddWithValue("@desc", p["Description"]);

                        string photoName = p["ImagePath"]?.ToString()?.Replace("pack://application:,,,/photo/", "").Trim() ?? "";
                        cmd.Parameters.AddWithValue("@photo", photoName);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка обновления: " + ex.Message); }
        }

        private void DeleteProductFromDb(Dictionary<string, object> p)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Tovar WHERE Article=@art";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@art", p["Article"]);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrdersWindow(_user).Show();
            Close();

        }

        private void ImgPhoto_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Icon.ico"));
        }
    }
    }
