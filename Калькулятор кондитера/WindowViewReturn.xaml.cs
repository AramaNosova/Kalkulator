using System;
using System.Linq;
using System.Windows;

namespace SportsRentalSystem
{
    public partial class WindowViewReturn : Window
    {
        private Returns _return;
        private Rentals _rental;

        public WindowViewReturn(int returnId)
        {
            InitializeComponent();
            LoadReturnData(returnId);
        }

        private void LoadReturnData(int returnId)
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    // Загружаем возврат с связанными данными
                    _return = context.Returns
                        .Include("Rentals")
                        .Include("Rentals.Clients")
                        .Include("Rentals.Equipment")
                        .Include("Rentals.Employees")
                        .FirstOrDefault(r => r.ReturnID == returnId);

                    if (_return == null)
                    {
                        MessageBox.Show("Возврат не найден");
                        this.Close();
                        return;
                    }

                    _rental = _return.Rentals;

                    // Заполняем информацию о возврате
                    txtReturnID.Text = _return.ReturnID.ToString();
                    txtRentalID.Text = _return.RentalID.ToString();
                    txtActualReturnDate.Text = _return.ActualReturnDate.ToString("dd.MM.yyyy HH:mm");
                    txtReturnCondition.Text = GetRussianReturnCondition(_return.ReturnCondition);
                    txtDamageDescription.Text = _return.DamageDescription ?? "Отсутствует";
                    txtManagerComment.Text = _return.ManagerComment ?? "Отсутствует";

                    // Заполняем информацию о прокате
                    txtClient.Text = $"{_rental.Clients.LastName} {_rental.Clients.FirstName} {_rental.Clients.MiddleName}".Trim();
                    txtEquipment.Text = _rental.Equipment.Name;
                    txtRentalDate.Text = _rental.RentalDate.ToString("dd.MM.yyyy HH:mm");
                    txtPlannedReturnDate.Text = _rental.PlannedReturnDate.ToString("dd.MM.yyyy");
                    txtRentalType.Text = _rental.RentalType == "hour" ? "час" : "день";
                    txtRentalPeriod.Text = _rental.RentalPeriod.ToString();
                    txtTotalCost.Text = $"{_rental.TotalCost:N2} ₽";
                    txtPaymentStatus.Text = GetRussianPaymentStatus(_rental.PaymentStatus);
                    txtRentalNotes.Text = _rental.Notes ?? "Отсутствуют";
                    txtEmployee.Text = $"{_rental.Employees.LastName} {_rental.Employees.FirstName}".Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                this.Close();
            }
        }

        private string GetRussianReturnCondition(string englishCondition)
        {
            switch (englishCondition)
            {
                case "Excellent":
                    return "Отличное";
                case "Good":
                    return "Хорошее";
                case "Damaged":
                    return "Повреждено";
                case "Lost":
                    return "Утеряно";
                default:
                    return englishCondition;
            }
        }

        private string GetRussianPaymentStatus(string englishStatus)
        {
            switch (englishStatus)
            {
                case "not paid":
                    return "не оплачено";
                case "paid":
                    return "оплачено";
                case "partially":
                    return "частично";
                default:
                    return englishStatus;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}