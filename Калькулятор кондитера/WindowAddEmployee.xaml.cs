using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SportsRentalSystem
{
    public partial class WindowAddEmployee : Window
    {
        public WindowAddEmployee()
        {
            InitializeComponent();
            cmbPosition.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (!ValidateData())
                    return;

                // Сохранение в базу данных
                using (var context = new SportsRentalSystemEntities())
                {
                    var employee = new Employees
                    {
                        LastName = txtLastName.Text.Trim(),
                        FirstName = txtFirstName.Text.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                        Position = (cmbPosition.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        Password = txtPassword.Password // В реальном приложении пароль нужно хэшировать!
                    };

                    context.Employees.Add(employee);
                    context.SaveChanges();

                    MessageBox.Show("Сотрудник успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    RefreshMainWindowData();
                    this.DialogResult = true;
                    this.Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshMainWindowData()
        {
            // Находим главное окно и обновляем его данные
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.updateEmployees();
            }
        }

        private bool ValidateData()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Введите телефон сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            // Проверка формата телефона
            if (!IsValidPhone(txtPhone.Text))
            {
                MessageBox.Show("Введите корректный номер телефона", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            // Проверка email (если указан)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            // Проверка пароля
            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmPassword.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Простая проверка - телефон должен содержать только цифры и быть не короче 10 символов
            var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
            return cleanPhone.Length >= 10;
        }

        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            // Простая реализация переключения видимости пароля
            if (txtPassword.PasswordChar == '•')
            {
                txtPassword.PasswordChar = '\0';
                btnTogglePassword.Content = "🙈";
            }
            else
            {
                txtPassword.PasswordChar = '•';
                btnTogglePassword.Content = "👁";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Перетаскивание окна
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        // Автоматическое форматирование телефона
        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Можно добавить автоматическое форматирование телефона
            // Например: +7 (XXX) XXX-XX-XX
        }
    }
}