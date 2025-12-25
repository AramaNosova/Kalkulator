using System;
using System.Linq;
using System.Windows;

namespace SportsRentalSystem
{
    public partial class WindowViewClient : Window
    {
        public WindowViewClient()
        {
            InitializeComponent();
            LoadClientData();
        }

        private void LoadClientData()
        {
            if (App.selectedClient != null)
            {
                var client = App.selectedClient;

                txtLastName.Text = client.LastName;
                txtFirstName.Text = client.FirstName;
                txtMiddleName.Text = client.MiddleName ?? "Не указано";
                txtPhone.Text = client.Phone;
                txtEmail.Text = client.Email ?? "Не указано";
                txtRegistrationDate.Text = client.RegistrationDate.ToString("dd.MM.yyyy HH:mm");
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

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно редактирования
            WindowEditClient editWindow = new WindowEditClient();
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные после редактирования
                LoadClientData();

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
                mainWindow.updateClients();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (App.selectedClient != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить клиента \"{App.selectedClient.LastName} {App.selectedClient.FirstName}\"?\n\nЭто действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new SportsRentalSystemEntities())
                        {
                            // Находим клиента в базе данных
                            var clientToDelete = context.Clients.Find(App.selectedClient.ClientID);

                            if (clientToDelete != null)
                            {
                                // Проверяем, нет ли активных прокатов у клиента
                                bool hasActiveRentals = context.Rentals.Any(r =>
                                    r.ClientID == App.selectedClient.ClientID &&
                                    r.RentalDate <= DateTime.Now &&
                                    (!context.Returns.Any(rt => rt.RentalID == r.RentalID)));

                                if (hasActiveRentals)
                                {
                                    MessageBox.Show(
                                        "Невозможно удалить клиента, так как у него есть активные прокаты.\nСначала оформите возврат оборудования.",
                                        "Ошибка удаления",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                // Удаляем клиента
                                context.Clients.Remove(clientToDelete);
                                context.SaveChanges();

                                MessageBox.Show("Клиент успешно удален!", "Успех",
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
    }
}