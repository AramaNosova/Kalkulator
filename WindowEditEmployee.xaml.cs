using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SportsRentalSystem
{
    public partial class WindowEditEmployee : Window
    {
        private Employees currentEmployee;

        public WindowEditEmployee()
        {
            InitializeComponent();
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            if (App.selectedEmployee != null)
            {
                currentEmployee = App.selectedEmployee;

                // Заполняем поля данными выбранного сотрудника
                txtLastName.Text = currentEmployee.LastName;
                txtFirstName.Text = currentEmployee.FirstName;
                txtMiddleName.Text = currentEmployee.MiddleName ?? "";
                txtPhone.Text = currentEmployee.Phone;
                txtEmail.Text = currentEmployee.Email ?? "";

                // Устанавливаем выбранную должность
                foreach (ComboBoxItem item in cmbPosition.Items)
                {
                    if (item.Content.ToString() == currentEmployee.Position)
                    {
                        cmbPosition.SelectedItem = item;
                        break;
                    }
                }

                // Если не нашли должность, выбираем первую
                if (cmbPosition.SelectedItem == null)
                    cmbPosition.SelectedIndex = 0;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (!ValidateData())
                    return;

                // Обновление данных в базе
                using (var context = new SportsRentalSystemEntities())
                {
                    var employeeToUpdate = context.Employees.Find(currentEmployee.EmployeeID);

                    if (employeeToUpdate != null)
                    {
                        employeeToUpdate.LastName = txtLastName.Text.Trim();
                        employeeToUpdate.FirstName = txtFirstName.Text.Trim();
                        employeeToUpdate.MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
                        employeeToUpdate.Phone = txtPhone.Text.Trim();
                        employeeToUpdate.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                        employeeToUpdate.Position = (cmbPosition.SelectedItem as ComboBoxItem)?.Content.ToString();

                        // Обновляем пароль только если он указан
                        if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                        {
                            employeeToUpdate.Password = txtPassword.Password;
                        }

                        context.SaveChanges();

                        MessageBox.Show("Данные сотрудника успешно обновлены!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Уведомляем главное окно об обновлении
                        RefreshMainWindowData();

                        this.DialogResult = true;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Проверка пароля (если указан)
            if (!string.IsNullOrWhiteSpace(txtPassword.Password) && txtPassword.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
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

        private void RefreshMainWindowData()
        {
            // Находим главное окно и обновляем его данные
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.updateEmployees();
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

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}