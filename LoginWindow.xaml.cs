using korzunov.models;
using System.Windows;

namespace korzunov
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorText.Text = "Введите логин и пароль.";
                return;
            }

            User user = DbHelper.Login(LoginBox.Text, PasswordBox.Password);
            if (user == null)
            {
                ErrorText.Text = "Пользователь не найден.";
                return;
            }

            new MainWindow(user).Show();
            this.Close();
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            User guest = new User();
            guest.FullName = "Гость";
            guest.RoleName = "Гость";
            new MainWindow(guest).Show();
            this.Close();
        }
    }
}