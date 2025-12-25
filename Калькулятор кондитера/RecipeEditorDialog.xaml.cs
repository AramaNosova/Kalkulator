using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace ConfectioneryCalculator.Views
{
    public partial class RecipeEditorDialog : Window
    {
        private Product currentProduct;
        private ObservableCollection<RecipeItem> recipeItems;
        private AppDbContext dbContext;
        
        public RecipeEditorDialog(int productId)
        {
            InitializeComponent();
            dbContext = new AppDbContext();
            currentProduct = dbContext.Products.Find(productId);
            recipeItems = new ObservableCollection<RecipeItem>();
            LoadRecipe();
            InitializeControls();
        }
        
        private void InitializeControls()
        {
            var ingredients = dbContext.Ingredients.ToList();
            IngredientComboBox.ItemsSource = ingredients;
            MarginTextBox.Text = "50";
            MarginTextBox.TextChanged += MarginTextBox_TextChanged;
            UpdateCalculations();
        }
        private void CalculateFinalPrice()
        {
            try
            {
                decimal rawCost = CalculateRawCost();

                decimal marginPercent = decimal.Parse(MarginTextBox.Text);
                // Итоговая цена = Сырьевая себестоимость × (1 + Наценка/100)
                decimal marginMultiplier = 1 + (marginPercent / 100);
                decimal finalPrice = rawCost * marginMultiplier;

                FinalPriceLabel.Content = $"{finalPrice:F2} ₽";

            }
            catch (Exception)
            {
                FinalPriceLabel.Content = "0.00 ₽";
            }
        }
        
        // Метод расчета себестоимости сырья
        private decimal CalculateRawCost()
        {
            decimal total = 0;
            foreach (var item in recipeItems)
            {
                // Стоимость = Количество × Цена за единицу
                total += item.Quantity * item.Ingredient.PricePerUnit;
            }
            return total;
        }
        
        // ⭐ Обработчик изменения наценки (реальный расчет)
        private void MarginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматический пересчет при вводе наценки
            UpdateCalculations();
        }
        
        private void UpdateCalculations()
        {
            // Обновляем себестоимость
            decimal rawCost = CalculateRawCost();
            RawCostLabel.Content = $"{rawCost:F2} ₽";
            
            // ⭐ Пересчитываем цену с наценкой
            CalculateFinalPrice();
        }
        
        // Сохранение рецепта
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal rawCost = CalculateRawCost();
                decimal marginPercent = decimal.Parse(MarginTextBox.Text);
                
                // ⭐ Расчет финальной цены для сохранения
                decimal finalPrice = rawCost * (1 + (marginPercent / 100));
                
                // Обновляем продукт в БД
                currentProduct.RawCost = rawCost;
                currentProduct.FullCost = finalPrice; // Сохраняем итоговую цену
                
                // Сохраняем изменения рецепта
                SaveRecipeItems();
                
                dbContext.SaveChanges();
                MessageBox.Show("Рецепт сохранен!", "Успех");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        
        // ... остальной код ...
    }
}

