using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SportsRentalSystem
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            LoadCategories();
            LoadStatuses();
        }

        private void LoadCategories()
        {
            try
            {
                using (var context = new SportsRentalSystemEntities())
                {
                    cmbCategory.ItemsSource = context.Categories.ToList();
                    cmbCategory.SelectedIndex = 0;
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
                    cmbStatus.SelectedIndex = 0;
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

                // Сохранение в базу данных
                using (var context = new SportsRentalSystemEntities())
                {
                    var equipment = new Equipment
                    {
                        Name = txtName.Text,
                        CategoryID = ((Categories)cmbCategory.SelectedItem).CategoryID,
                        StatusID = ((Statuses)cmbStatus.SelectedItem).StatusID,
                        Description = txtDescription.Text,
                        SerialNumber = txtSerialNumber.Text,
                        DailyRentalCost = decimal.Parse(txtDailyCost.Text),
                        HourlyRentalCost = string.IsNullOrWhiteSpace(txtHourlyCost.Text) ?
                            (decimal?)null : decimal.Parse(txtHourlyCost.Text),
                        DepositAmount = decimal.Parse(txtDeposit.Text),
                        ReceiptDate = DateTime.Today,
                        Notes = txtNotes.Text
                    };

                    context.Equipment.Add(equipment);
                    context.SaveChanges();

                    MessageBox.Show("Оборудование успешно добавлено!");
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

        // Перетаскивание окна
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }
    }
}
