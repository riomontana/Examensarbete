using System;
using System.ComponentModel.DataAnnotations;

namespace SensorGateway.Models
{
    public class DateAndInterval : BaseClass
    {
        private int interval;
        private DateTime fromDate;
        private DateTime toDate;

        public DateAndInterval()
        { }

        public DateAndInterval(int interval, DateTime fromDate, DateTime toDate)
        {
            Interval = interval;
            FromDate = fromDate;
            ToDate = toDate;
        }

        [Key]
        public int Id { get; set; }

        public int Interval
        {
            set { SetProperty(ref interval, value); }
            get { return interval; }
        }

        public DateTime FromDate
        {
            set { SetProperty(ref fromDate, value); }
            get { return fromDate; }
        }

        public DateTime ToDate 
        {
            set { SetProperty(ref toDate, value); }
            get { return toDate; }
        }

        public override string ToString()
        {
            if(interval == 0)
                return "No interval or dates set";

            return "Interval: " + interval + " minutes" + "\nFrom: " 
                + fromDate.Date.ToString("dd/MM/yyyy") + "\nTo: " + toDate.Date.ToString("dd/MM/yyyy");
        }

    }
}
