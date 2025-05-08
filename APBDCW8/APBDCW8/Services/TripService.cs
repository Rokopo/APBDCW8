using APBDCW8.Models;
using Microsoft.Data.SqlClient;

namespace APBDCW8.Services;

public class TripService : ITripService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Database=TravelAgencyDb;Trusted_Connection=True;";
    
    /// Pobiera wszystkie dostępne wycieczki wraz z listą krajów.
    public async Task<List<Trip>> GetTrips()
    {
        var trips = new Dictionary<int, Trip>();
    
        await using var con = new SqlConnection(_connectionString);
        await using var com = new SqlCommand();
    
        com.Connection = con;
        // Zapytanie SQL pobierające wycieczki i odpowiadające im kraje
        com.CommandText = @"SELECT t.IdTrip, t.Name AS TripName, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName
        FROM Trip t 
        JOIN Country_Trip CT ON t.IdTrip = CT.IdTrip 
        JOIN Country C ON CT.IdCountry = C.IdCountry";
    
        await con.OpenAsync();
    
        SqlDataReader reader = await com.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int idTrip = (int)reader["idTrip"];
        
            // Jeśli już mamy tę wycieczkę, dodajemy tylko nowy kraj
            if (trips.ContainsKey(idTrip))
            {
                string countryName = reader["CountryName"].ToString();
                if (!trips[idTrip].Countries.Contains(countryName))
                {
                    trips[idTrip].Countries.Add(countryName);
                }
            }
            else
            {
                // Tworzymy nową wycieczkę
                string countryName = reader["CountryName"].ToString();
            
                trips.Add(idTrip, new Trip()
                {
                    IdTrip = idTrip,
                    Name = reader["TripName"].ToString(),
                    Description = reader["Description"].ToString(),
                    DateFrom = (DateTime)reader["DateFrom"],
                    DateTo = (DateTime)reader["DateTo"],
                    MaxPeople = (int)reader["MaxPeople"],
                    Countries = new List<string> { countryName }
                });
            }
        }
    
        return trips.Values.ToList();
    }
}