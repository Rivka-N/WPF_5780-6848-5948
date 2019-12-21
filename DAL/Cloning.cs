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
            GuestRequest target = new GuestRequest(original.GuestRequestKey, original.Registration);
            target.Name = original.Name;
            target.LastName = original.LastName;
            target.Mail= original.Mail;
            target.EntryDate = original.EntryDate;
            target.ReleaseDate = original.ReleaseDate;
            target.AreaVacation = original.AreaVacation;
            target.SubArea = original.SubArea;
            target.TypeOfUnit = original.TypeOfUnit;
            target.NumAdult = original.NumAdult;
            target.NumChildren = original.NumChildren;
            target.Pool = original.Pool;
            target.Jacuzzi = original.Jacuzzi;
            target.Garden = original.Garden;
            target.ChildrenAttractions = original.ChildrenAttractions;
            target.NumSuggestions = original.NumSuggestions;
            target.Status = original.Status;
            return target;
        }
    }
}
