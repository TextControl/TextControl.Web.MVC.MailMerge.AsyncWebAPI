namespace tx_api_consumer.Models {
	public class ErrorViewModel {
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}