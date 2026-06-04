namespace Backend.Data.Entitites
{
    public class Turbine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public Turbine(int id, string name, float latitude, float longitude)
        {
            Id = id;
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
