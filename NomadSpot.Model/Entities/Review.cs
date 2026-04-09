using System;

namespace NomadSpot.Model.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string Author { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public int NoiseLevel { get; set; }
        public int WifiStrength { get; set; }
        public int ComfortLevel { get; set; }
        public int PriceLevel { get; set; }
        public int Cleanliness { get; set; }
        public int Crowdedness { get; set; }
        public DateTime Date { get; set; }
    }
}
