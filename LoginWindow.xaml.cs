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

