using System.ComponentModel.DataAnnotations.Schema;

namespace SensorGateway.Models
{
    public class Sensor : BaseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        private string name;
        private string uuid;
        private byte[] sensorData;
        private bool isActive;
        private DateAndInterval dateAndInterval;

        public Sensor()
        { }

        public Sensor(string name)
        {
            Name = name;
        }

        public Sensor(string name, string uuid, bool isActive, DateAndInterval dateAndInterval, byte[] sensorData)
        {
            Name = name;
            Uuid = uuid;
            SensorData = sensorData;
            IsActive = isActive;
            DateAndInterval = dateAndInterval;
        }

        public int Id { set; get; }

        [ForeignKey("DateAndInterval")]
        public int DateAndIntervalId { set; get; }

        public string Name
        {
            set { SetProperty(ref name, value); }
            get { return name; }
        }

        public string Uuid 
        {
            set { SetProperty(ref uuid, value); }
            get { return uuid; }
        }

        [NotMapped]
        public byte[] SensorData
        {
            set { SetProperty(ref sensorData, value); }
            get { return sensorData; }
        }

        public bool IsActive 
        {
            set { SetProperty(ref isActive, value); }
            get { return isActive; }
        }

        public DateAndInterval DateAndInterval
        {
            set { SetProperty(ref dateAndInterval, value); }
            get { return dateAndInterval; }
        }

        public string StrDateAndInterval
        {
            get 
            { 
                if (dateAndInterval != null)
                { 
                    return dateAndInterval.ToString(); 
                }
                return "";
            }
        }
    }
}