using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SportsRentalSystem
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SportsRentalSystemEntities sports = new SportsRentalSystemEntities();
        public static Employees employees = new Employees();
        public static Equipment equipment = new Equipment();
        public static Employees selectedEmployee = new Employees();
        public static Clients selectedClient = new Clients();
 
        public static Returns selectedReturn = null; // Добавляем для возвратов
        public static Rentals selectedRental { get; set; }
    }
}
