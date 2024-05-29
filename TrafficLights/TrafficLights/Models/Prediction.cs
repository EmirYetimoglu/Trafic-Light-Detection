namespace TrafficLights.Models
{
    public class Prediction
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Class { get; set; }
        public float Confidence { get; set; }
    }
}
