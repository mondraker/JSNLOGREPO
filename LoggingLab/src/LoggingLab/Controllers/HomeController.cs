using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoggingLab.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Mi primer evento");

            _logger.LogError(new EventId(10, "miError"), new Exception("Esto es una excepción"), "Mi primer error");

            _logger.LogWarning("Esto es un warning con el objeto punto {@punto} serializado, con @", new { X = 10, y = 13 });
            _logger.LogWarning("Esto es un warning con el objeto punto {$punto} serializado, con $", new { X = 10, y = 13 });
            _logger.LogWarning("Esto es un warning con el objeto punto {punto} serializado, sin nada", new { X = 10, y = 13 });

            using (_logger.BeginScope("CTX"))
            { 
                _logger.LogInformation("Evento en un contexto 'CTX', que no queda registrado, a través de la interfaz de .Net");
            }
            //Para obtener el contexto hay que prescindir de la interfaz de .Net y consumir directamente Serilog:
            Serilog.Log.ForContext("SourceContext", "Contexto1").Warning("evento warning disparado bajo un contexto determinado, invocando directamente a Serilog");

            Serilog.Log.ForContext("UnaPropiedadPuntual", new { Atributo1 = "valor1", Atributo2 = "valor2" }, true).Warning("evento warning disparado con una propiedad exclusiva de este evento, invocando directamente a Serilog");

            _logger.LogWarning(new EventId(150, "HITO_EVENTO_NEGOCIO_150"), "los eventos de negocio van codificados por el idevent");

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
