using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StitchingShop
{
    partial class Customer
    {
        public static DependencyProperty ContactNumbersProperty = DependencyProperty.Register("ContactNumbers",
            typeof(string), typeof(Customer), new PropertyMetadata(default(string), OnContactNumbersChangedCallback, OnContactNumbersCoerceValueCallback));

        static void OnContactNumbersChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Customer obj = (Customer)d;
            string[] newNumbers = ((string)e.NewValue).Split(new string[]{"\n", "\r"},  StringSplitOptions.RemoveEmptyEntries);
            int i;
            int SourceCount = newNumbers.Length;
            for (i = 0; i < SourceCount; i++)
                newNumbers[i]= newNumbers[i].Trim();
            Array.Sort(newNumbers);
            i = 1;
            int j=0;
            while(i < SourceCount)
            {
                if (newNumbers[j] != newNumbers[i])
                {
                    j++;
                    if (j < i)
                        newNumbers[j] = newNumbers[i];
                }
                i++;
            }
            if (SourceCount>0)
                SourceCount = j+1;

            int DestCount = obj.PhoneNumbers.Count;
            i = SourceCount - 1;
            j = DestCount - 1;
            int result;
            PhoneNumber NewPhoneNumber;
            while ((i >= 0) && (j >= 0))
            {
                result = string.CompareOrdinal(obj.PhoneNumbers[i].Number, newNumbers[j]);
                if (result < 0)
                {
                    NewPhoneNumber = new PhoneNumber();
                    NewPhoneNumber.Customer = obj;
                    NewPhoneNumber.Number = newNumbers[j];
                    obj.PhoneNumbers.Add(NewPhoneNumber);
                    j--;
                }
                else if (result > 0)
                {
                    obj.PhoneNumbers.Remove(obj.PhoneNumbers[i]);
                    i--;
                }
                else
                {
                    i--;
                    j--;
                }
            }                
        }
        static object OnContactNumbersCoerceValueCallback(DependencyObject d, object baseValue)
        {
            Customer obj = (Customer)d;
            StringBuilder sb = new StringBuilder();
            foreach (var phonenumber in obj.PhoneNumbers)
                sb.AppendLine(phonenumber.Number);
            return sb.ToString();
        }
        public string ContactNumbers
        {
            get
            {
                return ((string)(base.GetValue(Customer.ContactNumbersProperty)));
            }
            set
            {
                base.SetValue(Customer.ContactNumbersProperty, value);
            }
        }


    }
}
