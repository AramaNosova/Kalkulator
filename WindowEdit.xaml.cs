using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SportsRentalSystem
{
    public partial class WindowEdit : Window
    {
        private Equipment currentEquipment;

        public WindowEdit()
        {
            InitializeComponent();
            LoadCategories();
            LoadStatuses();
            LoadEquipmentData();
        }

        private void LoadEquipmentData()
        {
            if (App.equipment != null)
            {
                currentEquipment = App.equipment;

                // Заполняем поля данными выбранного оборудования
                txtName.Text = currentEquipment.Name;
                txtDescription.Text = currentEquipment.Description;
                txtSerialNumber.Text = currentEquipment.SerialNumber;
                txtDailyCost.Text = currentEquipment.DailyRentalCost.ToString();
                txtHourlyCost.Text = currentEquipment.HourlyRentalCost?.ToString() ?? "";
                txtDeposit.Text = currentEquipment.DepositAmount.ToString();
                txtNotes.Text = currentEquipment.Notes;

                // Устанавливаем выбранные категорию и статус
                cmbCategory.SelectedValue = currentEquipment.CategoryID;
                cmbStatus.SelectedValue = currentEquipment.StatusID;
            }
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    cmbCategory.ItemsSource = context.Categories.ToList();
                    cmbCategory.SelectedValuePath = "CategoryID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void LoadStatuses()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    cmbStatus.ItemsSource = context.Statuses.ToList();
                    cmbStatus.SelectedValuePath = "StatusID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название оборудования");
                    return;
                }

                if (cmbCategory.SelectedItem == null)
                {
                    MessageBox.Show("Выберите категорию");
                    return;
                }

                if (cmbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус");
                    return;
                }

                // Обновление данных в базе
                using (var context = new SportsRentalSystemEntities())
                {
                    var equipmentToUpdate = context.Equipment.Find(currentEquipment.EquipmentID);

                    if (equipmentToUpdate != null)
                    {
                        equipmentToUpdate.Name = txtName.Text;
                        equipmentToUpdate.CategoryID = (int)cmbCategory.SelectedValue;
                        equipmentToUpdate.StatusID = (int)cmbStatus.SelectedValue;
                        equipmentToUpdate.Description = txtDescription.Text;
                        equipmentToUpdate.SerialNumber = txtSerialNumber.Text;
                        equipmentToUpdate.DailyRentalCost = decimal.Parse(txtDailyCost.Text);
                        equipmentToUpdate.HourlyRentalCost = string.IsNullOrWhiteSpace(txtHourlyCost.Text) ?
                            (decimal?)null : decimal.Parse(txtHourlyCost.Text);
                        equipmentToUpdate.DepositAmount = decimal.Parse(txtDeposit.Text);
                        equipmentToUpdate.Notes = txtNotes.Text;

                        context.SaveChanges();

                        RefreshMainWindowData();
                        MessageBox.Show("Оборудование успешно обновлено!");
                        this.DialogResult = true;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении: " + ex.Message);
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