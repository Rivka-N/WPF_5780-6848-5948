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
    /// Interaction logic for newHostingUnit.xaml
    /// </summary>
    public partial class newHostingUnit : Window
    {
        public IBL bL;
        public HostingUnit hosting = new HostingUnit();
        IEnumerable<IGrouping<int, BankAccount>> bankSource;//bank combobox source

        public newHostingUnit()
        {

            InitializeComponent();
            bL = factoryBL.getBL();
            #region init unit
            hosting.Garden = Enums.Preference.No;
            hosting.Pool = Enums.Preference.No;
            hosting.Jacuzzi = Enums.Preference.No;
            hosting.Host.Bank = new BankAccount();

            hosting.Host.CollectionClearance = false;
            #endregion
            #region enmus

            hostingUnitType.ItemsSource = Enum.GetValues(typeof(Enums.HostingUnitType)).Cast<Enums.HostingUnitType>();
            area.ItemsSource = Enum.GetValues(typeof(Enums.Area)).Cast<Enums.Area>();
            meal.ItemsSource = Enum.GetValues(typeof(Enums.MealType)).Cast<Enums.MealType>();
            #endregion

            #region bank
            bankSource = bL.groupBranchesByBank();//branches grouped by bank
            initBank(true);//sets banks and comboboxes. tells function this is the start
            #endregion
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource hostingUnitViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("hostingUnitViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // hostingUnitViewSource.Source = [generic data source]

            System.Windows.Data.CollectionViewSource bankAccountViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("bankAccountViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // bankAccountViewSource.Source = [generic data source]
        }

        #region mail
        private void Tb_mail_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                //checks it's valid mail
                if (cb_email.Text != "")//empty textbox
                {
                    hosting.Host.Mail = checkMail(cb_email.Text);//checks if it's an email
                    cb_email.BorderBrush = Brushes.Gray;
                }

            }
            catch
            {
                cb_email.Text = "";
                cb_email.BorderBrush = Brushes.Red;
            }
        }

        public System.Net.Mail.MailAddress checkMail(string text)//cheks if text is email address and returns it if it is
        {
            try
            {

                //if (!Regex.IsMatch(text, @"^[a-zA-Z0-9]+@{1}[a-zA-Z0-9]+\.[a-zA-Z]{1,3}$"))//letters and numbers in the beginning
                // throw new invalidFormatBL();//not mail format

                var mail = new System.Net.Mail.MailAddress(text);
                if (mail.Address != text)
                    throw new invalidFormatBL();
                return mail;
            }
            catch
            {
                throw new invalidFormatBL();
            }
        }
        #endregion

        #region buttonAdd

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Regex.IsMatch(cb_nameTextBox.Text, @"^[\p{L}]+$"))//first contains only letters
                {
                    if (Regex.IsMatch(NameTextBox.Text, @"^[\p{L}]+$"))//last contains only letters
                    {
                        if (isValidID(cb_phoneTextBox.Text))
                        {
                            if (hosting.Host.Mail != null && checkMail(cb_email.Text) != null)//recieved a mail address and it's still in the textbox
                            {
                                if (Regex.IsMatch(cb_hostingUnitNameTextBox.Text, @"^[\p{L} ]+$"))
                                {
                                    if (hostingUnitType.SelectedIndex != -1)

                                    {

                                        if (area.SelectedIndex != -1)
                                        {


                                            if (meal.SelectedIndex != -1)
                                            {
                                                if (hosting.NumAdult > 0 || hosting.NumChildren > 0)//checks there are people

                                                {
                                                    if (hosting.Pool != Enums.Preference.Maybe)
                                                    {
                                                        if (hosting.Jacuzzi != Enums.Preference.Maybe)
                                                        {
                                                            if (hosting.Garden != Enums.Preference.Maybe)
                                                            {
                                                                if (hosting.Host.Bank.BankAcountNumber > 0)//valid number
                                                                {
                                                                    if (cb_branchAddr.SelectedIndex != -1)//branch is selected
                                                                    {
                                                                        bL.addHostingUnit(hosting); //if it's all valid, adsds guest
                                                                        MessageBox.Show("HostingUnit " + hosting.HostingUnitName + " added Successfully! ");
                                                                        Close();//closes window
                                                                    }
                                                                    else
                                                                        MessageBox.Show("Please select your bank", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                                                }
                                                                else
                                                                {
                                                                    MessageBox.Show("Invalid bank number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                                } 
                                                            }

                                                            else MessageBox.Show("Select yes or no for garden status", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                                        }

                                                        else MessageBox.Show("Select yes or no for jacuzzi status", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                    }
                                                    else MessageBox.Show("Select yes or no for pool status", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                                }

                                                else MessageBox.Show("Number of people can't be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                            else MessageBox.Show("Select maels status", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                        else MessageBox.Show("Select area vacationpe", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                                    }

                                    else MessageBox.Show("Select unit type", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                                else MessageBox.Show("Please Enter unit name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            }
                            else
                            {
                                MessageBox.Show("Please enter email address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                cb_email.BorderBrush = Brushes.Red;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid id, Please enter new ID", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            cb_phoneTextBox.BorderBrush = Brushes.Red;
                        }

                    }
                    else
                    {
                        MessageBox.Show("Please enter last name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        NameTextBox.BorderBrush = Brushes.Red;
                    }
                }

                else
                {
                    MessageBox.Show("Please enter first name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    cb_nameTextBox.BorderBrush = Brushes.Red;
                }
            }

            catch (Exception ex)
            {
                if (ex is unfoundRequestExceptionBL)//no request found error
                {
                    MessageBox.Show("No units found. We will contact you if any become available.",
                         "Error in request", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }
        #endregion

        #region numbers
        private void numAdultTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                hosting.NumAdult = addNum(cb_numAdultTextBox.Text);//checks this is a valid number. -1 if not
                cb_numAdultTextBox.BorderBrush = Brushes.Gray;

            }
            catch (Exception ex)
            {
                cb_numAdultTextBox.Text = "";
                cb_numAdultTextBox.BorderBrush = Brushes.Red;//colors border
                if (ex is LargeNumberExceptionPL)
                    MessageBox.Show(ex.Message);//if the number was too big, explains why number wasn't valid

            }

        }

        private void numChildrenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                hosting.NumChildren = addNum(cb_numChildrenTextBox.Text);//checks this is a valid number. -1 if not
                cb_numChildrenTextBox.BorderBrush = Brushes.Gray;

            }
            catch (Exception ex)
            {
                cb_numChildrenTextBox.BorderBrush = Brushes.Red;//colors border
                cb_numChildrenTextBox.Text = "";
                if (ex is LargeNumberExceptionPL)
                    MessageBox.Show(ex.Message);//if the number was too big, explains why number wasn't valid
            }

        }


        private int addNum(string number)
        {
            int text = 0;
            if (!Int32.TryParse(number, out text))//if it's not a number
                throw new invalidTypeExceptionPL();
            if (text < 0)//not a valid number
                throw new negativeNumberExceptionPL();
            if (text > 1000)
                throw new LargeNumberExceptionPL("Number cannot be over 1000");
            return text;//if it's valid, returns it
        }

        private void phoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {


            Int32 text = 0;
            if (Int32.TryParse(cb_phoneTextBox.Text, out text))
            {
                if (text < 0)
                {
                    cb_phoneTextBox.BorderBrush = Brushes.Red;
                    cb_phoneTextBox.Text = "";
                }
                else
                {
                    hosting.Host.HostKey = text;
                    cb_phoneTextBox.BorderBrush = Brushes.Gray;
                    return;
                }

                
            }//if it's not valid
                cb_phoneTextBox.BorderBrush = Brushes.Red;
                cb_phoneTextBox.Text = "";
           

        }
        
        public static bool MatchStringFromRegex(string str, string regexstr)
        {
            str = str.Trim();
            System.Text.RegularExpressions.Regex pattern = new System.Text.RegularExpressions.Regex(regexstr);
            return pattern.IsMatch(str);
        }

        public static bool isValidID(string str)
        {
           if (Regex.IsMatch(str, (".*(\\d{9})")))
            {
                return true;
            }
            return false;
        }


        #endregion

        #region names
        private void lastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(NameTextBox.Text, @"^[a-zA-Z]+$"))
            {
                hosting.Host.LastName = NameTextBox.Text;
                NameTextBox.BorderBrush = Brushes.Gray;

            }
            else
            {
                NameTextBox.Text = "";
                NameTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(cb_nameTextBox.Text, @"^[a-zA-Z]+$"))//if contains only letters
            {
                hosting.Host.Name = cb_nameTextBox.Text;
                cb_nameTextBox.BorderBrush = Brushes.Gray;
            }
            else
            {
                cb_nameTextBox.Text = "";
                cb_nameTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void hostingUnitNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(cb_hostingUnitNameTextBox.Text, @"^[a-zA-Z ]+$"))//if contains only letters
            {
                hosting.HostingUnitName = cb_hostingUnitNameTextBox.Text;
                cb_hostingUnitNameTextBox.BorderBrush = Brushes.Gray;
            }
            else
            {
                cb_hostingUnitNameTextBox.Text = "";
                cb_hostingUnitNameTextBox.BorderBrush = Brushes.Red;
            }
        }
        private void hostingUnitcb_nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        #endregion

        #region comboBox
        private void cb_hostingUnitType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hostingUnitType.SelectedIndex != -1)//selected
                hosting.HostingUnitType = (Enums.HostingUnitType)hostingUnitType.SelectedIndex;
        }

        private void cb_area_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(area.SelectedIndex != -1)
                hosting.AreaVacation = (Enums.Area)(area.SelectedIndex);
        }

        private void cb_meal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(meal.SelectedIndex != -1)
                hosting.Meal = (Enums.MealType)(meal.SelectedIndex);
        }

        #endregion

        #region checkBox
        private void poolCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_poolCheckBox.IsChecked == true)//changed to true
                hosting.Pool = Enums.Preference.Yes;
            else
            {
                if (cb_poolCheckBox.IsChecked == false)//changed to false
                    hosting.Pool = Enums.Preference.No;
                else
                    hosting.Pool = Enums.Preference.Maybe;//otherwise it's the third state
            }
        }

        private void jacuzziCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_jacuzziCheckBox.IsChecked == true)//changed to true
                hosting.Jacuzzi = Enums.Preference.Yes;
            else
            {
                if (cb_jacuzziCheckBox.IsChecked == false)//changed to false
                    hosting.Jacuzzi = Enums.Preference.No;
                else
                    hosting.Jacuzzi = Enums.Preference.Maybe;//otherwise it's the third state
            }
        }

        private void gardenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_gardenCheckBox.IsChecked == true)//changed to true
                hosting.Garden = Enums.Preference.Yes;
            else
            {
                if (cb_gardenCheckBox.IsChecked == false)//changed to false
                    hosting.Garden = Enums.Preference.No;
                else
                    hosting.Garden = Enums.Preference.Maybe;//otherwise it's the third state
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            if (cb_collectionClearance.IsChecked == true)//changed to true
                hosting.Host.CollectionClearance = true;
            else
            {
                    hosting.Host.CollectionClearance = false;
               
            }
        }
        #endregion

        #region bank
        private void initBank(bool start = false)//binding between bank and bank source
        {
            int curBank = -1;//current index of bank

            if (start)//init on start
            {
                foreach (var bank in bankSource)
                {
                    cb_bankName.Items.Add(bank.First().BankName);//adds key of each group to list
                    cb_bankNumberTextBox.Items.Add(bank.First().BankNumber.ToString());
                }

                cb_bankName.SelectedIndex = -1;
                cb_bankNumberTextBox.SelectedIndex = -1;
            }
            else//afterwards
            {
                curBank = cb_bankNumberTextBox.SelectedIndex;//current bank is the number in the list of the index selected
            }
            if (!start && curBank != -1)
            {
                foreach (var bank in bankSource.ElementAt(curBank))
                {
                    cb_branchAddr.Items.Add(bank.BranchCity + " : " + bank.BranchAddress);//adds key of each group to list
                    cb_branchNumber.Items.Add(bank.BranchNumber.ToString());

                }
                cb_branchNumber.IsEnabled = true;
                cb_branchAddr.IsEnabled = true;
            }
                cb_branchAddr.SelectedIndex = -1;
                cb_branchNumber.SelectedIndex = -1;//currently no index selected
            
        }
        #region bank number checks
        private void BankAcountNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (Regex.IsMatch(bankAcountNumberTextBox.Text, ("^[0-9]+$")))//only numbers
            {
                bankAcountNumberTextBox.BorderBrush = Brushes.Gray;
                hosting.Host.Bank.BankAcountNumber = Convert.ToInt32(bankAcountNumberTextBox.Text);//sets new bank number
               
            }
            else
            {
                bankAcountNumberTextBox.BorderBrush = Brushes.Red;
                bankAcountNumberTextBox.Text = "";
            }
        }
        private void BankAcountNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Regex.IsMatch(bankAcountNumberTextBox.Text, ("^[0-9]+$")))//only numbers
                {
                    bankAcountNumberTextBox.BorderBrush = Brushes.Gray;
                    hosting.Host.Bank.BankAcountNumber = Convert.ToInt32(bankAcountNumberTextBox.Text);
                }
                else
                {
                    bankAcountNumberTextBox.BorderBrush = Brushes.Red;
                    bankAcountNumberTextBox.Text = "";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
        #region comboboxes
        private void Cb_bankName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_bankNumberTextBox.SelectedIndex = cb_bankName.SelectedIndex;//selects that index in the bank names
            initBank();//sends to find correct group of branches.

        }
        private void Cb_bankNumberTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_bankName.SelectedIndex = cb_bankNumberTextBox.SelectedIndex;//selects that index in the bank numbers

        }

        #region branch
        private void Cb_branchAddr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_branchNumber.SelectedIndex = cb_branchAddr.SelectedIndex;
        }

        private void Cb_branchNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_branchAddr.SelectedIndex = cb_branchNumber.SelectedIndex;//sets others index
            setBank();//sets to functoin to set host's bank

        }
        private void setBank()
        {
            var b = bankSource.ElementAt(cb_bankNumberTextBox.SelectedIndex).ElementAt(cb_branchNumber.SelectedIndex);
            //selects from bank list this person's branch item

            hosting.Host.Bank.BankName = b.BankName; //sets bank
            hosting.Host.Bank.BankNumber = b.BankNumber;
            hosting.Host.Bank.BranchAddress = b.BranchAddress;
            hosting.Host.Bank.BranchCity = b.BranchCity;
            hosting.Host.Bank.BranchNumber = b.BranchNumber;
               
            }

        #endregion

        private void Cb_collectionClearance_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_collectionClearance.IsChecked == true)
                hosting.Host.CollectionClearance = true;
            else
                hosting.Host.CollectionClearance = false;

        }
        #endregion

        #endregion


    }
}





