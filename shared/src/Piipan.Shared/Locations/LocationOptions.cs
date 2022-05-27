namespace Piipan.Shared.Locations
{
    public class LocationOptions
    {
        public const string SectionName = "Locations";
        public LocationMapping[] Map { get; set; }
    }
    public class LocationMapping
    {
        public string Name { get; set; }
        public string[] States { get; set; }
    }
}
