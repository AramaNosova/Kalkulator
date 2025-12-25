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
using MahApps.Metro.IconPacks.Converter;
using MahApps.Metro.IconPacks.Utils;
using MahApps.Metro.IconPacks;


namespace SportsRentalSystem
{
    /// <summary>
    /// Логика взаимодействия для WindonLogin.xaml
    /// </summary>
    public partial class WindonLogin : Window
    {
        public WindonLogin()
        {
            InitializeComponent();
        }

        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                // Переключаем на скрытый режим (PasswordBox)
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordVisible.Text = txtPassword.Password;

                // Меняем иконку на "глаз закрыт"
                var icon = btnTogglePassword.Content as PackIconMaterial;
                if (icon != null)
                {
                    icon.Kind = PackIconMaterialKind.EyeOff;
                }

                btnTogglePassword.ToolTip = "Показать пароль";
            }
            else
            {
                // Переключаем на видимый режим (TextBox)
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Password = txtPasswordVisible.Text;

                // Меняем иконку на "глаз открыт"
                var icon = btnTogglePassword.Content as PackIconMaterial;
                if (icon != null)
                {
                    icon.Kind = PackIconMaterialKind.Eye;
                }

                btnTogglePassword.ToolTip = "Скрыть пароль";
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginUser = App.sports.Employees
                .Where(p => p.Email == txtUsername.Text &&
                        (p.Password == txtPassword.Password ||
                        p.Password == txtPasswordVisible.Text))
                .FirstOrDefault();

            if (loginUser != null)
            {
                App.employees = loginUser;
                MainWindow window = new MainWindow();
                window.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Неверно введён логин или пароль");
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Логика регистрации
        }

        private void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // Логика восстановления пароля
        }

        // Обработчики для кнопок управления окном (уже должны быть)
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Maximize(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
