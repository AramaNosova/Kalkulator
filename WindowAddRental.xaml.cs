using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SportsRentalSystem
{
    public partial class WindowAddRental : Window
    {
        public WindowAddRental()
        {
            InitializeComponent();
            LoadClients();
            LoadEquipment();
            InitializeDefaults();
        }

        private void LoadClients()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    var clients = context.Clients.ToList();

                    // Создаем список с полным ФИО для отображения
                    var clientsWithFullName = clients.Select(c => new
                    {
                        Client = c,
                        FullName = GetFullName(c.LastName, c.FirstName, c.MiddleName),
                        DisplayText = $"{c.LastName} {c.FirstName} {c.MiddleName}".Trim()
                    }).ToList();

                    cmbClient.ItemsSource = clientsWithFullName;
                    cmbClient.DisplayMemberPath = "DisplayText"; // Отображаем полное ФИО
                    cmbClient.SelectedValuePath = "Client"; // Сохраняем весь объект клиента
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
            }
        }

        // Метод для формирования полного ФИО
        private string GetFullName(string lastName, string firstName, string middleName)
        {
            return $"{lastName} {firstName} {middleName}".Trim();
        }

        private void LoadEquipment()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    // Загружаем только доступное оборудование (StatusID = 1 - Свободен)
                    var availableEquipment = context.Equipment
                        .Where(e => e.StatusID == 1)
                        .ToList();

                    cmbEquipment.ItemsSource = availableEquipment;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки оборудования: " + ex.Message);
            }
        }

        private void InitializeDefaults()
        {
            cmbRentalType.SelectedIndex = 0;
            cmbPaymentStatus.SelectedIndex = 0;
            dpPlannedReturnDate.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (cmbClient.SelectedItem == null)
                {
                    MessageBox.Show("Выберите клиента");
                    return;
                }

                if (cmbEquipment.SelectedItem == null)
                {
                    MessageBox.Show("Выберите оборудование");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtRentalPeriod.Text) || !int.TryParse(txtRentalPeriod.Text, out int period) || period <= 0)
                {
                    MessageBox.Show("Введите корректный период проката");
                    return;
                }

                if (dpPlannedReturnDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату возврата");
                    return;
                }

                // Получаем клиента из выбранного элемента
                Clients selectedClient;
                var selectedClientItem = cmbClient.SelectedItem;

                // Проверяем тип выбранного элемента
                if (selectedClientItem.GetType().Name.Contains("Anonymous")) // Это анонимный тип с оберткой
                {
                    // Используем рефлексию для получения свойства Client
                    var clientProperty = selectedClientItem.GetType().GetProperty("Client");
                    selectedClient = (Clients)clientProperty.GetValue(selectedClientItem);
                }
                else
                {
                    // Если используется напрямую объект Clients
                    selectedClient = selectedClientItem as Clients;
                }

                if (selectedClient == null)
                {
                    MessageBox.Show("Ошибка при получении данных клиента");
                    return;
                }

                // Сохранение в базу данных
                using (var context = new SportsRentalSystemEntities())
                {
                    // Получаем выбранное оборудование
                    var selectedEquipment = cmbEquipment.SelectedItem as Equipment;

                    // Проверяем, что оборудование все еще доступно
                    var equipmentInDb = context.Equipment.Find(selectedEquipment.EquipmentID);
                    if (equipmentInDb == null || equipmentInDb.StatusID != 1)
                    {
                        MessageBox.Show("Выбранное оборудование больше не доступно для проката");
                        LoadEquipment(); // Обновляем список оборудования
                        return;
                    }

                    // Преобразуем статус оплаты в английские значения
                    string paymentStatus = "not paid"; // значение по умолчанию
                    if (cmbPaymentStatus.SelectedItem is ComboBoxItem paymentItem)
                    {
                        string statusText = paymentItem.Content.ToString();
                        if (statusText == "не оплачено")
                            paymentStatus = "not paid";
                        else if (statusText == "оплачено")
                            paymentStatus = "paid";
                        else if (statusText == "частично")
                            paymentStatus = "partially";
                    }

                    // Создаем запись о прокате
                    var rental = new Rentals
                    {
                        ClientID = selectedClient.ClientID, // Используем полученного клиента
                        EquipmentID = selectedEquipment.EquipmentID,
                        EmployeeID = App.employees.EmployeeID, // Добавляем ID текущего сотрудника
                        RentalDate = DateTime.Now,
                        PlannedReturnDate = dpPlannedReturnDate.SelectedDate.Value,
                        RentalType = ((ComboBoxItem)cmbRentalType.SelectedItem).Content.ToString() == "час" ? "hour" : "day",
                        RentalPeriod = int.Parse(txtRentalPeriod.Text),
                        TotalCost = decimal.Parse(txtTotalCost.Text.Replace(" ₽", "").Replace(" ", "")),
                        PaymentStatus = paymentStatus,
                        Notes = txtNotes.Text
                    };

                    // Меняем статус оборудования на "В прокате" (StatusID = 2)
                    equipmentInDb.StatusID = 2;

                    // Сохраняем изменения
                    context.Rentals.Add(rental);
                    context.SaveChanges();

                    MessageBox.Show("Прокат успешно оформлен!");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
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

        // Обработчики событий для расчета стоимости
        private void cmbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика при выборе клиента (если нужна)
        }

        private void cmbEquipment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEquipment.SelectedItem is Equipment selectedEquipment)
            {
                borderEquipmentInfo.Visibility = Visibility.Visible;
                txtEquipmentInfo.Text = $"{selectedEquipment.Name}\n{selectedEquipment.Description}";
                txtEquipmentPrice.Text = $"Стоимость: {selectedEquipment.DailyRentalCost:N2} ₽/день";

                if (selectedEquipment.HourlyRentalCost.HasValue)
                {
                    txtEquipmentPrice.Text += $", {selectedEquipment.HourlyRentalCost.Value:N2} ₽/час";
                }
            }
            else
            {
                borderEquipmentInfo.Visibility = Visibility.Collapsed;
            }

            CalculateCost();
        }

        private void cmbRentalType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateCost();
        }

        private void txtRentalPeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCost();
        }

        private void dpPlannedReturnDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateCost();
        }

        private void CalculateCost()
        {
            try
            {
                if (cmbEquipment.SelectedItem is Equipment selectedEquipment &&
                    !string.IsNullOrWhiteSpace(txtRentalPeriod.Text))
                {
                    int period = int.Parse(txtRentalPeriod.Text);
                    decimal rentalCost = 0;
                    decimal deposit = selectedEquipment.DepositAmount;

                    if (cmbRentalType.SelectedItem is ComboBoxItem rentalTypeItem)
                    {
                        string rentalType = rentalTypeItem.Content.ToString();

                        if (rentalType == "час" && selectedEquipment.HourlyRentalCost.HasValue)
                        {
                            rentalCost = selectedEquipment.HourlyRentalCost.Value * period;
                        }
                        else if (rentalType == "день")
                        {
                            rentalCost = selectedEquipment.DailyRentalCost * period;
                        }
                    }

                    txtRentalCost.Text = $"{rentalCost:N2} ₽";
                    txtDepositCost.Text = $"{deposit:N2} ₽";
                    txtTotalCost.Text = $"{rentalCost + deposit:N2} ₽";
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки расчета
            }
        }

        // Валидация ввода
        private void txtRentalPeriod_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void txtRentalPeriod_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!text.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Перетаскивание окна
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}