using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SportsRentalSystem.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _selectedTab;

        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TabItem> Tabs { get; set; }

        public MainWindowViewModel()
        {
            InitializeTabs();
            SelectedTab = "Equipment";
        }

        private void InitializeTabs()
        {
            Tabs = new ObservableCollection<TabItem>
            {
                new TabItem { Header = "Оборудование", ContentType = TabContentType.Equipment, IsEnabled = true },
                new TabItem { Header = "Клиенты", ContentType = TabContentType.Customers, IsEnabled = true },
                new TabItem { Header = "Сотрудники", ContentType = TabContentType.Employees, IsEnabled = true },
                new TabItem { Header = "Прокат", ContentType = TabContentType.Rentals, IsEnabled = true },
                new TabItem { Header = "Возвраты", ContentType = TabContentType.Returns, IsEnabled = true },
                new TabItem { Header = "Отчеты", ContentType = TabContentType.Reports, IsEnabled = true }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TabItem
    {
        public string Header { get; set; }
        public TabContentType ContentType { get; set; }
        public bool IsEnabled { get; set; }
    }

    public enum TabContentType
    {
        Equipment,
        Customers,
        Employees,
        Rentals,
        Returns,
        Reports
    }
}