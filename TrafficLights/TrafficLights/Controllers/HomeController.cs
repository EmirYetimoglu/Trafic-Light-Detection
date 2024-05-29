using TrafficLights.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace TrafficLights.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHost;

        public HomeController(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            string uploadFolder = Path.Combine(_webHost.WebRootPath, "uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string fileName = Path.GetFileName(file.FileName);
            string fileSavePath = Path.Combine(uploadFolder, fileName);

            using (FileStream stream = new FileStream(fileSavePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            byte[] imageArray = System.IO.File.ReadAllBytes(fileSavePath);
            string encoded = Convert.ToBase64String(imageArray);
            byte[] data = Encoding.ASCII.GetBytes(encoded);
            string api_key = "0eJL7lujx73N0Kc5WyPd"; // Your API Key
            string model_endpoint = "redlighttraffic/1"; // Set model endpoint

            // Construct the URL
            string uploadURL =
                    "https://detect.roboflow.com/" + model_endpoint + "?api_key=" + api_key;

            // Service Request Config
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Configure Request
            WebRequest request = WebRequest.Create(uploadURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            // Write Data
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            // Get Response
            string responseContent = null;
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr99 = new StreamReader(stream))
                    {
                        responseContent = sr99.ReadToEnd();
                    }
                }
            }

            // Parse the JSON response
            var JSONresponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

            ViewBag.carCount = JSONresponse.Predictions.Count;

            // Load the uploaded image
            using (var image = System.Drawing.Image.FromFile(fileSavePath))
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Draw bounding boxes
                foreach (var prediction in JSONresponse.Predictions)
                {
                    float x1 = prediction.X - (prediction.Width / 2);
                    float y1 = prediction.Y - (prediction.Height / 2);
                    float x2 = prediction.X + (prediction.Width / 2);
                    float y2 = prediction.Y + (prediction.Height / 2);

                    graphics.DrawRectangle(Pens.Red, x1, y1, prediction.Width, prediction.Height);
                }

                // Save the modified image
                string processedFileName = "processed_" + fileName;
                string processedFilePath = Path.Combine(uploadFolder, processedFileName);
                image.Save(processedFilePath, ImageFormat.Jpeg);

                ViewBag.ProcessedImage = processedFileName;
            }

            ViewBag.ImageName = fileName;
            ViewBag.Message = fileName + " uploaded and processed successfully";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}