using LiteDB;
using Newtonsoft.Json;
using System.Text;

namespace tx_wp_api {
	public class TextControlProcessing {
		public static void Merge(ProcessingRequest request) {
			
			using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
				tx.Create();

				// simulate long process
				Thread.Sleep(3000);

				// create document or use MailMerge to generate larger document
				tx.Text = "My created document.";
				
				byte[] data;
				tx.Save(out data, TXTextControl.BinaryStreamType.AdobePDF);

				request.StoreDocument(data);
			}

			request.Processed = true;
			request.Update();

			Task.Run(() => FireAndForgetWebHook(request));
		}

		private static void FireAndForgetWebHook(ProcessingRequest request) {
			HttpClient client = new HttpClient();

			var json = JsonConvert.SerializeObject(request);
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			client.PostAsync(request.WebHookUrl, data);
		}
	}
}
