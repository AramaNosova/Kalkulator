using System;
using System.Linq;
using System.Windows;

namespace SportsRentalSystem
{
    public partial class WindowViewEmployee : Window
    {
        private Employees employees;
        public WindowViewEmployee()
        {
            InitializeComponent();
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            if (App.selectedEmployee != null)
            {
                var employee = App.selectedEmployee;

                txtLastName.Text = employee.LastName;
                txtFirstName.Text = employee.FirstName;
                txtMiddleName.Text = employee.MiddleName ?? "Не указано";
                txtPhone.Text = employee.Phone;
                txtEmail.Text = employee.Email ?? "Не указано";
                txtPosition.Text = employee.Position;
                txtEmployeeID.Text = employee.EmployeeID.ToString();
            }
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

        private void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (employees != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить оборудование \"{employees.FirstName}\"?\n\nЭто действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new SportsRentalSystemEntities())
                        {
                            // Находим оборудование в базе данных
                            var employeestToDelete = context.Employees.Find(employees.EmployeeID);

                            if (employeestToDelete != null)
                            {

                                // Удаляем оборудование
                                context.Employees.Remove(employeestToDelete);
                                context.SaveChanges();

                                MessageBox.Show("Оборудование успешно удалено!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                this.DialogResult = true;
                                this.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно редактирования сотрудника
            WindowEditEmployee editWindow = new WindowEditEmployee();
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные после редактирования
                LoadEmployeeData();

                // Уведомляем главное окно об обновлении
                RefreshMainWindowData();
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
    }
}
