using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SportsRentalSystem
{
    public partial class WindowProcessReturn : Window
    {
        private SportsRentalSystemEntities context;
        private Rentals selectedRental;

        public WindowProcessReturn()
        {
            InitializeComponent();
            context = new SportsRentalSystemEntities();
            LoadActiveRentals();
        }

        private void LoadActiveRentals()
        {
            try
            {
                var activeRentals = context.Rentals
                    .Where(r => !r.Returns.Any()) // Правильная проверка для коллекции
                    .ToList();
                lvActiveRentals.ItemsSource = activeRentals;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки активных прокатов: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var searchText = txtSearch.Text.Trim().ToLower();
                var allRentals = context.Rentals.Where(r => r.Returns == null).ToList();

                if (!string.IsNullOrEmpty(searchText))
                {
                    var filteredRentals = allRentals.Where(r =>
                        r.Clients.LastName.ToLower().Contains(searchText) ||
                        r.Clients.FirstName.ToLower().Contains(searchText) ||
                        r.Equipment.Name.ToLower().Contains(searchText) ||
                        r.Equipment.SerialNumber.ToLower().Contains(searchText)
                    ).ToList();
                    lvActiveRentals.ItemsSource = filteredRentals;
                }
                else
                {
                    lvActiveRentals.ItemsSource = allRentals;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        private void lvActiveRentals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRental = lvActiveRentals.SelectedItem as Rentals;
            if (selectedRental != null)
            {
                ShowRentalInfo();
                btnProcessReturn.IsEnabled = true;
            }
            else
            {
                HideRentalInfo();
                btnProcessReturn.IsEnabled = false;
            }
        }

        private void ShowRentalInfo()
        {
            grpRentalInfo.Visibility = Visibility.Visible;
            grpDepositCalc.Visibility = Visibility.Visible;

            txtClientInfo.Text = $"{selectedRental.Clients.LastName} {selectedRental.Clients.FirstName} {selectedRental.Clients.MiddleName}";
            txtEquipmentInfo.Text = $"{selectedRental.Equipment.Name} (SN: {selectedRental.Equipment.SerialNumber})";
            txtCostInfo.Text = $"Прокат: {selectedRental.TotalCost:N0}₽, Залог: {selectedRental.Equipment.DepositAmount:N0}₽";

            txtDepositAmount.Text = $"{selectedRental.Equipment.DepositAmount:N0} ₽";
            CalculateDepositReturn();
        }

        private void HideRentalInfo()
        {
            grpRentalInfo.Visibility = Visibility.Collapsed;
            grpDepositCalc.Visibility = Visibility.Collapsed;
        }

        private void cmbReturnCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var condition = cmbReturnCondition.SelectedItem as ComboBoxItem;
            if (condition != null)
            {
                bool showDamageField = condition.Content.ToString() == "Damaged";
                txtDamageLabel.Visibility = showDamageField ? Visibility.Visible : Visibility.Collapsed;
                txtDamageDescription.Visibility = showDamageField ? Visibility.Visible : Visibility.Collapsed;

                CalculateDepositReturn();
            }
        }

        private void CalculateDepositReturn()
        {
            if (selectedRental == null || cmbReturnCondition.SelectedItem == null) return;

            var condition = (cmbReturnCondition.SelectedItem as ComboBoxItem)?.Content.ToString();
            decimal deposit = selectedRental.Equipment.DepositAmount;
            decimal withheld = 0;
            decimal returnAmount = deposit;

            switch (condition)
            {
                case "Damaged":
                    withheld = deposit * 0.5m; // Удерживаем 50% за повреждения
                    returnAmount = deposit - withheld;
                    break;
                case "Lost":
                    withheld = deposit; // Удерживаем весь залог
                    returnAmount = 0;
                    break;
                case "Good":
                    withheld = 0; // Возвращаем весь залог
                    returnAmount = deposit;
                    break;
                case "Excellent":
                    withheld = 0; // Возвращаем весь залог
                    returnAmount = deposit;
                    break;
            }

            txtWithheldAmount.Text = $"{withheld:N0} ₽";
            txtReturnAmount.Text = $"{returnAmount:N0} ₽";
        }

        private void btnProcessReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateReturnData()) return;

                // Создание записи о возврате
                var returnRecord = new Returns
                {
                    RentalID = selectedRental.RentalID,
                    ActualReturnDate = DateTime.Now,
                    ReturnCondition = (cmbReturnCondition.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    DamageDescription = txtDamageDescription.Visibility == Visibility.Visible ? txtDamageDescription.Text : null,
                    ManagerComment = txtManagerComment.Text
                };

                // Обновление статуса оборудования
                var equipment = context.Equipment.Find(selectedRental.EquipmentID);
                var availableStatus = context.Statuses.FirstOrDefault(s => s.StatusName == "Свободен" || s.StatusName == "Available");
                if (availableStatus != null)
                {
                    equipment.StatusID = availableStatus.StatusID;
                }

                // Сохранение в БД
                context.Returns.Add(returnRecord);
                context.SaveChanges();

                MessageBox.Show("Возврат успешно оформлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении возврата: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateReturnData()
        {
            if (selectedRental == null)
            {
                MessageBox.Show("Выберите прокат для возврата", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbReturnCondition.SelectedItem == null)
            {
                MessageBox.Show("Выберите состояние оборудования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var condition = (cmbReturnCondition.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (condition == "Damaged" && string.IsNullOrWhiteSpace(txtDamageDescription.Text))
            {
                MessageBox.Show("Заполните описание повреждений", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
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
    }
}