using APBDCW8.Models;

namespace APBDCW8.Services;

public interface ITripService
{
    public Task<List<Trip>> GetTrips();
}