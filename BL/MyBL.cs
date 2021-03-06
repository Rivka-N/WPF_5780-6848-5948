﻿using BE;
using DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Net.Http;
using System.Threading;

namespace BL
{
    public sealed class MyBL : IBL
    {
        static IDAL myDAL;

        #region Singleton
        private static readonly MyBL instance = new MyBL();
        public static MyBL Instance
        {
            get { return instance; }
        }

        static MyBL()
        {
                        
            myDAL = factoryDAL.getDAL();
            new Thread(() =>
            {
                if (myDAL.getLastUpdatedStatus() < DateTime.Today)//only updates them if it didn't update today already
                {
                    List<Order> orders = myDAL.getAllOrders();//all orders
                    myDAL.changeOrderStatus(ord => ord.Status == Enums.OrderStatus.Mailed && ord.OrderDate < DateTime.Today.AddDays(30), Enums.OrderStatus.Expired);
                    //changes status of orders more than 30 days old
                    myDAL.setLastUpdatedStatus();//updates date to datetime.today
                }
                Thread.Sleep(86400000);//sleeps for 24 hours
            }).Start();//starts it
        }
        private MyBL()
        {
            
        }
        #endregion

        #region add items

        public void addHostingUnit(HostingUnit unit)//add hostingUnit to the hostingUnit list in DS
        {
            myDAL.addHostingUnit(unit.Clone());
        }
        public void addOrder(Order ord)//add order to the order list in DS
        {
            myDAL.addOrder(ord.Clone());
        }


        #endregion

        #region calculate data
        public int TotalSumCollectedFromUnits()//calculates amoutn of money collected from all hosting units together
        {
            var sum = 0;
            var units = myDAL.getAllHostingUnits();//saves units
            foreach (HostingUnit hu in units)//goes over units
                sum += hu.MoneyPaid;//adds money paid from this unit to toal
            return sum;
        }
        #endregion

        #region add guest
        public List<HostingUnit> findUnit(GuestRequest guest)//finds applicable units for request and sends mail to hosts
        {
            var units = myDAL.getAllHostingUnits();
            List<HostingUnit> listOfUnits = new List<HostingUnit>();
            for (int i = 0; i < units.Count(); i++)
            {
                if (matchesUnit(units[i], guest))//if guest can be put in his unit
                {
                    listOfUnits.Add(units[i]);//adds to the guest list
                }
            }
            if (listOfUnits.Count() == 0)
                throw new unfoundRequestExceptionBL();
            mail(listOfUnits, guest);  //sends mail to all of the hosts 

            return listOfUnits;
        }

        public void addGuest(GuestRequest guest)//add guest to the data list in DS
        {
            myDAL.addGuest(guest.Clone());//adds guest to list
            findUnit(guest);//tries to find units for this guest request
        }
        #endregion

        #region order

        public void mail(List<HostingUnit> Offers, GuestRequest guest)//sends mail with guest details to the host
        {
            //sends mail to the hosts

           
                #region send mail to hosts
                new Thread(() =>
                {
                    try
                    {
                        MailMessage mail = new MailMessage();
                        foreach (HostingUnit hosting in Offers)
                        {
                            mail.To.Add(hosting.Host.Mail);

                        }
                        mail.From = new MailAddress("amazingvacations169@gmail.com", "Amazing Vacations");
                        mail.Subject = "Guest Request";
                        mail.Body = "Hello! \n" + guest.Name + "is intresting in your hostingUnit, for more datails please log in yo your personal area and connect with your guest";
                        SmtpClient smtp = new SmtpClient();
                        smtp.UseDefaultCredentials = false;
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        smtp.Credentials = new System.Net.NetworkCredential("amazingvacations169@gmail.com", "vacation169");

                        smtp.EnableSsl = true;


                        smtp.Send(mail);//send mail
                    }
                    catch (Exception ex)
                    {
                        //try to send mail again with a few second wait?
                        throw new networkErrorExceptionBL("unable to send mail: " + ex.Message);
                    }
                }).Start();
                #endregion
            
        }

        public void sendGuestMail(HostingUnit unit, GuestRequest guest)//guest and hosting unit, sends mail to guest and creates order from details
        {
            new Thread(() =>
            {
                try
                {

                    #region send mail
                    //add new background worker here
                    MailMessage mail = new MailMessage();
                    mail.To.Add(guest.Mail);
                    mail.From = new MailAddress("amazingvacations169@gmail.com", "Amazing Vacations");
                    mail.Subject = "Hosting Unit Offer";
                    //mail.Body = "Hello " + guest.Name + "\nWe found a " + guest.TypeOfUnit + " for you.\n Here are the details:\n" + unit.ToString() +
                    //    "\nPlease respond to " + unit.Host.Mail + " and finalize the details\n";//change the to string

                    string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\HTML.html");

                    string mailBody = File.ReadAllText(filePath);
                    mailBody = mailBody
                        .Replace("@@name@@", guest.Name)
                        .Replace("@@hostingType@@", (unit.HostingUnitType).ToString())
                        .Replace("@@hostMail@@", (unit.Host.Mail).ToString());

                    mail.Body = mailBody;

                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.UseDefaultCredentials = false;
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    smtp.Credentials = new System.Net.NetworkCredential("amazingvacations169@gmail.com", "vacation169");

                    smtp.EnableSsl = true;


                    smtp.Send(mail);//send mail

                    #endregion

                    myDAL.changeStatus(guest, Enums.OrderStatus.Mailed);//mailed status
                    Order ord = new Order();//makes new order
                    ord.HostingUnitKey = unit.HostingUnitKey;
                    ord.GuestRequestKey = guest.GuestRequestKey;
                    ord.HostName = unit.Host.Name + " " + unit.Host.LastName;
                    ord.GuestName = guest.Name + " " + guest.LastName;
                    ord.OrderDate = DateTime.Today;//sent mail today
                    ord.CreateDate = guest.Registration;//original request created
                    addOrder(ord);//send to the function which adds the order to the order list

                }
                catch (Exception ex)
                {
                    //try to send mail again with a few second wait?
                    throw new networkErrorExceptionBL("unable to send mail: " + ex.Message);
                }

            }).Start();
        }

        public bool availableDates(HostingUnit unit, GuestRequest guest)//checks if guest request's dates are available in this unit
        {
            try
            {
                DateTime end = guest.ReleaseDate;
                for (DateTime start = guest.EntryDate; start < end; start = start.AddDays(1))//Check availability
                {

                    if (unit.Diary[start.Month, start.Day] == true)
                    {
                        return false;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidException(ex.Message);
            }

        }

     
        public void order(HostingUnit unit, GuestRequest guest)//final order
                                                               //makes sure that the days in the request available
                                                               //update guest status
                                                               //take off transaction fee
        {
            try
            {
                DateTime end = guest.ReleaseDate;
                for (DateTime start = guest.EntryDate; start < end; start = start.AddDays(1))//Check availability
                {
                    if (unit.Diary[start.Month, start.Day] == true)
                    {
                        throw new overbookedExceptionBL();//if its already occupied
                    }
                }
                for (DateTime start = guest.EntryDate; start <= end; start = start.AddDays(1))//set the days
                {
                    unit.Diary[start.Month, start.Day] = true;
                }

                Order thisOrder = findOrder(guest, unit);
                myDAL.deleteOrders(order => { return order.GuestRequestKey == thisOrder.GuestRequestKey && order.HostingUnitKey != thisOrder.HostingUnitKey; });
                //deletes orders with the same guestrequestKey as this one
                myDAL.changeOrderStatus((order => order.OrderKey== thisOrder.OrderKey), Enums.OrderStatus.Closed);//changes order status to closed
                //changes current order status
                myDAL.changeStatus(guest, Enums.OrderStatus.Closed);//changes guest status
                int numDays = numOfDays(guest.EntryDate, guest.ReleaseDate);
                myDAL.changeUnit(unit.Clone());//updates unit matrix
                myDAL.addCharge(unit, numDays);//adds charge for number of days guest is staying
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int numOfDays(DateTime start, DateTime end)//number of days between start and end
        {
            return (end - start).Days;
        }

        private Order findOrder(GuestRequest guest, HostingUnit unit)//returns the order with this hosting unit and this guest
        {
            var ords = getOrders( order => order.GuestRequestKey == guest.GuestRequestKey && order.HostingUnitKey == unit.HostingUnitKey);//gets order from guest and unit
            return ords.First();//returns first item found
        }

        private bool matchesUnit(HostingUnit unit, GuestRequest guest)//returns true if guest can be placed in this unit. false otherwise
        {
            if (unit.Host.CollectionClearance && guest.Status!=Enums.OrderStatus.Closed)//if the host has collection clearance and the guest isn't already booked
                if (guest.TypeOfUnit == unit.HostingUnitType && guest.AreaVacation == unit.AreaVacation && availableDates(unit, guest))
                    if ((guest.NumAdult+guest.NumChildren) <= (unit.NumAdult + unit.NumChildren +5))//if the size of the hotel almost matches request
                        return true;//this person can be in his unit
            return false; //can't have anyone in his unit
        }

        

        #endregion

        #region find methods
        public HostingUnit findUnit(int unitKey)//finds unit based on key
        {
            if (unitKey < 0)
                throw new InvalidException("invalid unit key");
            var unit = myDAL.findUnit(unitKey);
            return (unit == null) ? throw new unfoundRequestExceptionBL("unit not found") : unit;
        }

        public GuestRequest findGuest(GuestRequest g1, string text)//find guest based on text of guestnum.
        {
            int guestNum;
            if (!Int32.TryParse(text, out guestNum) || guestNum <= 0)
                throw new InvalidException("invalid guest number");
            g1.GuestRequestKey = guestNum;
            return (myDAL.findGuest(g1));
        }

        #endregion
        #region search filtering methods
        public List<HostingUnit> searchUnits(string text, int unitType, int area, Enums.FunctionSender sender)
        {
            Func<HostingUnit, bool> conditions = null;
            if (text == null)
                throw new InvalidException("invalid text");
            switch (sender)
            {
                default:
                    conditions = (unit =>
                    (Enum.IsDefined(typeof(Enums.HostingUnitType), unitType) ? unit.HostingUnitType == (Enums.HostingUnitType)unitType : true)
                    && ((Enum.IsDefined(typeof(Enums.Area), area)) ? unit.AreaVacation == (Enums.Area)area : true)//adds unittype to condition
                && (unit.HostingUnitKey.ToString().Contains(text) || unit.Host.HostKey.ToString().Contains(text)
                || unit.HostingUnitName.Contains(text)
                || unit.Host.Name.Contains(text) || unit.Host.LastName.Contains(text)));
                    break;
            }
            return getHostingUnits(conditions);//returns all units that match conditions
        }

        public List<HostingUnit> searchUnits(string text, Enums.FunctionSender fs=0)//returns all units that this text was found in
        {   if (text == null || text == "")//no string
                return getAllHostingUnits();//returns all units
            switch (fs)
            {
                default://returns search through all units details
            return getHostingUnits
                (u => u.HostingUnitKey.ToString().Contains(text) || u.MoneyPaid.ToString().Contains(text)
                || u.HostingUnitType.ToString().Contains(text) || u.HostingUnitName.Contains(text)
                || u.Host.HostKey.ToString().Contains(text) || u.Host.Name.Contains(text) || u.Host.LastName.Contains(text)
                ||u.Host.Mail.Address.ToString().Contains(text));//returns all units that contain the text in their details
        }
        }
        public List<GuestRequest> searchRequests(Enums.OrderStatus status, DateTime? selectedDate, string query, Enums.FunctionSender owner=Enums.FunctionSender.Default)
        {
            Func<GuestRequest, bool> p = null;
            try
            {
                if (query == null)
                    throw new InvalidException("error in search query");
                switch (owner)
                {
                    case Enums.FunctionSender.Owner:
                        p= guest => guest.Status == status && (guest.Name.Contains(query) || guest.LastName.Contains(query)
                             || guest.GuestRequestKey.ToString().Contains(query) || guest.Mail.Address.Contains(query));//sets condition
                        break;
                    default:
                        p= guest => guest.Status == status
                          && (guest.TypeOfUnit.ToString().Contains(query) || guest.Name.Contains(query) || guest.LastName.Contains(query)
                          || guest.GuestRequestKey.ToString().Contains(query) || guest.Mail.Address.Contains(query));
                        break;
                }
                var reqs = getRequests(p);//gets request that match all conditions
                if (selectedDate == null)//no date selected
                    return reqs;//returns requests found
                else//also date to filter by
                {
                    return reqs.Where(gr => gr.Registration == selectedDate).Select(guest=>guest).ToList();//returns list filtered by date
                }
            }
            catch(Exception ex)
            {
               
                throw new InvalidException("Unable to find items");
            }
        }

        public Func<GuestRequest, bool> GuestSearchQuery(string query, string child, string adult, bool? garden, bool? jaccuzi, bool? pool, int meal)
            //builds function to sort guests by
        {
            try
            {
                Func<GuestRequest, bool> text = (g=> true), children= (g => true), adults= (g => true), wantGarden= (g => true), wantJ= (g => true), wantPool= (g => true), wantMeal=(g => true);
                //goes one by one and sets all of the queries based on the value of the items
                #region conditions
                if (query != null && query != "")//has query
                    text = guest => guest.Name.Contains(query) || guest.LastName.Contains(query)
                              || guest.GuestRequestKey.ToString().Contains(query);
                if (child != "")
                    children = (guest => guest.NumChildren.ToString() == child);
                if (adult != "")
                    adults = (guest => guest.NumAdult.ToString() == adult);
                if (garden != null)
                {
                    if (garden == false)//no
                        wantGarden = (guest => guest.Garden == Enums.Preference.No);
                    else
                        wantGarden = (guest => guest.Garden == Enums.Preference.Yes);
                    
                }

                if (pool != null)
                {
                    if (pool == false)//no
                        wantPool = (guest => guest.Pool== Enums.Preference.No);
                    else
                        wantPool= (guest => guest.Pool== Enums.Preference.Yes);

                }//true, false, null
                if (jaccuzi != null)
                {
                    if (jaccuzi== false)//no
                        wantJ= (guest => guest.Jacuzzi == Enums.Preference.No);
                    else
                        wantJ= (guest => guest.Jacuzzi == Enums.Preference.Yes);

                }
                if (meal!=-1)//something was selected
                {
                    if (meal == 0)
                    {
                        wantMeal = (guest => guest.Meal == Enums.MealType.Full);
                    }
                    else if (meal == 1)
                    {
                        wantMeal = (guest => guest.Meal == Enums.MealType.Half);
                    }
                    else if (meal == 2)
                    {
                        wantMeal = (guest => guest.Meal == Enums.MealType.None);

                    }
                    else throw new invalidFormatBL();//not a valid number from the combobox
                        
                 }
                #endregion
                return gu => text(gu) && children(gu) && adults(gu) && wantGarden(gu) && wantJ(gu) && wantPool(gu) && wantMeal(gu);
                //returns all conditions combined
            }
            catch (Exception ex)
            {
                throw new InvalidException(ex.Message);
            }

        }

        public Func<GuestRequest, bool> GuestSearchQuery(DateTime? selectedDate, string query, Enums.FunctionSender owner)//calculates condition to filter by
        {
            try
            {
                Func<GuestRequest, bool> p = null;
                if (query == null)
                    throw new InvalidException("error in search query");
                switch (owner)
                {
                    case Enums.FunctionSender.Owner:
                        p = guest => guest.Name.Contains(query) || guest.LastName.Contains(query)
                               || guest.GuestRequestKey.ToString().Contains(query) || guest.Mail.Address.Contains(query);//initial conditions

                        if (selectedDate != null)
                            return (g => p(g) && g.Registration == selectedDate);
                        break;
                    case Enums.FunctionSender.Host:
                        p = guest => guest.Name.Contains(query) || guest.LastName.Contains(query);//initial conditions
                        if (selectedDate != null)
                            return (g => p(g) && g.EntryDate == selectedDate);//what date to filter by?
                        break;
                    default:
                        p = guest => guest.TypeOfUnit.ToString().Contains(query) || guest.Name.Contains(query) || guest.LastName.Contains(query)
                           || guest.GuestRequestKey.ToString().Contains(query) || guest.Mail.Address.Contains(query);
                        if (selectedDate != null)
                            return (g => p(g) && g.Registration == selectedDate);
                        break;
                }
                return p;
            }
            catch (Exception ex)
            {
                throw new InvalidException(ex.Message);
            }
                
        }
        public List<GuestRequest> searchRequests(DateTime? selectedDate, string query, Enums.FunctionSender owner)//all statuses selected
        {
            try
            {
                var reqs = GuestSearchQuery(selectedDate, query, owner);
                return getRequests(reqs);//gets all requests who match condition
            }
            catch(Exception ex)
            {

                if (ex is NullReferenceException)
                    return null;//no requests
                throw new invalidFormatBL("Unable to find items");//query was is wrong format
            }
        }

        public List<Order> searchOrders(DateTime? selectedDate, string text, Enums.FunctionSender owner, Enums.OrderStatus status=Enums.OrderStatus.Closed)//filters from all orders based on parameters recieved
        {
            Func<Order, bool> condition = null;//conditions to filter with
            Func<Order, bool> dateCondition=null;//conditions to filter with including date
            var orders = myDAL.getAllOrders();//all orders to filter from
            IEnumerable<Order> ordersToReturn=null;//list of filtered orders
            switch (owner)//sets conditions based on who sent to function and what conditions it wants to be checked
            {
                case Enums.FunctionSender.Owner:
                    if (text!=null && text!="")//text was entered
                  condition = ord => ord.Status == status
                  && (/*(ord.HostName != null && ord.GuestName != null && ord.HostName.Contains(text) || ord.GuestName.Contains(text))//checks first that guest and host name exist
                  || */ ord.GuestRequestKey.ToString().Contains(text) || ord.HostingUnitKey.ToString().Contains(text)
                  || ord.OrderKey.ToString().Contains(text));//sets function with conditions to check
                    // sees if date selected and sets function accordingly
                    else condition = ord => ord.Status == status;
                    break;

                default:
                    if (text != null && text != "")//text was entered
                        condition = ord => ord.Status == status
                    && ((ord.HostName != null && ord.GuestName != null && ord.HostName.Contains(text) || ord.GuestName.Contains(text))//checks first that guest and host name exist
                    || ord.GuestRequestKey.ToString().Contains(text) || ord.HostingUnitKey.ToString().Contains(text)
                    || ord.OrderKey.ToString().Contains(text));//sets function with conditions to check
                    else
                        condition = ord => ord.Status == status;
                    break;
                  
                    
            }
            if (selectedDate != null)//checks if there's a date selected and updates function accordingly
            {
                dateCondition = ord => condition(ord) && ord.CreateDate == selectedDate;
            }
            else//no date selected
                dateCondition = ord => condition(ord);
            if (orders == null)
                return orders;//nothing there
            ordersToReturn =
                     from ord in orders
                     let p = dateCondition(ord) //checks that all conditions apply
                       where p
                     select ord;
            return ordersToReturn.ToList();//converts to list and returns
        }

        #endregion
        #region gets

        public List<GuestRequest> getReleventRequests(HostingUnit unit)//finds requests relevent for this unit
        {
            try
            {
                var orders = getOrders(ord => unit.HostingUnitKey == ord.HostingUnitKey && ord.Status != Enums.OrderStatus.Closed);//finds requests for this unit that aren't closed
                List<GuestRequest> relevent = new List<GuestRequest>();//makes list to keep guest requests to return 

                var releventQuery = from g in getRequests()
                                    where matchesUnit(unit, g)
                                    select g;//selects guests the ones that can fit in unit
                relevent = releventQuery.ToList();//saves as list
                if (orders != null)
                {
                    foreach (Order o in orders)//goes over mailed orders found
                        if (relevent.Where(g => g.GuestRequestKey == o.GuestRequestKey).Select(g => g) == null)//if didn't find other requests with the same guest request key
                            relevent.Add(getRequests(req => req.GuestRequestKey == o.GuestRequestKey)[0]);//adds first of list of guests for this order found (it should only find one)
                }
                    return relevent;
            }
            catch
            {
                return null;//none found
            }
        }


        public List<HostingUnit> getAllHostingUnits()
        {
            return myDAL.getAllHostingUnits();
        }

        public List<HostingUnit> getHostingUnits(Func<HostingUnit, bool> p)
        {
            return myDAL.getAllHostingUnits().Where(p).Select(hu => (HostingUnit)hu.Clone()).ToList();
        }

        public List<GuestRequest> getRequests()
        {
            return myDAL.getRequests();
        }
        
        public List<Order> getAllOrders()
        {
            return myDAL.getAllOrders();
        }
        public List<GuestRequest> getRequests(Func<GuestRequest, bool> predicate)
        {
            
            var requests = from guest in myDAL.getRequests()
                           let p = predicate(guest)
                           where p
                           select guest.Clone();
            return requests.ToList();
        }


        public List<Order> getOrders(Func<Order, bool> predicate)
        {
            if (myDAL.getAllOrders()==null)
                return null;
            var ords = from ord in myDAL.getAllOrders()
                       let p = predicate(ord)
                       where p
                       select ord.Clone();
            return ords.ToList();
        }

        #endregion
        #region change
        public void changeUnit(HostingUnit hostingUnit1)
        {
            myDAL.changeUnit(hostingUnit1);
        }
        public HostingUnit copy(HostingUnit hosting)//returns copy of the hosting unit
        {
            return hosting.Clone();
        }

        #endregion
        #region delete
        public void deleteUnit(int unit)
        {
            try
            {

                HostingUnit toDelete = findUnit(unit);
                try
                {
                    var orders = ordersOfUnit(unit).ToList();//all orders with unit as their unit key into list
                    if (orders.Count == 0)//no orders for that unit
                        myDAL.deleteUnit(toDelete);
                    else
                    {
                        var requests = from ord in orders
                                       let request = myDAL.findGuest(ord.GuestRequestKey)//sets request to the guestrequest that goes with it
                                       where (DateTime.Today < request.ReleaseDate)//date of end of vacation is after today
                                       select ord;
                        if (requests == null)
                            myDAL.deleteUnit(toDelete);//no orders after today
                        else
                            throw new InvalidException("cannot delete. vacations are booked for the future");
                    }
                }
                catch(unfoundRequestExceptionBL ex)
                {
                    myDAL.deleteUnit(toDelete);//no orders after today. deletes

                }
            }
            catch(Exception ex)
            { 
                throw new InvalidException(ex.Message);
            }
            
           //if orders exist with that hostingunitkey
            //find their guest requests based on guest request number and check if the order already passed
            //if there are orders in unit throw
        }
        #endregion
       
        #region unit checks pl
        


        

        #endregion
        #region guest and host checks for pl
     public void addHostNum(string text, out Int32 h1)//adds host number to host
        {
            if (Int32.TryParse(text, out h1))
            {
                if (h1 < 0)
                    throw new InvalidException("invalid host num");
            }
            else
                throw new InvalidException("invalid host num");
        }
        #endregion
        #region mail
        public System.Net.Mail.MailAddress checkMail(string text)//cheks if text is email address and returns it if it is
        {
            try
            {

                if (!Regex.IsMatch(text, @"@([\w\-]+)((\.(\w){2,})+)$"))//words@
                     throw new invalidFormatBL();//not mailformat

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
        #region LINQ and grouping

            public IEnumerable<IGrouping<Enums.Area, HostingUnit>> groupUnitsByArea()
            {
            var units = myDAL.getAllHostingUnits();
            var groupArea = from unit in units
                            group unit by unit.AreaVacation into newGroup
                            select newGroup;
            return groupArea;
          }
        public IEnumerable<IGrouping<Enums.Area, GuestRequest>> groupRequestsByArea()
        {
            var guests=myDAL.getRequests();
            var groupArea = from GuestRequest in guests
                            group GuestRequest by GuestRequest.AreaVacation into newGroup
                            select newGroup;
            return groupArea;
        }
        public IEnumerable<IGrouping<Enums.MealType, GuestRequest>> groupRequestsByMeal()
        {
            var guests = myDAL.getRequests();
            var mealGroup= from GuestRequest in guests
                            group GuestRequest by GuestRequest.Meal into newGroup
                            select newGroup;
            return mealGroup;
        }

        public IEnumerable<Order> ordersOfUnit(HostingUnit hu)
        {
            return ordersOfUnit(hu.HostingUnitKey);
        }
        public IEnumerable<Order>ordersOfUnit(int unitNum)
        {

            var allOrders = myDAL.getAllOrders();
            if (allOrders == null || allOrders.Count == 0)
                throw new unfoundRequestExceptionBL("no orders found");//nothing there
            var thisUnit = from order in allOrders
                           let orderKey = order.HostingUnitKey//saves unit key for easier access
                           where orderKey == unitNum
                           select order;
            return thisUnit;
        }


   
        public IEnumerable<IGrouping<Host, HostingUnit>> groupHostsByUnits()
        {
            var units = myDAL.getAllHostingUnits();
            return from HostingUnit in units
                   group HostingUnit by HostingUnit.Host into newGroup
                   select newGroup;
        }



        #endregion

        #region banks
        public IEnumerable<IGrouping<int, BankAccount>> groupBranchesByBank()
        {
            var bank = myDAL.getAllBranches();
            return from branches in bank
             group branches by branches.BankNumber into allBanks
             select allBanks;
        }
        #endregion
    }
}
