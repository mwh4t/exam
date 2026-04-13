using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace exam
{
    public partial class AddEditProductWindow : Window
    {
        private Product _existing; // null = добавление, не null = редактирование

        public AddEditProductWindow(Product existing)
        {
            InitializeComponent();
            _existing = existing;

            CategoryBox.ItemsSource = DbHelper.GetCategories();
            ManufacturerBox.ItemsSource = DbHelper.GetManufacturers();
            SupplierBox.ItemsSource = DbHelper.GetSuppliers()
                .FindAll(s => s != "Все поставщики");

            if (existing != null)
            {
                // Редактирование — заполняем поля
                Title = "Редактировать товар";
                ArticleBox.Text = existing.Article;
                ArticleBox.IsReadOnly = true;
                NameBox.Text = existing.Name;
                DescBox.Text = existing.Description;
                PriceBox.Text = existing.Price.ToString();
                DiscountBox.Text = existing.Discount.ToString();
                StockBox.Text = existing.Stock.ToString();
                CategoryBox.SelectedItem = existing.CategoryName;
                ManufacturerBox.SelectedItem = existing.ManufacturerName;
                SupplierBox.SelectedItem = existing.SupplierName;
            }
            else
            {
                Title = "Добавить товар";
                CategoryBox.SelectedIndex = 0;
                ManufacturerBox.SelectedIndex = 0;
                SupplierBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(ArticleBox.Text) ||
                string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ErrorText.Text = "Заполните артикул и наименование.";
                return;
            }

            decimal price = 0;
            decimal.TryParse(PriceBox.Text, out price);
            if (price < 0)
            {
                ErrorText.Text = "Цена не может быть отрицательной.";
                return;
            }

            int stock = 0;
            int.TryParse(StockBox.Text, out stock);
            if (stock < 0)
            {
                ErrorText.Text = "Количество не может быть отрицательным.";
                return;
            }

            int discount = 0;
            int.TryParse(DiscountBox.Text, out discount);

            Product p = new Product();
            p.Article = ArticleBox.Text.Trim();
            p.Name = NameBox.Text.Trim();
            p.Description = DescBox.Text.Trim();
            p.Price = price;
            p.Discount = discount;
            p.Stock = stock;
            p.CategoryName = CategoryBox.SelectedItem.ToString();
            p.ManufacturerName = ManufacturerBox.SelectedItem.ToString();
            p.SupplierName = SupplierBox.SelectedItem.ToString();

            if (_existing == null)
                DbHelper.AddProduct(p);
            else
                DbHelper.UpdateProduct(p);

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}

