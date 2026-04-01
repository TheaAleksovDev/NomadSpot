using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Entities
{
    public abstract class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Rating { get; set; }
        public int NoiseLevel { get; set; }
        public bool HasWifi { get; set; }
        public bool HasPowerOutlets { get; set; }
        public DateTime LastVerified { get; set; }
        public bool IsActive { get; set; }
        public string LocationType { get; set; }
        public List<Review> Reviews { get; set; } = new();
    }
}
