using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using tx_api_consumer.Models;

namespace tx_api_consumer.Controllers {
	public class HomeController : Controller {
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger) {
			_logger = logger;
		}

		public IActionResult Index() {
			return View();
		}

		public IActionResult RequestWebAPI() {

			var webHookUrl = Request.Scheme + "://" + Request.Host + "/Home/WebHook";
			var requestUrl = "https://localhost:7210/api/DocumentProcessing/merge?webHookUrl=" + webHookUrl;

			HttpClient client = new HttpClient();
			HttpResponseMessage responseMessage = client.PostAsync(requestUrl, null).Result;

			return View();
		}

		[HttpPost]
		public bool WebHook([FromBody] object request) {

			dynamic ProcessingRequest = JObject.Parse(request.ToString());

			HttpClient client = new HttpClient();
			HttpResponseMessage responseMessage = client.GetAsync(ProcessingRequest.RetrieveDocumentUrl.Value).Result;

			if (responseMessage.IsSuccessStatusCode) { 
				string data = responseMessage.Content.ReadAsStringAsync().Result;
				System.IO.File.WriteAllBytes("App_Data/" + ProcessingRequest.Id.Value + ".pdf", Convert.FromBase64String(data));
				return true;
			}
			else
				return true;
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() {
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}