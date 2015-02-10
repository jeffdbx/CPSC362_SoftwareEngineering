// CPSC362 - Software Engineering
// Property Management application
// Author: Jeff Bohlin
// Date: 03-01-2013
//
// ValidateData.cs  - This class is used to validate user input data  
// in many of the the application's text boxes.
//
// Reference: http://miteshsureja.blogspot.com/2011/08/validate-data-using-idataerrorinfo-in.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace PropertyManagement
{
    public class ValidateData : IDataErrorInfo
    {
        public string Name    { get; set; }
        public string Address { get; set; }
        public string City    { get; set; }
        public string State   { get; set; }
        public string Units   { get; set; }
        public string Zip     { get; set; }

        public string UnitNumber   { get; set; }
        public string FloorNumber  { get; set; }
        public string SquareFt     { get; set; }
        public string BathQuantity { get; set; }
        public string BedQuantity  { get; set; }
        public string MonthlyRent  { get; set; }
        public string Deposit      { get; set; }

        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Phone     { get; set; }
        public string AmountDue { get; set; }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string name]
        {
            get
            {
                string result = null;

                //-------------------------------------------------------------------
                //--------------------------- Property Data -------------------------
                //-------------------------------------------------------------------
                if (name == "Name")
                {
                    if (string.IsNullOrEmpty(Name) || Name.Length > 255)
                        result = "Please enter a Name.";
                }
                if (name == "Address")
                {
                    if (string.IsNullOrEmpty(Address) || Address.Length > 255)
                        result = "Please enter an Address.";
                }
                if (name == "City")
                {
                    if (string.IsNullOrEmpty(City) || City.Length > 50)
                        result = "Please enter a City.";
                }
                if (name == "State")
                {
                    if (string.IsNullOrEmpty(State) || State.Length > 20)
                        result = "Please enter a State.";
                }
                if (name == "Zip")
                {
                    int i = 0;
                    bool isZipANumber = int.TryParse(Zip, out i);

                    if (string.IsNullOrEmpty(Zip) || Zip.Length > 5 || !isZipANumber || (isZipANumber && (i < 0 || i > 99999)))
                    {
                        result = "Please enter a valid number for the zip code.";
                    }
                }
                if (name == "Units")
                {
                    int i = 0;
                    bool isUnitsANumber = int.TryParse(Units, out i);
                    if (string.IsNullOrEmpty(Units) || Units.Length > 3 || !isUnitsANumber || (isUnitsANumber && (i < 0 || i > 999)))
                    {
                        result = "Please enter a valid number for Total Units.";
                    }
                }
                if (name == "MonthlyRent")
                {
                    // Nasty regex for validating a dollar amount.
                    string pattern = @"^\?([1-9]{1}[0-9]{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\(\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))\)$";
                    if (!string.IsNullOrEmpty(MonthlyRent) && !Regex.IsMatch(MonthlyRent, pattern))
                    {
                        result = "Please enter a valid Monthly Rent.";
                    }
                }
                if (name == "Deposit")
                {
                    string pattern = @"^\?([1-9]{1}[0-9]{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\(\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))\)$";

                    if (!string.IsNullOrEmpty(Deposit) && !Regex.IsMatch(Deposit, pattern))
                    {
                        result = "Please enter a valid Deposit Amount.";
                    }
                }

                //-------------------------------------------------------------------
                //---------------------------- Unit Data ----------------------------
                //-------------------------------------------------------------------
                if (name == "UnitNumber")
                {
                    if (string.IsNullOrEmpty(UnitNumber))
                        result = "Please enter a Unit Number.";
                }
                if (name == "FloorNumber")
                {
                    int i = 0;
                    bool isFloorNumberANumber = int.TryParse(FloorNumber, out i);
                    if (string.IsNullOrEmpty(FloorNumber) || FloorNumber.Length > 3 || !isFloorNumberANumber || (isFloorNumberANumber && (i < 0 || i > 999)))
         
                        result = "Please enter a Floor Number.";
                }
                if (name == "SquareFt")
                {
                    int i = 0;
                    bool isSquareFtANumber = int.TryParse(SquareFt, out i);
                    if (string.IsNullOrEmpty(SquareFt) || SquareFt.Length > 4 || !isSquareFtANumber || (isSquareFtANumber && (i < 0 || i > 9999)))
                        result = "Please enter the Square Feet.";
                }
                if (name == "BathQuantity")
                {
                    int i = 0;
                    bool isBathQtyANumber = int.TryParse(BathQuantity, out i);
                    if (string.IsNullOrEmpty(BathQuantity) || BathQuantity.Length > 3 || !isBathQtyANumber || (isBathQtyANumber && (i < 0 || i > 999)))
                        result = "Please enter the Bath Quantity.";
                }
                if (name == "BedQuantity")
                {
                    int i = 0;
                    bool isBedQtyANumber = int.TryParse(BedQuantity, out i);
                    if (string.IsNullOrEmpty(BedQuantity) || BedQuantity.Length > 3 || !isBedQtyANumber || (isBedQtyANumber && (i < 0 || i > 999)))
                        result = "Please enter the Bed Quantity.";
                }
                if (name == "FirstName")
                {
                    if (string.IsNullOrEmpty(FirstName))
                        result = "Please enter a First Name.";
                }
                if (name == "LastName")
                {
                    if (string.IsNullOrEmpty(LastName))
                        result = "Please enter a Last Name.";
                }
                if (name == "Phone")
                {
                    if (string.IsNullOrEmpty(Phone))
                        result = "Please enter a Phone Number";
                }
                if (name == "AmountDue")
                {
                    string pattern = @"^\?([1-9]{1}[0-9]{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))$|^\(\$?([1-9]{1}\d{0,2}(\,\d{3})*(\.\d{0,2})?|[1-9]{1}\d{0,}(\.\d{0,2})?|0(\.\d{0,2})?|(\.\d{1,2}))\)$";

                    if (!string.IsNullOrEmpty(AmountDue) && !Regex.IsMatch(AmountDue, pattern))
                    {
                        result = "Please enter a valid Monthly Rent.";
                    }
                }
                return result;
            }
        }
    }
}
