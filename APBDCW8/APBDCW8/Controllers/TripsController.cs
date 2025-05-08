using APBDCW8.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBDCW8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }
        
        /// Zwraca wszystkie dostępne wycieczki wraz z podstawowymi informacjami i listą krajów.
        /// <response code="200">Zwraca listę wycieczek

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var trips = await _tripService.GetTrips();
            return Ok(trips);
        }
        
        
    }
}

