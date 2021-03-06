﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace DAL
{
    public static class Cloning
    {
        public static GuestRequest Clone(this GuestRequest original)
        {
            if (original == null)
                return null;
            GuestRequest target = new GuestRequest(original.GuestRequestKey, original.Registration);
            target.Name = original.Name;
            target.GuestRequestKey = original.GuestRequestKey;
            target.LastName = original.LastName;
            target.Mail= original.Mail;
            target.EntryDate = original.EntryDate;
            target.ReleaseDate = original.ReleaseDate;
            target.AreaVacation = original.AreaVacation;
            target.TypeOfUnit = original.TypeOfUnit;
            target.NumAdult = original.NumAdult;
            target.NumChildren = original.NumChildren;
            target.Pool = original.Pool;
            target.Jacuzzi = original.Jacuzzi;
            target.Garden = original.Garden;
            target.Meal = original.Meal;
            target.Status = original.Status;
            return target;
        }

        public static HostingUnit Clone(this HostingUnit original)
        {   if (original == null)
                return null;
            HostingUnit target = new HostingUnit();
            target.HostingUnitName = original.HostingUnitName;
            target.HostingUnitType = original.HostingUnitType;
            target.HostingUnitKey = original.HostingUnitKey;
            target.AreaVacation = original.AreaVacation;
            target.Meal = original.Meal;
            target.Diary = (bool[,])(original.Diary.Clone());
            target.Garden = original.Garden;
            target.Host = original.Host.Clone();
            target.HostingUnitKey = original.HostingUnitKey;
            target.NumAdult = original.NumAdult;
            target.NumChildren = original.NumChildren;
            target.Jacuzzi = original.Jacuzzi;
            target.Pool = original.Pool;
            target.MoneyPaid = original.MoneyPaid;
           

            return target;
        }

        public static Host Clone(this Host original)
        {
            if (original == null)
                return null;
            Host target = new Host();
            target.CollectionClearance = original.CollectionClearance;
            target.HostKey = original.HostKey;
            target.LastName = original.LastName;
            target.Name = original.Name;
            target.Mail = original.Mail;
            if (original.Bank == null)
                target.Bank = new BankAccount();
            else
                target.Bank = original.Bank.Clone();
            return target;
            
        }

        public static BankAccount Clone(this BankAccount original)
        {
            if (original == null)
                return null;
            BankAccount target = new BankAccount();
            target.BankAcountNumber = original.BankAcountNumber;
            target.BankName = original.BankName;
            target.BankNumber = original.BankNumber;
            target.BranchAddress = original.BranchAddress;
            target.BranchCity = original.BranchCity;
            target.BranchNumber = original.BranchNumber;
            return target;
        }

        public static Order Clone(this Order original)
        {
            if (original == null)
                return null;
            Order target = new Order(original.CreateDate, original.OrderDate);
            target.HostingUnitKey = original.HostingUnitKey;
            target.GuestRequestKey = original.GuestRequestKey;
            target.OrderKey = original.OrderKey;
            target.CreateDate = original.CreateDate;
            target.OrderDate = original.OrderDate;
            target.Status = original.Status;
            if(original.HostName!=null)
                target.HostName = original.HostName;
            if (original.GuestName!=null)
            target.GuestName = original.GuestName;
            return target;
        }
    }
}
