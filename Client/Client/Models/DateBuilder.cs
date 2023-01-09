using System;

namespace Client.Models
{
    public class DateBuilder
    {
        private string Year = "";
        private string Month = "";
        private string Day = "";

        public bool ValidateDateTime(DateTime dt)
        {
            if ((dt.Year > DateTime.Now.Year - 5) && (dt <= DateTime.Now))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FormateDate(DateTime dt)
        {
            Year = dt.Year.ToString(); 
            Month = dt.Month.ToString();
            Day = dt.Day.ToString();

            if (dt.Year > 9999)
            {
                Year = "9999";
            }
            if (dt.Year < 1000)
            {
                Year = "1000";
            }
            if (dt.Month <= 9)
            {
                Month = "0" + dt.Month;
            }
            if (dt.Day <= 9)
            {
                Day = "0" + dt.Day;
            }
            return;
        }
    
        public string GetFormattedYear()
        {
            return Year;
        }
        public string GetFormattedMonth()
        {
            return Month;
        }
        public string GetFormattedDay()
        {
            return Day;
        }
    }  
}
