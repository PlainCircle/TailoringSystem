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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StitchingShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Customer MyCustomer;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyCustomer = (Customer)this.FindResource("MyCustomer");
            MyCustomer.PhoneNumbers.Add(new PhoneNumber() { Customer = MyCustomer, Number = "1111111" });   
            MyCustomer.PhoneNumbers.Add(new PhoneNumber() { Customer = MyCustomer, Number = "2222222" });
            MyCustomer.PhoneNumbers.Add(new PhoneNumber() { Customer = MyCustomer, Number = "3333333" });
            MyCustomer.PhoneNumbers.Add(new PhoneNumber() { Customer = MyCustomer, Number = "4444444" });
            MyCustomer.PhoneNumbers.Add(new PhoneNumber() { Customer = MyCustomer, Number = "5555555" });
        }

        string result;
        void UpdateResult(string v) { result += (v + "\n"); }

        class MyCol<T> : MappingCollection<T> { }

        private void btnResetPhoneBook_Click(object sender, RoutedEventArgs e)
        {
            MyCol<int> myIntCol = new MainWindow.MyCol<int>();
            for (int i = 1; i <= 20; i++)
                myIntCol.Add(i);
            MyCol<string> myStrCol = (MyCol<string>)myIntCol.ConvertAll(i => "value = " + i.ToString());
            result = "";
            myStrCol.ForEach(UpdateResult);
            MessageBox.Show("result = " + result);
            //MyCustomer.ContactNumbers = "+92 1111111 /n+92 2222222/n+92 3333333/n+92 4444444/n+92 5555555";
        }
    }
}
