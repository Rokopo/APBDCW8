using APBDCW8.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace APBDCW8.Services;

public class ClientService : IClientService
{
    
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Database=TravelAgencyDb;Trusted_Connection=True;";
    
    /// Dodaje nowego klienta do bazy danych.
    public async Task<int> addClient(Client client)
    {

        if (string.IsNullOrWhiteSpace(client.FirstName) || string.IsNullOrWhiteSpace(client.LastName) || string.IsNullOrWhiteSpace(client.Email) 
            || string.IsNullOrWhiteSpace(client.Pesel) || string.IsNullOrWhiteSpace(client.Telephone))
        {
            return 0;
        }
        using (SqlConnection conn = new SqlConnection(_connectionString))

        {
            //// Zapytanie SQL dodające klienta i zwracające jego ID
            string Query = @"INSERT INTO CLIENT (FirstName,LastName,Email,Telephone,Pesel) VALUES
                (@FirstName,@LastName,@Email,@Telephone,@Pesel); SELECT SCOPE_IDENTITY();";
            await using SqlCommand cmd = new SqlCommand(Query, conn);
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
            
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            if (result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            return 0;
        }
    }

    ///// Pobiera wszystkie wycieczki powiązane z danym klientem.
    public async Task<List<ClientTrip>> GetClientTrips(int IdClient)
    {
        var clientr_trip = new List<ClientTrip>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            // Sprawdzenie czy klient istnieje
            string checkClient = "SELECT COUNT(*) FROM CLIENT WHERE IdClient = @IdClient";
            await using SqlCommand cmd = new SqlCommand(checkClient, conn);
            cmd.Parameters.AddWithValue("@IdClient", IdClient);
            
            var result = (int) await cmd.ExecuteScalarAsync();

            if (result == 0)
            {
                return null;
            }
            
            // Sprawdzenie czy klient ma przypisane wycieczki
            string checkTrips = "SELECT COUNT(IdTrip) FROM Client_Trip WHERE IdClient = @IdClient";
            await using SqlCommand cmd1 = new SqlCommand(checkTrips, conn);
            cmd1.Parameters.AddWithValue("@IdClient", IdClient);
            result = (int) await cmd1.ExecuteScalarAsync();
            if (result == 0)
            {
                return null;
            }
            
            // Zapytanie SQL pobierające dane o wycieczkach klienta
            string Query =
                @"Select c.IdClient, t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,STRING_AGG(cc.Name, ', ') AS Countries ,c.RegisteredAt, c.PaymentDate
            FROM Trip t JOIN Client_Trip c on t.IdTrip = c.IdTrip Join Country_Trip CT on t.IdTrip = CT.IdTrip JOIN Country cc ON CT.IdCountry = cc.IdCountry WHERE 
                         c.IdClient = @IdClient GROUP BY c.IdClient, t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.RegisteredAt, c.PaymentDate";
            await using SqlCommand queryCommand = new SqlCommand(Query, conn);
            queryCommand.Parameters.AddWithValue("@IdClient", IdClient);
            
            SqlDataReader reader = await queryCommand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int klientId = (int)reader["IdClient"];
                int IdTrip = (int)reader["IdTrip"];
                var RegistrationId = reader["RegisteredAt"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RegisteredAt"]);
                var PaymentDate = reader["PaymentDate"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PaymentDate"]);

                var trip = new Trip()
                {
                    IdTrip = (int)reader["IdTrip"],
                    Name = (string)reader["Name"],
                    Description = (string)reader["Description"],
                    DateFrom = (DateTime)reader["DateFrom"],
                    DateTo = (DateTime)reader["DateTo"],
                    MaxPeople = (int)reader["MaxPeople"],
                    Countries = reader["Countries"].ToString().Split(',').ToList()
                };

                clientr_trip.Add(new ClientTrip()
                {
                    idClient = klientId,
                    IdTrip = IdTrip,
                    RegisteredAt = RegistrationId,
                    PaymentDate = PaymentDate,
                    Trips = new List<Trip> {trip}
                });
            }

            return clientr_trip;
        }
    }

    /// Rejestruje klienta na wycieczkę.
    public async Task<string> RegisterTrip(int IdClient, int IdTrip)
    {
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            await con.OpenAsync();

            // Sprawdzenie istnienia klienta
            string check = "SELECT COUNT(*) FROM CLIENT WHERE IdClient = @IdClient";
            await using SqlCommand com1 = new SqlCommand(check, con);
            com1.Parameters.AddWithValue("@IdClient", IdClient);
            
            int res = (int) await com1.ExecuteScalarAsync();

            if (res == 0)
            {
                return "Klient nie istnieje";
            }
            
            // Sprawdzenie istnienia wycieczki
            string check1 = "SELECT COUNT(*) FROM TRIP WHERE IdTrip = @IdTrip";
            SqlCommand com2 = new SqlCommand(check1, con);
            com2.Parameters.AddWithValue("@IdTrip", IdTrip);
            
            int res2 = (int)await com2.ExecuteScalarAsync();
            if (res2 == 0)
            {
                return "Podana Wycieczka nie istnieje";
            }
            
            // Sprawdzenie czy klient już jest zapisany
            string check3 = "SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
            SqlCommand com4 = new SqlCommand(check3, con);
            com4.Parameters.AddWithValue("@IdClient", IdClient);
            com4.Parameters.AddWithValue("@IdTrip", IdTrip);
            
            int res3 = (int)await com4.ExecuteScalarAsync();
            if (res3 == 1)
            {
                return "juz jest";
            }
            
            // Sprawdzenie limitu miejsc na wycieczce
            string HowManyPeopleTrip = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
            SqlCommand getHowManyPeopleTrip = new SqlCommand(HowManyPeopleTrip, con);
            getHowManyPeopleTrip.Parameters.AddWithValue("@IdTrip", IdTrip);
            int ResCurrentTripPeople = (int)await getHowManyPeopleTrip.ExecuteScalarAsync();
            
            string MaxPeopleTrip = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
            SqlCommand getMaxPeopleTrip = new SqlCommand(MaxPeopleTrip, con);
            getMaxPeopleTrip.Parameters.AddWithValue("@IdTrip", IdTrip);
            int ResMaxPeople = (int)await getMaxPeopleTrip.ExecuteScalarAsync();

            if (ResCurrentTripPeople > ResMaxPeople)
            {
                return $"Max za male";
            }
            
            string Query = @"
                INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                VALUES (@IdClient, @IdTrip, CONVERT(INT, CONVERT(VARCHAR, GETDATE(), 112)));
            ";
            SqlCommand com3 = new SqlCommand(Query, con);
            com3.Parameters.AddWithValue("@IdClient", IdClient);
            com3.Parameters.AddWithValue("@IdTrip", IdTrip);
            int QueryRes = await com3.ExecuteNonQueryAsync(); //zwraca liczbe zmienionych wierszy?

            if (QueryRes > 0)
            {
                return "Sukces";
            }
            
            return "Error";
            
        }

    }

    /// Usuwa rejestrację klienta z wycieczki.
    public async Task<string> DeleteClientRegistration(int IdClient, int IdTrip)
    {
        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            await con.OpenAsync();
            // Sprawdzenie, czy rejestracja istnieje
            string check = "SELECT COUNT(*) FROM CLIENT_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
            await using SqlCommand com1 = new SqlCommand(check, con);
            com1.Parameters.AddWithValue("@IdClient", IdClient);
            com1.Parameters.AddWithValue("@IdTrip", IdTrip);
            var res = (int) await com1.ExecuteScalarAsync();
            if (res == 0)
            {
                return "nie istnieje";
            }

            // Usunięcie rekordu z tabeli Client_Trip
            string Query = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
            SqlCommand com2 = new SqlCommand(Query, con);
            com2.Parameters.AddWithValue("@IdClient", IdClient);
            com2.Parameters.AddWithValue("@IdTrip", IdTrip);
            int QueryRes = await com2.ExecuteNonQueryAsync();

            if (QueryRes > 0)
            {
                return "Sukces";
            }
            
            return "Error";
        }
    }
}