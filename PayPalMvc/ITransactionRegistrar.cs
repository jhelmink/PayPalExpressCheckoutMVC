using System.Web.Routing;

namespace PayPalMvc {
	public interface ITransactionRegistrar {
		/// <summary>
		/// Sends a transaction registration to PayPal
		/// </summary>
        SetExpressCheckoutResponse SendSetExpressCheckout(string currencyCode, decimal amount, string description, string trackingReference, string serverURL, string userEmail = null);

        /// <summary>
        /// Gets results of transaction
        /// </summary>
        GetExpressCheckoutDetailsResponse SendGetExpressCheckoutDetails(string token);

        /// <summary>
        /// Requests the payment to be completed
        /// </summary>
        DoExpressCheckoutPaymentResponse SendDoExpressCheckoutPayment(string token, string payerId, string currencyCode, decimal amount);
    }
}