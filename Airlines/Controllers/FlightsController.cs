using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airlines.DataAccess;
using Airlines.Models;

namespace Airlines.Controllers
{
    [Route("api/[controller]")]
    [ApiController]



    public class FlightsController : ControllerBase
    {
        private readonly AppdbContext _context;

        public FlightsController(AppdbContext context)
        {
            _context = context;
        }

        // GET: api/Flights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlight()
        {
            return await _context.Flight.ToListAsync();
        }


        public class FlightRoute
        {
            public List<Flight>? Flight { get; set; }
            public decimal TotalPrice { get; set; }
        }

        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchFlights([FromQuery] string origin, [FromQuery] string destination)
        {
            // Verificar si el origen existe
            var validOrigin = await _context.Flight.AnyAsync(f => f.Origin == origin);
            if (!validOrigin)
            {
                return NotFound("El origen especificado no existe.");
            }

            // Buscar vuelos directos
            var directFlights = await _context.Flight
                .Where(f => f.Origin == origin && f.Destination == destination)
                .Select(f => new
                {
                    Origin = f.Origin,
                    Destination = f.Destination,
                    Price = f.Price,
                    Transport = new
                    {
                        FlightCarrier = f.TransportFlightCarrier,
                        FlightNumber = f.FlightNumber
                    }
                }).ToListAsync();

            // Buscar vuelos con escalas
            var intermediateStops = await _context.Flight
                .Where(f => f.Origin == origin)
                .Join(_context.Flight.Where(f => f.Destination == destination),
                      flight1 => flight1.Destination,
                      flight2 => flight2.Origin,
                      (flight1, flight2) => new
                      {
                          Flight1 = flight1,
                          Flight2 = flight2
                      })
                .Select(pair => new
                {
                    Origin = origin,
                    Destination = destination,
                    Intermediate = new[] {
                new {
                    Origin = pair.Flight1.Origin,
                    Destination = pair.Flight1.Destination,
                    Price = pair.Flight1.Price,
                    Transport = new
                    {
                        FlightCarrier = pair.Flight1.TransportFlightCarrier,
                        FlightNumber = pair.Flight1.FlightNumber
                    }
                },
                new {
                    Origin = pair.Flight2.Origin,
                    Destination = pair.Flight2.Destination,
                    Price = pair.Flight2.Price,
                    Transport = new
                    {
                        FlightCarrier = pair.Flight2.TransportFlightCarrier,
                        FlightNumber = pair.Flight2.FlightNumber
                    }
                }
                    },
                    TotalPrice = Convert.ToDecimal(pair.Flight1.Price) + Convert.ToDecimal(pair.Flight2.Price)
                }).ToListAsync();

            if (intermediateStops.Count > 0)
            {
                // Construir información de la ruta con escalas
                var journeyInfo = new
                {
                    Journey = new
                    {
                        Origin = origin,
                        Destination = destination,
                        Price = intermediateStops.Min(f => f.TotalPrice),
                        Flights = intermediateStops.SelectMany(f => f.Intermediate).ToList<object>()
                    }
                };

                return Ok(journeyInfo);
            }
            else if (directFlights.Any())
            {
                // Si no hay vuelos con escalas pero hay vuelos directos,
                // agregamos la información de los vuelos directos a la respuesta
                var journeyInfo = new
                {
                    Journey = new
                    {
                        Origin = origin,
                        Destination = destination,
                        Price = directFlights.Sum(flight => Convert.ToDecimal(flight.Price)),
                        Flights = directFlights.Select(flight => new
                        {
                            Origin = flight.Origin,
                            Destination = flight.Destination,
                            Price = flight.Price,
                            Transport = flight.Transport
                        }).ToList<object>()
                    }
                };

                return Ok(journeyInfo);
            }
            else
            {
                return NotFound("No se encontraron vuelos para la ruta especificada.");
            }
        }



        private bool FlightExists(int id)
        {
            return _context.Flight.Any(e => e.ID == id);
        }
    }
}
