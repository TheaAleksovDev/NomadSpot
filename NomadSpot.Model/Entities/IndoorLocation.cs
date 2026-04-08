using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Entities
{
    public class IndoorLocation : Location
    {
        public int ComfortLevel { get; set; }
        public int PriceLevel { get; set; }
        public string OpeningHours { get; set; }
        public IndoorType IndoorType { get; set; }
    }

    public enum IndoorType
    {
        Cafe,
        Library,
        CoworkingSpace,
        University,
        Other
    }
}
