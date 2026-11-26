using System;
using System.Collections.Generic;
using System.Windows;

namespace DemAppZharnikova
{
    public partial class ProductDialog : Window
    {
        private Dictionary<string, object> _product;

        public ProductDialog()
        {
            InitializeComponent();
        }

        // Статический метод-фабрика: открывает диалог и возвращает true, если сохранили
        public static bool Show(Window owner, Dictionary<string, object> p, bool isNew)
        {
            var dlg = new ProductDialog { Owner = owner, Title = isNew ? "Добавить товар" : "Редактировать товар" };
            dlg._product = p;
            dlg.TbArt.Text = (string)p["Article"];
            dlg.TbName.Text = (string)p["Name"];
            dlg.TbUnit.Text = (string)p["Unit"];
            dlg.TbPrice.Text = p["Price"].ToString();
            dlg.TbSupplier.Text = (string)p["Supplier"];
            dlg.TbMaker.Text = (string)p["Manufacturer"];
            dlg.TbCategory.Text = (string)p["Category"];
            dlg.TbDiscount.Text = p["Discount"].ToString();
            dlg.TbStock.Text = p["Stock"].ToString();
            dlg.TbDesc.Text = (string)p["Description"];
            dlg.TbPhoto.Text = (string)p["ImagePath"];
            return dlg.ShowDialog() == true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _product["Article"] = TbArt.Text.Trim();
                _product["Name"] = TbName.Text.Trim();
                _product["Unit"] = TbUnit.Text.Trim();
                _product["Price"] = decimal.Parse(TbPrice.Text.Replace(',', '.'));
                _product["Supplier"] = TbSupplier.Text.Trim();
                _product["Manufacturer"] = TbMaker.Text.Trim();
                _product["Category"] = TbCategory.Text.Trim();
                _product["Discount"] = int.Parse(TbDiscount.Text);
                _product["Stock"] = int.Parse(TbStock.Text);
                _product["Description"] = TbDesc.Text.Trim();
                _product["ImagePath"] = TbPhoto.Text.Trim();

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