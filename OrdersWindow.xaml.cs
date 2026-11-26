using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DemAppZharnikova
{
    public partial class OrdersWindow : Window
    {
        private Dictionary<string, object> _user;

        public OrdersWindow(Dictionary<string, object> user)
        {
            InitializeComponent();
            _user = user;

            var isAdmin = (string)user["Role"] == "Администратор";
            BtnAdd.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnEdit.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            BtnDelete.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            RefreshOrders();
        }

        private void RefreshOrders()
        {
            DgOrders.ItemsSource = null;
            DgOrders.ItemsSource = MainWindow.Orders;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow(_user).Show();
            Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var next = MainWindow.Orders.Count == 0 ? 1 : MainWindow.Orders.Max(ord => (int)ord["Number"]) + 1;

            var o = new Dictionary<string, object>
    {
        {"Number", next}, {"Items",""}, {"OrderDate",""},
        {"DeliveryDate",""}, {"Pickup",""}, {"Client",""},
        {"Code",0}, {"Status","Новый"}
    };

            if (OrderDialog.Show(this, o, true))
            {
                MainWindow.Orders.Add(o);
                RefreshOrders();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgOrders.SelectedItem is Dictionary<string, object> o)
            {
                if (OrderDialog.Show(this, o, false)) RefreshOrders();
            }
            else MessageBox.Show("Выберите заказ");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgOrders.SelectedItem is Dictionary<string, object> o)
            {
                if (MessageBox.Show("Удалить заказ?", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MainWindow.Orders.Remove(o);
                    RefreshOrders();
                }
            }
            else MessageBox.Show("Выберите заказ");
        }
    }
}