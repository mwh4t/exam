using korzunov.models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace korzunov
{
    public partial class MainWindow : Window
    {
        private User _user;

        public MainWindow(User user)
        {
            InitializeComponent();
            _user = user;
            UserLabel.Text = user.FullName + " (" + user.RoleName + ")";

            bool canFilter = user.RoleName == "Менеджер" || user.RoleName == "Администратор";
            FilterPanel.Visibility = canFilter ? Visibility.Visible : Visibility.Collapsed;
            AdminPanel.Visibility = user.RoleName == "Администратор" ? Visibility.Visible : Visibility.Collapsed;

            if (canFilter)
            {
                List<string> suppliers = new List<string>();
                suppliers.Add("Все поставщики");
                suppliers.AddRange(DbHelper.GetNames("supplier"));
                SupplierBox.ItemsSource = suppliers;
                SupplierBox.SelectedIndex = 0;

                SortBox.ItemsSource = new string[] {
                    "Без сортировки", "Кол-во по возрастанию", "Кол-во по убыванию"
                };
                SortBox.SelectedIndex = 0;
            }

            Refresh();
        }

        private void Refresh()
        {
            string q = SearchBox.Text ?? "";
            string sup = SupplierBox.SelectedItem as string ?? "Все поставщики";
            ProductsGrid.ItemsSource = DbHelper.GetProducts(q, sup, SortBox.SelectedIndex);
        }

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

        private void ProductsGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_user.RoleName != "Администратор") return;
            if (ProductsGrid.SelectedItem == null) return;

            Product p = (Product)ProductsGrid.SelectedItem;
            new AddEditProductWindow(p).ShowDialog();
            Refresh();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            new AddEditProductWindow(null).ShowDialog();
            Refresh();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар.");
                return;
            }
            Product p = (Product)ProductsGrid.SelectedItem;
            if (DbHelper.IsInOrder(p.Article))
            {
                MessageBox.Show("Нельзя удалить товар, который есть в заказах.");
                return;
            }
            MessageBoxResult res = MessageBox.Show("Удалить " + p.Name + "?", "Подтверждение",
                MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                DbHelper.DeleteProduct(p.Article);
                Refresh();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void Search_Changed(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void Combo_Changed(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }
    }
}