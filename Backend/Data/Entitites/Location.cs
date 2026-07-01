namespace Backend.Data.Entitites
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public bool IsHarbour { get; set; }
        public Location() { }
        public Location(string name, float latitude, float longitude, bool isHarbour)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            IsHarbour = isHarbour;
        }
    }
}
