using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity; 

namespace SportsRentalSystem
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            output();
            this.Title = "🏄 Система учета проката спортивного оборудования - Версия 1.0";
            LoadCategoryFilter();
            LoadStatusFilter();
            
            update();
            updateEmployees();
            LoadPositionFilter(); // Добавляем загрузку должностей
            updateClients();
            updateRentals();
            updateReturns();
            InitializeReports();
        }

        private void InitializeReports()
        {
            // Устанавливаем начальную дату на 1 месяц назад
            dpReportStartDate.SelectedDate = DateTime.Today.AddMonths(-1);
            dpReportEndDate.SelectedDate = DateTime.Today;

            // Загружаем отчёты
            LoadReports();
        }

        public class TopClientInfo
        {
            public string ClientName { get; set; }
            public int RentalCount { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class TopEquipmentInfo
        {
            public string EquipmentName { get; set; }
            public string CategoryName { get; set; }
            public int RentalCount { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        private void LoadReports()
        {
            try
            {
                // Проверяем что контролы загружены
                if (txtRevenue == null || dpReportStartDate == null || dpReportEndDate == null)
                {
                    Console.WriteLine("Контролы не загружены, пропускаем загрузку отчётов");
                    return;
                }

                // Получаем даты с проверкой на null
                DateTime startDate = dpReportStartDate.SelectedDate?.Date ?? DateTime.Today.AddMonths(-1);
                DateTime endDate = dpReportEndDate.SelectedDate?.Date ?? DateTime.Today;

                // Корректируем время
                startDate = startDate.Date;
                endDate = endDate.Date.AddDays(1).AddSeconds(-1);

                using (var context = new SportsRentalSystemEntities())
                {
                    // 1. Получаем все прокаты за период
                    var rentals = context.Rentals
                        .Where(r => r.RentalDate >= startDate && r.RentalDate <= endDate)
                        .ToList();

                    // 2. Основные показатели
                    decimal totalRevenue = rentals.Sum(r => r.TotalCost);
                    int rentalCount = rentals.Count;
                    decimal avgCheck = rentalCount > 0 ? totalRevenue / rentalCount : 0;

                    txtRevenue.Text = $"{totalRevenue:N2} ₽";
                    txtRentalCount.Text = rentalCount.ToString();
                    txtAvgCheck.Text = $"{avgCheck:N2} ₽";

                    // 3. Новые клиенты
                    int newClients = context.Clients
                        .Count(c => c.RegistrationDate >= startDate && c.RegistrationDate <= endDate);
                    txtNewClients.Text = newClients.ToString();

                    // 4. Возвраты
                    var returns = context.Returns
                        .Where(r => r.ActualReturnDate >= startDate && r.ActualReturnDate <= endDate)
                        .ToList();
                    txtReturnsCount.Text = returns.Count.ToString();

                    // 5. Топ клиентов (простая версия)
                    var clientGroups = rentals
                        .GroupBy(r => r.ClientID)
                        .Select(g => new TopClientInfo
                        {
                            ClientName = GetClientName(g.Key, context),
                            RentalCount = g.Count(),
                            TotalAmount = g.Sum(r => r.TotalCost)
                        })
                        .OrderByDescending(c => c.TotalAmount)
                        .Take(5)
                        .ToList();

                    lvTopClients.ItemsSource = clientGroups;

                    // 6. Топ оборудования (простая версия)
                    var equipmentGroups = rentals
                        .GroupBy(r => r.EquipmentID)
                        .Select(g => new TopEquipmentInfo
                        {
                            EquipmentName = GetEquipmentName(g.Key, context),
                            CategoryName = GetCategoryName(g.Key, context),
                            RentalCount = g.Count(),
                            TotalRevenue = g.Sum(r => r.TotalCost)
                        })
                        .OrderByDescending(e => e.RentalCount)
                        .Take(5)
                        .ToList();

                    lvTopEquipment.ItemsSource = equipmentGroups;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки отчётов: {ex.Message}");

                // Устанавливаем безопасные значения
                if (txtRevenue != null) txtRevenue.Text = "0 ₽";
                if (txtRentalCount != null) txtRentalCount.Text = "0";
                if (txtNewClients != null) txtNewClients.Text = "0";
                if (txtReturnsCount != null) txtReturnsCount.Text = "0";
                if (txtAvgCheck != null) txtAvgCheck.Text = "0 ₽";
                if (lvTopClients != null) lvTopClients.ItemsSource = new List<TopClientInfo>();
                if (lvTopEquipment != null) lvTopEquipment.ItemsSource = new List<TopEquipmentInfo>();
            }
        }

        // Вспомогательные методы
        private string GetClientName(int clientId, SportsRentalSystemEntities context)
        {
            var client = context.Clients.FirstOrDefault(c => c.ClientID == clientId);
            return client != null ? $"{client.LastName} {client.FirstName}" : "Неизвестный";
        }

        private string GetEquipmentName(int equipmentId, SportsRentalSystemEntities context)
        {
            var equipment = context.Equipment.FirstOrDefault(e => e.EquipmentID == equipmentId);
            return equipment?.Name ?? "Неизвестное";
        }

        private string GetCategoryName(int equipmentId, SportsRentalSystemEntities context)
        {
            var equipment = context.Equipment
                .Include(e => e.Categories)
                .FirstOrDefault(e => e.EquipmentID == equipmentId);

            return equipment?.Categories?.CategoryName ?? "Без категории";
        }

        private void SetDefaultValues()
        {
            txtRevenue.Text = "0 ₽";
            txtRentalCount.Text = "0";
            txtNewClients.Text = "0";
            txtReturnsCount.Text = "0";
            txtAvgCheck.Text = "0 ₽";
            lvTopClients.ItemsSource = new List<TopClientInfo>();
            lvTopEquipment.ItemsSource = new List<TopEquipmentInfo>();
        }

        private void dpReportDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Просто вызываем загрузку отчётов
                LoadReports();
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки при загрузке
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void RefreshReportsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadReports();
        }

        public void update()
        {
            App.sports = new SportsRentalSystemEntities();
            IQueryable<Equipment> query = App.sports.Equipment;

            string searchText = poisk.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(p => p.Name.Contains(searchText));
            }

            var selectedCategory = cmbCategoryFilter.SelectedItem as Categories;
            if (selectedCategory != null && selectedCategory.CategoryID != 0)
            {
                query = query.Where(p => p.CategoryID == selectedCategory.CategoryID);
            }

            var selectedStatus = cmbStatusFilter.SelectedItem as Statuses;
            if (selectedStatus != null && selectedStatus.StatusID != 0)
            {
                query = query.Where(p => p.StatusID == selectedStatus.StatusID);
            }

            var List = query.ToList();
            int vsegoTxt = List.Count;
            int dostupnoTxt = List.Count(p => p.Statuses.StatusName == "Свободен" || p.Statuses.StatusName == "Available");
            int nedostupnoTxt = List.Count(p => p.Statuses.StatusName == "В прокате" || p.Statuses.StatusName == "Rented");

            ListVi.ItemsSource = List;

            Vsego.Text = "📊 Всего единиц: " + vsegoTxt.ToString();
            Dostupno.Text = " Доступно: " + dostupnoTxt.ToString();
            NeDostupno.Text = " В прокате: " + nedostupnoTxt.ToString();
        }


        // Обработчик изменения фильтра категории
        private void cmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            update();
        }

        // Обработчик изменения фильтра статуса
        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            update();
        }

        private void LoadCategoryFilter()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    // Создаем список категорий + вариант "Все категории"
                    var categories = context.Categories.ToList();
                    var allCategories = new List<Categories>
                    {
                        new Categories { CategoryID = 0, CategoryName = "Все категории" }
                    };
                    allCategories.AddRange(categories);

                    cmbCategoryFilter.ItemsSource = allCategories;
                    cmbCategoryFilter.SelectedIndex = 0; // Выбираем "Все категории" по умолчанию
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }



        // Загрузка статусов для фильтра
        private void LoadStatusFilter()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    // Создаем список статусов + вариант "Все статусы"
                    var statuses = context.Statuses.ToList();
                    var allStatuses = new List<Statuses>
                    {
                        new Statuses { StatusID = 0, StatusName = "Все статусы" }
                    };
                    allStatuses.AddRange(statuses);

                    cmbStatusFilter.ItemsSource = allStatuses;
                    cmbStatusFilter.SelectedIndex = 0; // Выбираем "Все статусы" по умолчанию
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message);
            }
        }

        private void up(object sender, TextChangedEventArgs e)
        {
            update();
        }

        public void output()
        {
            nameSotrudnik.Text = "Добро пожаловать, " + App.employees.FirstName + " " + App.employees.MiddleName + "!";
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwind, int wMsg, int wParam, int lParam);

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                WindowInteropHelper helper = new WindowInteropHelper(this);
                SendMessage(helper.Handle, 161, 2, 0);
                //this.DragMove();
            }
        }

        private void Maximize(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                ToolTip.Text = "Развернуть";
                this.WindowState = WindowState.Normal;
            }
            else
            {
                ToolTip.Text = "Восстановить";
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Minimize(object sender, RoutedEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 addWindow = new Window1();
            if (addWindow.ShowDialog() == true)
            {
                // Обновить список оборудования
                update();
            }
        }

        // Просмотр оборудования по двойному клику
        private void ListVi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewSelectedEquipment();
        }

        // Удаление оборудования по кнопке
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ListVi.SelectedItem as Equipment;
            if (selectedItem != null)
            {
                App.equipment= selectedItem;

                // Открываем окно просмотра, где есть кнопка удаления
                WindowView viewWindow = new WindowView();
                if (viewWindow.ShowDialog() == true)
                {
                    update(); // Обновляем список после удаления
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для удаления");
            }
        }

        // Просмотр оборудования
        private void ViewSelectedEquipment()
        {
            var selectedItem = ListVi.SelectedItem as Equipment;
            if (selectedItem != null)
            {
                // Сохраняем выбранное оборудование в глобальной переменной
                App.equipment = selectedItem;

                WindowView viewWindow = new WindowView();
                if (viewWindow.ShowDialog() == true)
                {
                    update(); // Обновляем список после удаления
                }
                else
                {
                    update(); // Обновляем список после возможного редактирования
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для просмотра");
            }
        }

        // Редактирование оборудования по кнопке
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedEquipment();
        }

        private void EditSelectedEquipment()
        {
            var selectedItem = ListVi.SelectedItem as Equipment;
            if (selectedItem != null)
            {
                // Сохраняем выбранное оборудование в глобальной переменной
                App.equipment = selectedItem;

                WindowEdit editWindow = new WindowEdit();
                if (editWindow.ShowDialog() == true)
                {
                    update(); // Обновляем список после редактирования
                }
            }
            else
            {
                MessageBox.Show("Выберите оборудование для редактирования");
            }
            update();
        }

        // Добавление нового сотрудника
        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAddEmployee addEmployeeWindow = new WindowAddEmployee();
            if (addEmployeeWindow.ShowDialog() == true)
            {
                // Можно добавить обновление списка сотрудников, если он будет
                MessageBox.Show("Сотрудник успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //СОТРУДНИКИ
        public void updateEmployees()
        {
            App.sports = new SportsRentalSystemEntities();
            var List = App.sports.Employees.ToList();
            int vsegoTxt = 0;
            int adminTxt = 0;
            int managerTxt = 0;

            // Получаем выбранную должность из фильтра
            string selectedPosition = cmbPositionFilter.SelectedItem as string;
            string searchText = poiskEmployee?.Text?.Trim() ?? "";

            // Применяем фильтры
            if (!string.IsNullOrEmpty(searchText) && selectedPosition != "Все должности")
            {
                // Фильтр по ФИО И по должности
                List = App.sports.Employees
                    .Where(p => (p.LastName.Contains(searchText) ||
                                p.FirstName.Contains(searchText) ||
                                (p.MiddleName != null && p.MiddleName.Contains(searchText))) &&
                                p.Position == selectedPosition)
                    .ToList();
            }
            else if (!string.IsNullOrEmpty(searchText))
            {
                // Только фильтр по ФИО
                List = App.sports.Employees
                    .Where(p => p.LastName.Contains(searchText) ||
                               p.FirstName.Contains(searchText) ||
                               (p.MiddleName != null && p.MiddleName.Contains(searchText)))
                    .ToList();
            }
            else if (selectedPosition != "Все должности")
            {
                // Только фильтр по должности
                List = App.sports.Employees
                    .Where(p => p.Position == selectedPosition)
                    .ToList();
            }
            else
            {
                // Без фильтров - все записи
                List = App.sports.Employees.ToList();
            }

            // Подсчет статистики
            vsegoTxt = List.Count;
            adminTxt = List.Count(p => p.Position == "Администратор");
            managerTxt = List.Count(p => p.Position == "Менеджер");

            ListViEmployees.ItemsSource = List;

            VsegoEmployees.Text = "📊 Всего сотрудников: " + vsegoTxt.ToString();
            AdminCount.Text = " Администраторов: " + adminTxt.ToString();
            ManagerCount.Text = " Менеджеров: " + managerTxt.ToString();


        }

        // Обработчик изменения фильтра должности
        private void cmbPositionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateEmployees();
        }

        // Обработчик поиска сотрудников
        private void upEmployee(object sender, TextChangedEventArgs e)
        {
            updateEmployees();
        }

        // Просмотр сотрудника по двойному клику
        private void ListViEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewSelectedEmployee();
        }

        // Просмотр сотрудника
        private void ViewSelectedEmployee()
        {
            var selectedItem = ListViEmployees.SelectedItem as Employees;
            if (selectedItem != null)
            {
                App.selectedEmployee = selectedItem;
                WindowViewEmployee viewWindow = new WindowViewEmployee();
                if (viewWindow.ShowDialog() == true)
                {
                    updateEmployees();
                }
                else
                {
                    updateEmployees();
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для просмотра");
            }
        }

        // Загрузка должностей для фильтра
        private void LoadPositionFilter()
        {
            try
            {
                // Проверяем, что ComboBox инициализирован
                if (cmbPositionFilter == null)
                {
                    MessageBox.Show("ComboBox еще не инициализирован");
                    return;
                }

                using (var context = new SportsRentalSystemEntities())
                {
                    // Получаем уникальные должности из базы данных
                    var positions = context.Employees
                        .Select(e => e.Position)
                        .Distinct()
                        .Where(p => p != null) // Исключаем null значения
                        .OrderBy(p => p)
                        .ToList();

                    // Создаем список для ComboBox
                    var positionList = new List<string> { "Все должности" };
                    positionList.AddRange(positions);

                    cmbPositionFilter.ItemsSource = positionList;
                    cmbPositionFilter.SelectedIndex = 0; // Выбираем "Все должности" по умолчанию
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Ошибка загрузки должностей: " + ex.Message);
            }
        }
        //КЛИЕНТЫ
        public void updateClients()
        {
            try
            {
                App.sports = new SportsRentalSystemEntities();
                var List = App.sports.Clients.ToList();

                int vsegoTxt = 0;
                int activeTxt = 0;
                int newThisMonthTxt = 0;
                string searchText = poiskClient?.Text?.Trim() ?? "";

                if (!string.IsNullOrEmpty(searchText))
                {
                    List = App.sports.Clients
                        .Where(p => p.LastName.Contains(searchText) ||
                                   p.FirstName.Contains(searchText) ||
                                   (p.MiddleName != null && p.MiddleName.Contains(searchText)))
                        .ToList();
                }
                vsegoTxt = List.Count;
                activeTxt = List.Count;
                newThisMonthTxt = List.Count(p => p.RegistrationDate.Month == DateTime.Now.Month &&
                                                 p.RegistrationDate.Year == DateTime.Now.Year);
                ListViClients.ItemsSource = List;

                VsegoClients.Text = "📊 Всего клиентов: " + vsegoTxt.ToString();
                ActiveClients.Text = " Активных: " + activeTxt.ToString();
                NewClients.Text = " Новых за месяц: " + newThisMonthTxt.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в updateClients(): {ex.Message}");
            }
        }

        // Обработчик поиска клиентов
        private void upClient(object sender, TextChangedEventArgs e)
        {
            updateClients();
        }

        // Добавление нового клиента
        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAddClient addClientWindow = new WindowAddClient();
            if (addClientWindow.ShowDialog() == true)
            {
                updateClients();
            }
        }

        // Просмотр клиента по двойному клику
        private void ListViClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewSelectedClient();
        }
        // Просмотр клиента
        private void ViewSelectedClient()
        {
            var selectedItem = ListViClients.SelectedItem as Clients;
            if (selectedItem != null)
            {
                App.selectedClient = selectedItem;
                WindowViewClient viewWindow = new WindowViewClient();
                if (viewWindow.ShowDialog() == true)
                {
                    updateClients();
                }
                else
                {
                    updateClients();
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для просмотра");
            }
        }

        // Редактирование клиента по кнопке
        private void EditClientButton_Click(object sender, RoutedEventArgs e)
        {
            // Можно создать WindowEditClient аналогично сотрудникам
            MessageBox.Show("Функция редактирования клиента в разработке");
        }

        // Удаление клиента по кнопке
        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = ListViClients.SelectedItem as Clients;
            if (selectedItem != null)
            {
                App.selectedClient = selectedItem;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить клиента {selectedItem.LastName} {selectedItem.FirstName}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new SportsRentalSystemEntities())
                        {
                            var clientToDelete = context.Clients.Find(selectedItem.ClientID);
                            if (clientToDelete != null)
                            {
                                // Проверяем, нет ли активных прокатов у клиента
                                bool hasActiveRentals = context.Rentals.Any(r =>
                                    r.ClientID == selectedItem.ClientID &&
                                    r.RentalDate <= DateTime.Now &&
                                    (!context.Returns.Any(rt => rt.RentalID == r.RentalID)));

                                if (hasActiveRentals)
                                {
                                    MessageBox.Show(
                                        "Невозможно удалить клиента, так как у него есть активные прокаты.",
                                        "Ошибка удаления",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    return;
                                }

                                context.Clients.Remove(clientToDelete);
                                context.SaveChanges();

                                MessageBox.Show("Клиент успешно удален!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                updateClients();
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
            else
            {
                MessageBox.Show("Выберите клиента для удаления");
            }
        }

        // 
        // ========== МЕТОДЫ ДЛЯ ПРОКАТА ==========

        public void updateRentals()
        {
            try
            {
                App.sports = new SportsRentalSystemEntities();

                // Получаем все RentalID из таблицы Returns (уже возвращенные прокаты)
                var returnedRentalIds = App.sports.Returns.Select(r => r.RentalID).ToList();

                // Используем JOIN для получения связанных данных
                var activeRentals = (from rental in App.sports.Rentals
                                     join client in App.sports.Clients on rental.ClientID equals client.ClientID
                                     join equipment in App.sports.Equipment on rental.EquipmentID equals equipment.EquipmentID
                                     join employee in App.sports.Employees on rental.EmployeeID equals employee.EmployeeID
                                     where !returnedRentalIds.Contains(rental.RentalID)
                                     select new
                                     {
                                         rental.RentalID,
                                         rental.ClientID,
                                         rental.EquipmentID,
                                         rental.RentalDate,
                                         rental.PlannedReturnDate,
                                         RentalType = rental.RentalType == "hour" ? "час" :
                                                     rental.RentalType == "day" ? "день" : rental.RentalType,
                                         rental.RentalPeriod,
                                         rental.TotalCost,
                                         PaymentStatus = rental.PaymentStatus == "not paid" ? "не оплачено" :
                                                       rental.PaymentStatus == "paid" ? "оплачено" :
                                                       rental.PaymentStatus == "partially" ? "частично" : rental.PaymentStatus,
                                         rental.Notes,
                                         ClientName = client.LastName + " " + client.FirstName,
                                         EquipmentName = equipment.Name,
                                         EmployeeName = employee.LastName + " " + employee.FirstName
                                     }).ToList();

                // Получаем текст поиска
                string searchText = poiskRentals?.Text?.Trim() ?? "";

                // Применяем фильтр по поисковому запросу
                if (!string.IsNullOrEmpty(searchText))
                {
                    activeRentals = activeRentals
                        .Where(r => (r.ClientName?.Contains(searchText) == true) ||
                                   (r.EquipmentName?.Contains(searchText) == true) ||
                                   (r.EmployeeName?.Contains(searchText) == true) ||
                                   (r.PaymentStatus?.Contains(searchText) == true) ||
                                   (r.RentalType?.Contains(searchText) == true) ||
                                   (r.RentalID.ToString().Contains(searchText)) ||
                                   (r.TotalCost.ToString().Contains(searchText)))
                        .ToList();
                }

                int vsegoTxt = activeRentals.Count;
                int activeTxt = activeRentals.Count;
                int overdueTxt = activeRentals.Count(r => r.PlannedReturnDate < DateTime.Now);

                ListViRentals.ItemsSource = activeRentals;

                VsegoRentals.Text = "📊 Всего прокатов: " + vsegoTxt.ToString();
                ActiveRentals.Text = " Активных: " + activeTxt.ToString();
                OverdueRentals.Text = " Просроченных: " + overdueTxt.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в updateRentals(): {ex.Message}");
            }
        }

        // Обработчик поиска прокатов
        private void upRentals(object sender, TextChangedEventArgs e)
        {
            updateRentals();
        }

        // Обработчик кнопки обновления
        private void RefreshRentalsButton_Click(object sender, RoutedEventArgs e)
        {
            updateRentals();
        }

        private void ListViReturns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListViReturns.SelectedItem != null)
            {
                dynamic selectedItem = ListViReturns.SelectedItem;
                int returnId = selectedItem.ReturnID;

                var viewWindow = new WindowViewReturn(returnId);
                viewWindow.ShowDialog();
            }
        }

        // В раздел методов для проката добавьте:

        private void AddRentalButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAddRental addRentalWindow = new WindowAddRental();
            if (addRentalWindow.ShowDialog() == true)
            {
                updateRentals();
                update(); // Обновляем список оборудования
            }
        }

        private void ProcessReturnButton_Click(object sender, RoutedEventArgs e)
        {
            WindowProcessReturn returnWindow = new WindowProcessReturn();
            if (returnWindow.ShowDialog() == true)
            {
                updateRentals();
                update(); // Обновляем список оборудования
            }
        }

        private void ListViRentals_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewSelectedRental();
        }

        private void ViewSelectedRental()
        {
            var selectedItem = ListViRentals.SelectedItem as Rentals;
            if (selectedItem != null)
            {
                App.selectedRental = selectedItem;
                WindowViewRental viewWindow = new WindowViewRental();
                viewWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите прокат для просмотра");
            }
        }

        private void cmbRentalStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateRentals();
        }

        // В методе updateReturns() преобразуем английские статусы в русские для отображения
        public void updateReturns()
        {
            try
            {
                App.sports = new SportsRentalSystemEntities();

                var returnsList = (from ret in App.sports.Returns
                                   join rental in App.sports.Rentals on ret.RentalID equals rental.RentalID
                                   join equipment in App.sports.Equipment on rental.EquipmentID equals equipment.EquipmentID
                                   join client in App.sports.Clients on rental.ClientID equals client.ClientID
                                   select new
                                   {
                                       ReturnID = ret.ReturnID,
                                       RentalID = ret.RentalID,
                                       EquipmentName = equipment.Name,
                                       ClientName = client.LastName + " " + client.FirstName,
                                       ActualReturnDate = ret.ActualReturnDate,
                                       // Преобразуем для отображения
                                       ReturnCondition = ret.ReturnCondition == "Excellent" ? "Отличное" :
                                                       ret.ReturnCondition == "Good" ? "Хорошее" :
                                                       ret.ReturnCondition == "Damaged" ? "Повреждено" :
                                                       ret.ReturnCondition == "Lost" ? "Утеряно" : ret.ReturnCondition,
                                       DamageDescription = ret.DamageDescription,
                                       ManagerComment = ret.ManagerComment,
                                       PlannedReturnDate = rental.PlannedReturnDate,
                                       RentalDate = rental.RentalDate
                                   }).ToList();

                ListViReturns.ItemsSource = returnsList;

                // Обновляем статистику с русскими значениями
                int totalReturns = returnsList.Count;
                int goodCondition = returnsList.Count(r => r.ReturnCondition == "Хорошее" || r.ReturnCondition == "Отличное");
                int damagedCondition = returnsList.Count(r => r.ReturnCondition == "Повреждено");
                int lostCondition = returnsList.Count(r => r.ReturnCondition == "Утеряно");

                VsegoReturns.Text = "📊 Всего возвратов: " + totalReturns.ToString();
                ActiveReturns.Text = " В хорошем состоянии: " + goodCondition.ToString();
                OverdueReturns.Text = " С повреждениями/утеряны: " + (damagedCondition + lostCondition).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в updateReturns(): {ex.Message}");
            }
        }

        private void RefreshReturnsButton_Click(object sender, RoutedEventArgs e)
        {
            updateReturns();
        }



    }
}