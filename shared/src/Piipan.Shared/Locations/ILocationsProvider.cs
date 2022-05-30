namespace Piipan.Shared.Locations
{
    public interface ILocationsProvider
    {
        string[] GetStates(string location);
    }
}
