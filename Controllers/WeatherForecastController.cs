using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet( "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> GetWeather()
    {
        Console.Write("Checking Checking");
        return Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

[HttpGet( "GetChecking")]
public IActionResult GetChecking()
    {
        int num1 = 7;
        String string1 = "checking";
        
        return Ok(num1.ToString() + " " + string1);
    }

}


