using System;
using System.Collections.Generic;
using System.Windows;

namespace DemAppZharnikova
{
    public partial class OrderDialog : Window
    {
        private Dictionary<string, object> _order;

        public OrderDialog() { InitializeComponent(); }

        public static bool Show(Window owner, Dictionary<string, object> o, bool isNew)
        {
            var dlg = new OrderDialog { Owner = owner, Title = isNew ? "Добавить заказ" : "Редактировать заказ" };
            dlg._order = o;
            dlg.TbNumber.Text = o["Number"].ToString();
            dlg.TbItems.Text = (string)o["Items"];
            dlg.TbDate.Text = (string)o["OrderDate"];
            dlg.TbDelivery.Text = (string)o["DeliveryDate"];
            dlg.TbPickup.Text = (string)o["Pickup"];
            dlg.TbClient.Text = (string)o["Client"];
            dlg.TbCode.Text = o["Code"].ToString();
            dlg.TbStatus.Text = (string)o["Status"];
            return dlg.ShowDialog() == true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _order["Number"] = int.Parse(TbNumber.Text);
                _order["Items"] = TbItems.Text.Trim();
                _order["OrderDate"] = TbDate.Text.Trim();
                _order["DeliveryDate"] = TbDelivery.Text.Trim();
                _order["Pickup"] = TbPickup.Text.Trim();
                _order["Client"] = TbClient.Text.Trim();
                _order["Code"] = int.Parse(TbCode.Text);
                _order["Status"] = TbStatus.Text.Trim();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}