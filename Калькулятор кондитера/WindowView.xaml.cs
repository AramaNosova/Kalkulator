using System;
using System.Linq;
using System.Windows;

namespace SportsRentalSystem
{
    public partial class WindowView : Window
    {
        private Equipment currentEquipment;

        public WindowView()
        {
            InitializeComponent();
            LoadEquipmentData();
        }

        private void LoadEquipmentData()
        {
            if (App.equipment != null)
            {
                currentEquipment = App.equipment;

                // Заполняем поля данными
                txtName.Text = currentEquipment.Name;
                txtCategory.Text = currentEquipment.Categories?.CategoryName ?? "Не указано";
                txtStatus.Text = currentEquipment.Statuses?.StatusName ?? "Не указано";
                txtDescription.Text = currentEquipment.Description ?? "Не указано";
                txtSerialNumber.Text = currentEquipment.SerialNumber ?? "Не указано";
                txtEquipmentID.Text = currentEquipment.EquipmentID.ToString();
                txtDailyCost.Text = currentEquipment.DailyRentalCost.ToString("C");
                txtHourlyCost.Text = currentEquipment.HourlyRentalCost?.ToString("C") ?? "Не указано";
                txtDeposit.Text = currentEquipment.DepositAmount.ToString("C");
                txtNotes.Text = currentEquipment.Notes ?? "Не указано";
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно редактирования
            WindowEdit editWindow = new WindowEdit();
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные после редактирования
                LoadEquipmentData();

                // Уведомляем главное окно об обновлении
                RefreshMainWindowData();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentEquipment != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить оборудование \"{currentEquipment.Name}\"?\n\nЭто действие нельзя отменить!",
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
                            var equipmentToDelete = context.Equipment.Find(currentEquipment.EquipmentID);

                            if (equipmentToDelete != null)
                            {
                                // Проверяем, нет ли активных прокатов для этого оборудования
                                bool hasActiveRentals = context.Rentals.Any(r =>
                                    r.EquipmentID == currentEquipment.EquipmentID &&
                                    r.RentalDate <= DateTime.Now &&
                                    (!context.Returns.Any(rt => rt.RentalID == r.RentalID)));

                                if (hasActiveRentals)
                                {
                                    MessageBox.Show(
                                        "Невозможно удалить оборудование, так как у него есть активные прокаты.\nСначала оформите возврат оборудования.",
                                        "Ошибка удаления",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                // Удаляем оборудование
                                context.Equipment.Remove(equipmentToDelete);
                                context.SaveChanges();

                                MessageBox.Show("Оборудование успешно удалено!", "Успех",
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
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshMainWindowData()
        {
            // Находим главное окно и обновляем его данные
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.update();
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
    }
}