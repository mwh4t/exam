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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace exam
{
    public partial class MainWindow : Window
    {
        private User _user;
        private List<Product> _allProducts = new List<Product>();

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;

            UserLabel.Text = user.FullName + " (" + user.RoleName + ")";

            // Скрываем поиск/фильтр для гостя и клиента
            bool canFilter = user.RoleName == "Менеджер" ||
                             user.RoleName == "Администратор";
            FilterPanel.Visibility = canFilter ? Visibility.Visible : Visibility.Collapsed;

            // Скрываем кнопки администратора
            AdminPanel.Visibility = user.RoleName == "Администратор"
                ? Visibility.Visible : Visibility.Collapsed;

            if (canFilter)
            {
                SupplierBox.ItemsSource = DbHelper.GetSuppliers();
                SupplierBox.SelectedIndex = 0;

                SortBox.ItemsSource = new[] { "Без сортировки", "Кол-во ↑", "Кол-во ↓" };
                SortBox.SelectedIndex = 0;
            }

            LoadProducts();
        }

        private void LoadProducts()
        {
            _allProducts = DbHelper.GetProducts();
            Refresh();
        }

        private void Refresh()
        {
            List<Product> result = new List<Product>();

            foreach (Product p in _allProducts)
            {
                // Поиск по всем текстовым полям
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    string q = SearchBox.Text.ToLower();
                    bool match = p.Article.ToLower().Contains(q) ||
                                 p.Name.ToLower().Contains(q) ||
                                 p.Description.ToLower().Contains(q) ||
                                 p.CategoryName.ToLower().Contains(q) ||
                                 p.SupplierName.ToLower().Contains(q) ||
                                 p.ManufacturerName.ToLower().Contains(q);
                    if (!match) continue;
                }

                // Фильтр по поставщику
                string supplier = SupplierBox.SelectedItem != null
                    ? SupplierBox.SelectedItem.ToString() : "Все поставщики";
                if (supplier != "Все поставщики" && p.SupplierName != supplier)
                    continue;

                result.Add(p);
            }

            // Сортировка по складу
            string sort = SortBox.SelectedItem != null
                ? SortBox.SelectedItem.ToString() : "Без сортировки";
            if (sort == "Кол-во ↑")
                result = result.OrderBy(p => p.Stock).ToList();
            else if (sort == "Кол-во ↓")
                result = result.OrderByDescending(p => p.Stock).ToList();

            ProductsGrid.ItemsSource = result;
        }

        // Подсветка строк
        private void ProductsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Product p = e.Row.Item as Product;
            if (p == null) return;

            if (p.Stock == 0)
                e.Row.Background = new SolidColorBrush(Colors.LightBlue);
            else if (p.Discount > 15)
                e.Row.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57));
            else
                e.Row.Background = new SolidColorBrush(Colors.White);
        }

        // Двойной клик — открыть редактирование (только админ)
        private void ProductsGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_user.RoleName != "Администратор") return;
            if (ProductsGrid.SelectedItem == null) return;

            Product p = (Product)ProductsGrid.SelectedItem;
            AddEditProductWindow window = new AddEditProductWindow(p);
            window.ShowDialog();
            LoadProducts();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditProductWindow window = new AddEditProductWindow(null);
            window.ShowDialog();
            LoadProducts();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления.", "Ошибка");
                return;
            }

            Product p = (Product)ProductsGrid.SelectedItem;

            if (DbHelper.IsProductInOrder(p.Article))
            {
                MessageBox.Show("Нельзя удалить товар, который есть в заказах.", "Ошибка");
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Удалить товар " + p.Name + "?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                DbHelper.DeleteProduct(p.Article);
                LoadProducts();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => Refresh();
        private void SupplierBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => Refresh();
        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => Refresh();
    }
}

