﻿using System;
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
using System.Text.RegularExpressions;
using BE;
using BL;
namespace PL
{
    /// <summary>
    /// Interaction logic for addUnitOptions.xaml
    /// </summary>
    public partial class addUnitOptions : Window
    {
        private IBL myBL;
        HostingUnit hostingUnit1;
        int hostNum;
        public addUnitOptions()
        {

            InitializeComponent();
            hostingUnit1 = new HostingUnit();
            myBL = factoryBL.getBL();
            hostNum = -1;
            cb_area.ItemsSource = Enum.GetValues(typeof(Enums.Area)).Cast<Enums.Area>();
            cb_hostingUnitType.ItemsSource = Enum.GetValues(typeof(Enums.HostingUnitType)).Cast<Enums.HostingUnitType>();
        }
        #region host details
        private void Tb_id_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                myBL.addHostNum(tb_id.Text, hostNum);
                hostingUnit1.Host.HostKey = hostNum;
            }
            catch (Exception ex)
            {
                tb_id_txt.Text = "הכנס קוד מארח\n " + ex.Message;
                tb_id_txt.Background = Brushes.Red;

            }
        }

        private void Tb_phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                myBL.checkPhone(tb_phone.Text, hostingUnit1.Host);
                tb_phonetxt.Background = Brushes.White;
            }
            catch
            {
                tb_phonetxt.Background = Brushes.Red;
            }
        }

        private void Tb_email_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                myBL.addMail(tb_email.Text, hostingUnit1.Host);
                tb_email_txt.Text = "כתובת מייל";
            }
            catch
            {
                tb_email_txt.Text = "כתובת מייל לא תקין.\n הכנס מייל";
            }
        }

        private void Tb_last_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(tb_last.Text, @"^[a-zA-Z]+$"))
            {
                hostingUnit1.Host.LastName = tb_last.Text;
            }
            else
            {
                tb_last.Text = "";
                tb_last.Background = Brushes.Red;
            }
        }

        private void Tb_first_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(tb_first.Text, @"^[a-zA-Z]+$"))
            {
                hostingUnit1.Host.Name = tb_first.Text;
            }
            else
            {
                tb_first.Text = "";
                tb_first.Background = Brushes.Red;
            }
        }
        #endregion
        #region checkboxes
        private void Cb_pool_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_pool.IsChecked == true)//changed to true
                hostingUnit1.Pool = Enums.Preference.Yes;
            else
            {
                hostingUnit1.Pool = Enums.Preference.No;
            }
        }
        private void Cb_garden_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_garden.IsChecked == true)//changed to true
                hostingUnit1.Garden = Enums.Preference.Yes;
            else
            {
                hostingUnit1.Garden = Enums.Preference.No;
            }
        }
        private void CheckBox_attractionsChecked(object sender, RoutedEventArgs e)
        {
            if (cb_attractions.IsChecked == true)//changed to true
                hostingUnit1.ChildrenAttractions = Enums.Preference.Yes;
            else
            {
                hostingUnit1.ChildrenAttractions = Enums.Preference.No;
            }
        }

        private void CheckBox_jaccuziChecked(object sender, RoutedEventArgs e)
        {
            if (cb_jaccuzi.IsChecked == true)//changed to true
                hostingUnit1.Jacuzzi = Enums.Preference.Yes;
            else
            {
                hostingUnit1.Jacuzzi = Enums.Preference.No;
            }
        }
        #endregion
        #region unit type and area
        private void Cb_area_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tb_area.Background = Brushes.White;
            hostingUnit1.AreaVacation = (Enums.Area)(cb_area.SelectedIndex);

        }

        private void Cb_hostingUnitType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tb_unitType.Background = Brushes.White;
            hostingUnit1.HostingUnitType = (Enums.HostingUnitType)(cb_hostingUnitType.SelectedIndex);
        }

        private void Tb_subAreaInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //add subarea to hosting unit or take awway from guest? need to check it's in the area
            //hostingUnit1.subArea=tb_subAreaInput.Text;
        }
        #endregion
        #region people
        private void Tb_Enter_Adults_TextChanged(object sender, TextChangedEventArgs e)
        {
            int text = 0;
            if (Int32.TryParse(tb_Enter_Adults.Text, out text))
            {
                if (text < 0)
                {
                    tb_Enter_Adults.Background = Brushes.OrangeRed;
                    tb_Enter_Adults.Text = "";
                }
                else
                {
                    hostingUnit1.NumAdult = text;
                    tb_Enter_Adults.Background = Brushes.White;
                }
            }
            else
            {
                tb_Enter_Adults.Background = Brushes.OrangeRed;
                tb_Enter_Adults.Text = "";
            }
        }


        private void Tb_Enter_Child_TextChanged(object sender, TextChangedEventArgs e)
        {
            int text = 0;
            if (Int32.TryParse(tb_Enter_Child.Text, out text))
            {
                if (text < 0)
                {
                    tb_Enter_Child.Background = Brushes.OrangeRed;
                    tb_Enter_Child.Text = "";
                }

                else
                {
                    hostingUnit1.NumChildren = text;
                    tb_Enter_Child.Background = Brushes.White;
                }
            }
            else
            {
                tb_Enter_Child.Background = Brushes.OrangeRed;
                tb_Enter_Child.Text = "";
            }
        }
        #endregion
    }
}
