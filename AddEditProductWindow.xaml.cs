using korzunov.models;
using System.Windows;

namespace korzunov
{
    public partial class AddEditProductWindow : Window
    {
        private Product _existing;

        public AddEditProductWindow(Product existing)
        {
            InitializeComponent();
            _existing = existing;

            CategoryBox.ItemsSource = DbHelper.GetNames("category");
            ManufacturerBox.ItemsSource = DbHelper.GetNames("manufacturer");
            SupplierBox.ItemsSource = DbHelper.GetNames("supplier");

            if (existing != null)
            {
                Title = "Редактирование товара";
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
                Title = "Добавление товара";
                CategoryBox.SelectedIndex = 0;
                ManufacturerBox.SelectedIndex = 0;
                SupplierBox.SelectedIndex = 0;
                DiscountBox.Text = "0";
                StockBox.Text = "0";
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

            decimal price;
            if (!decimal.TryParse(PriceBox.Text, out price) || price < 0)
            {
                ErrorText.Text = "Цена должна быть неотрицательным числом.";
                return;
            }

            int stock;
            if (!int.TryParse(StockBox.Text, out stock) || stock < 0)
            {
                ErrorText.Text = "Количество должно быть неотрицательным числом.";
                return;
            }

            int discount;
            int.TryParse(DiscountBox.Text, out discount);

            if (CategoryBox.SelectedItem == null ||
                ManufacturerBox.SelectedItem == null ||
                SupplierBox.SelectedItem == null)
            {
                ErrorText.Text = "Выберите категорию, производителя и поставщика.";
                return;
            }

            Product p = new Product();
            p.Article = ArticleBox.Text.Trim();
            p.Name = NameBox.Text.Trim();
            p.Description = DescBox.Text;
            p.Price = price;
            p.Discount = discount;
            p.Stock = stock;
            p.CategoryName = CategoryBox.SelectedItem.ToString();
            p.ManufacturerName = ManufacturerBox.SelectedItem.ToString();
            p.SupplierName = SupplierBox.SelectedItem.ToString();

            DbHelper.SaveProduct(p, _existing == null);
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}