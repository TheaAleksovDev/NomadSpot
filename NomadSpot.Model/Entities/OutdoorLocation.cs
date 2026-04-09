namespace NomadSpot.Model.Entities
{
    public class OutdoorLocation : Location
    {
        public bool HasBenches { get; set; }
        public bool HasShade { get; set; }
        public bool PetFriendly { get; set; }
        public bool HasPublicToilet { get; set; }
        public bool NearShops { get; set; }
        public OutdoorType OutdoorType { get; set; }
    }

    public enum OutdoorType
    {
        Park,
        Bench,
        SportsCourt,
        Beach,
        Garden,
        Plaza,
        Other
    }
}
