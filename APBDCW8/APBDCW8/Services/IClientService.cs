using APBDCW8.Models;

namespace APBDCW8.Services;

public interface IClientService
{
    public Task<int> addClient(Client client);
    public Task<List<ClientTrip>> GetClientTrips(int id);
    
    public Task<string> RegisterTrip(int IdClient, int IdTrip);
    
    public Task<string> DeleteClientRegistration(int IdClient, int IdTrip);
}