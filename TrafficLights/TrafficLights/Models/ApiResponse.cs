using TrafficLights.Controllers;
namespace TrafficLights.Models
{
    public class ApiResponse
    {
        public List<Prediction> Predictions { get; set; }
        public ImageSize Image { get; set; }
    }
}
