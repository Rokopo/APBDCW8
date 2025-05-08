using APBDCW8.Models;
using APBDCW8.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBDCW8.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        
        private readonly IClientService _clientService;
        

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }
        
        /// Zwraca wszystkie wycieczki powiązane z konkretnym klientem.
         
        [Route("/{IdClient}/trips")]
        [HttpGet("{IdClient}/trips")]
        public async Task<IActionResult> GetTrips(int IdClient)
        {
            var trips =await _clientService.GetClientTrips(IdClient);

            if (trips == null)
            {
                return NotFound("Klient badz wycieczki nie znalezione");
            }
            return Ok(trips);
        }

        // Endpoint do dodawania nowego klienta
        [HttpPost]
        public async Task<IActionResult> AddClient([FromBody] Client client)
        {
            var c = await _clientService.addClient(client);

            if (c > 0)
            {
                return Created(string.Empty, c);
            }
            return BadRequest("Wpisz wszsytkie potrzebne wartosci do utworzenia Klienta");
            
            
        }
        
        
        [Route("/{IdClient}/trips/{IdTrip}")]
        [HttpPut("{IdClient}/trips/{IdTrip}")]
        public async Task<IActionResult> UpdateClientTrip(int IdClient, int IdTrip)
        {
            var res = await _clientService.RegisterTrip(IdClient, IdTrip);

            switch (res)
            {
                case "Klient nie istnieje":
                    return NotFound("Podany klient nie istnieje");
                case "Podana Wycieczka nie istnieje":
                    return NotFound("Podana Wycieczka nie istnieje");
                case "Max za male":
                    return Conflict("Max ilosc osob osiagniete");
                case "Sukces":
                    return Created(string.Empty,"Sukces");
                case "juz jest":
                    return Conflict("Taki rekord juz jest");
                default:
                    return StatusCode(500, new { message = "Wystapil nieoczekiwany blad"});
                
            }
            
        }

        [Route("/{IdClient}/trips/{IdTrip}")]
        [HttpDelete("{IdClient}/trips/{IdTrip}")]
        public async Task<IActionResult> DeleteClientTrip(int IdClient, int IdTrip)
        {
            var res = await _clientService.DeleteClientRegistration(IdClient, IdTrip);

            switch (res)
            {
                case "nie istnieje":
                    return NotFound("Podana rejestracja nie wykryta");
                case "Sukces":
                    return Ok("Rejestracja usunieta pomyslnie");
                default:
                    return StatusCode(500, "Wystapil nieoczekiwany blad");
            }
        }
    }
}
