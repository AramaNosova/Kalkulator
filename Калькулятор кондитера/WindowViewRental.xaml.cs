using System;
using System.Linq;
using System.Windows;

namespace SportsRentalSystem
{
    public partial class WindowViewRental : Window
    {
        private SportsRentalSystemEntities context;

        public WindowViewRental()
        {
            InitializeComponent();
            context = new SportsRentalSystemEntities();
            LoadRentalData();
        }

        private void LoadRentalData()
        {
            try
            {
                var rental = context.Rentals.Find(App.selectedRental?.RentalID);
                if (rental == null) return;

                // Получаем информацию о возврате (если есть)
                var returnInfo = rental.Returns.FirstOrDefault();

                // Основная информация
                txtRentalID.Text = rental.RentalID.ToString();
                txtStatus.Text = returnInfo == null ? "Активен" : "Завершен";
                txtRentalDate.Text = rental.RentalDate.ToString("dd.MM.yyyy HH:mm");
                txtPlannedReturn.Text = rental.PlannedReturnDate.ToString("dd.MM.yyyy");
                txtPaymentStatus.Text = rental.PaymentStatus;

                // Информация о клиенте
                txtClientName.Text = $"{rental.Clients.LastName} {rental.Clients.FirstName} {rental.Clients.MiddleName}";
                txtClientPhone.Text = rental.Clients.Phone;
                txtClientEmail.Text = rental.Clients.Email ?? "Не указан";

                // Информация об оборудовании
                txtEquipmentName.Text = rental.Equipment.Name;
                txtEquipmentCategory.Text = rental.Equipment.Categories?.CategoryName ?? "Не указана";
                txtEquipmentSerial.Text = rental.Equipment.SerialNumber ?? "Не указан";
                txtEquipmentStatus.Text = rental.Equipment.Statuses?.StatusName ?? "Не указан";

                // Финансовая информация
                txtRentalType.Text = rental.RentalType;
                txtRentalPeriod.Text = $"{rental.RentalPeriod} {(rental.RentalType == "час" ? "час." : "дн.")}";

                // Расчет стоимости проката (общая стоимость минус залог)
                decimal rentalCost = rental.TotalCost - rental.Equipment.DepositAmount;
                txtRentalCost.Text = $"{rentalCost:N0} ₽";
                txtDeposit.Text = $"{rental.Equipment.DepositAmount:N0} ₽";
                txtTotalCost.Text = $"{rental.TotalCost:N0} ₽";

                // Информация о возврате
                if (returnInfo != null)
                {
                    grpReturnInfo.Visibility = Visibility.Visible;
                    txtActualReturn.Text = returnInfo.ActualReturnDate.ToString("dd.MM.yyyy HH:mm");
                    txtReturnCondition.Text = GetConditionDescription(returnInfo.ReturnCondition);
                    txtDamageDescription.Text = returnInfo.DamageDescription ?? "Отсутствуют";
                    txtManagerComment.Text = returnInfo.ManagerComment ?? "Отсутствует";
                }
                else
                {
                    grpReturnInfo.Visibility = Visibility.Collapsed;
                }

                // Примечания
                txtNotes.Text = rental.Notes ?? "Отсутствуют";
                if (string.IsNullOrWhiteSpace(rental.Notes))
                {
                    grpNotes.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных проката: {ex.Message}");
            }
        }

        private string GetConditionDescription(string condition)
        {
            switch (condition)
            {
                case "Excellent": return "Отличное";
                case "Good": return "Хорошее";
                case "Damaged": return "Поврежденное";
                case "Lost": return "Утеряно";
                default: return condition;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}