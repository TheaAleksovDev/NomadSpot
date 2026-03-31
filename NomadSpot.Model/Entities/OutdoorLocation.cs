using System;
using System.Collections.Generic;
using System.Text;

namespace NomadSpot.Model.Entities
{
    public class OutdoorLocation : Location
    {
        public bool HasBenches { get; set; }
        public bool HasShade { get; set; }
        public bool PetFriendly { get; set; }
        public bool HasPublicToilet { get; set; }
        public bool NearShops { get; set; }
    }
}
