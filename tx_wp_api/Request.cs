using LiteDB;

namespace tx_wp_api {
	public class ProcessingRequest {

		private string _retrieveDocumentUrl;

		public string Id { get; set; }
		public bool Processed { get; set; }
		public string? WebHookUrl { get; set; }
		public string RetrieveDocumentUrl {
			get { return _retrieveDocumentUrl; }
			set { _retrieveDocumentUrl = value; }
		}

		public ProcessingRequest() { }
		
		public ProcessingRequest(string Id) {
			using (var db = new LiteDatabase(@"Filename=App_Data/processingqueue.db; Connection=shared")) {
				var col = db.GetCollection<ProcessingRequest>("queue");

				col.EnsureIndex(x => x.Id);

				var results = col.Query().Where(x => x.Id == Id);

				if (results != null) { 
					this.WebHookUrl = results.First().WebHookUrl;
					this.Id = results.First().Id;
					this.Processed = results.First().Processed;
				}
			}
		}

		public void StoreDocument(byte[] document) {
			using (var db = new LiteDatabase(@"Filename=App_Data/documents.db; Connection=shared")) {

				MemoryStream stream = new MemoryStream(document);

				// upload document to db
				var fs = db.FileStorage;
				stream.Position = 0;
				var test = fs.Upload("$/templates/" + this.Id, this.Id, stream);
			}
		}

		public string RetrieveDocument() {
			using (var db = new LiteDatabase(@"Filename=App_Data/documents.db; Connection=shared")) {
				var fs = db.FileStorage;

				using (MemoryStream ms = new MemoryStream()) {
					if (fs.Exists("$/templates/" + this.Id) == true) {
						fs.Download("$/templates/" + this.Id, ms);
						return Convert.ToBase64String(ms.ToArray());
					}
					else
						return null;
				}
			};
		}

		public void Create(string requestUrl) {

			this.Id = Guid.NewGuid().ToString();
			this.RetrieveDocumentUrl = requestUrl.Replace("/1", "/" + this.Id);

			using (var db = new LiteDatabase(@"Filename=App_Data/processingqueue.db; Connection=shared")) {
				var col = db.GetCollection<ProcessingRequest>("queue");
				col.Insert(this);
			}
		}

		public void Update() {
			using (var db = new LiteDatabase(@"Filename=App_Data/processingqueue.db; Connection=shared")) {
				var col = db.GetCollection<ProcessingRequest>("queue");

				col.EnsureIndex(x => x.Id);

				var results = col.Query().Where(x => x.Id == this.Id).First();
				results.Processed = this.Processed;

				col.Update(results);
			}
		}
	}

}