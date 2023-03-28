using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace WoodDealsParser
{
    internal class WoodDealsValidator
    {
        public bool IsValidDeal(Content deal, List<SqlParameter> parameters)
        {
            return
                IsValidName(deal.sellerName) &&
                IsValidInn(deal.sellerInn) &&
                IsValidName(deal.buyerName) &&
                IsValidInn(deal.sellerInn) &&
                IsValidDouble(deal.woodVolumeBuyer) &&
                IsValidDouble(deal.woodVolumeSeller) &&
                IsValidDate(deal.dealDate) &&
                IsValidNumeric(deal.dealNumber);
        }

        private bool IsValidInn(string inn)
        {
            if (inn.Length == 10)
            {
                int[] multipliers = { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
                int sum = 0;

                for (int i = 0; i < 9; i++)
                {
                    sum += int.Parse(inn[i].ToString()) * multipliers[i];
                }

                int control = sum % 11;
                control = (control == 10) ? 0 : control;

                return control == int.Parse(inn[9].ToString());
            }
            else if (inn.Length == 12)
            {
                int[] multipliers1 = { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
                int[] multipliers2 = { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

                int sum1 = 0;
                for (int i = 0; i < 10; i++)
                {
                    sum1 += int.Parse(inn[i].ToString()) * multipliers1[i];
                }

                int control1 = sum1 % 11;
                control1 = (control1 == 10) ? 0 : control1;

                if (control1 != int.Parse(inn[10].ToString()))
                {
                    return false;
                }

                int sum2 = 0;
                for (int i = 0; i < 11; i++)
                {
                    sum2 += int.Parse(inn[i].ToString()) * multipliers2[i];
                }

                int control2 = sum2 % 11;
                control2 = (control2 == 10) ? 0 : control2;

                return control2 == int.Parse(inn[11].ToString());
            }
            else
            {
                return false;
            }
        }

        private bool IsValidName(string inputName)
        {
            return !string.IsNullOrEmpty(inputName) && inputName.Length <= 255;
        }

        private bool IsValidDate(string dealDate)
        {
            if (Regex.IsMatch(dealDate, @"^(19|20)\d{2}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])$"))
            {
                return DateTime.Parse(dealDate) <= DateTime.Now;
            }
            else
            {
                return false;
            }
        }

        private bool IsValidDouble(string woodVolume)
        {
            return Regex.IsMatch(woodVolume, @"^\d+(\.\d+)?$");
        }

        private bool IsValidNumeric(string dealNumber)
        {
            return Regex.IsMatch(dealNumber, @"^[0-9]+$") && dealNumber.Length <= 50;
        }
    }
}
