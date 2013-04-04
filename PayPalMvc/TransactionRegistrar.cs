using System.Web.Routing;
using System;
using System.Web.Configuration;

namespace PayPalMvc
{
	/// <summary>
	/// Default ITransactionRegistrar implementation
	/// </summary>
	public class TransactionRegistrar : ITransactionRegistrar
    {
		readonly Configuration configuration;
		readonly IHttpRequestSender requestSender;
        private HttpPostSerializer serializer;
        private ResponseSerializer deserializer;
		
        /// <summary>
		/// Creates a new instance of the TransactionRegistrar using the configuration specified in the web.conf, and an HTTP Request Sender.
		/// </summary>
		public TransactionRegistrar() : this(Configuration.Current, new HttpRequestSender()) {
		}

		/// <summary>
		/// Creates a new instance of the TransactionRegistrar
		/// </summary>
		public TransactionRegistrar(Configuration configuration, IHttpRequestSender requestSender)
        {
			this.configuration = configuration;
			this.requestSender = requestSender;
            this.serializer = new HttpPostSerializer();
            this.deserializer = new ResponseSerializer();
		}

        /// <summary>
        /// Setup the Express Checkout request with PayPal
        /// </summary>
        /// <param name="currencyCode">Currency Code to use for sale</param>
        /// <param name="amount">Total amount of sale</param>
        /// <param name="description">Description that PayPal will show to users for this sale</param>
        /// <param name="trackingReference">Unique tracking references for this sale</param>
        /// <param name="serverURL">Your server URL (Cancel/Return Actions get appended to this)</param>
        /// <param name="userEmail">Optional email for user making purchase</param>
        /// <returns></returns>
        public SetExpressCheckoutResponse SendSetExpressCheckout(string currencyCode, decimal amount, string description, string trackingReference, string serverURL, string userEmail = null)
        {
            SetExpressCheckoutRequest request = new SetExpressCheckoutRequest(currencyCode, amount, description, trackingReference, serverURL, userEmail);
            
            string postData = serializer.Serialize(request);
            Logging.LogLongMessage("PayPal Send Request", "Serlialized Request to PayPal API: " + postData);
            
            string response = requestSender.SendRequest(Configuration.Current.PayPalAPIUrl, postData);
            string decodedResponse = System.Web.HttpUtility.UrlDecode(response, System.Text.Encoding.Default);
            Logging.LogLongMessage("PayPal Response Recieved", "Decoded Respose from PayPal API: " + decodedResponse);
            
            return deserializer.Deserialize<SetExpressCheckoutResponse>(decodedResponse);
        }

        /// <summary>
        /// Get PayPal purchase status for the sale and the PayPal account details used for purchase
        /// </summary>
        /// <param name="token">The Express Checkout token for this sale</param>
        /// <returns></returns>
        public GetExpressCheckoutDetailsResponse SendGetExpressCheckoutDetails(string token)
        {
            GetExpressCheckoutDetailsRequest request = new GetExpressCheckoutDetailsRequest(token);
            
            string postData = serializer.Serialize(request);
            Logging.LogLongMessage("PayPal Send Request", "Serlialized Request to PayPal API: " + postData);
            
            string response = requestSender.SendRequest(Configuration.Current.PayPalAPIUrl, postData);
            string decodedResponse = System.Web.HttpUtility.UrlDecode(response, System.Text.Encoding.Default);
            Logging.LogLongMessage("PayPal Response Recieved", "Decoded Respose from PayPal API: " + decodedResponse);
            
            return deserializer.Deserialize<GetExpressCheckoutDetailsResponse>(decodedResponse);
        }

        /// <summary>
        /// Request payment to be taken by PayPal for the sale
        /// </summary>
        /// <param name="token">The Express Checkout token for this sale</param>
        /// <param name="payerId">The PayerId of the PayPal account used for this purchase</param>
        /// <param name="currencyCode">Currency Code to use for sale</param>
        /// <param name="amount">Total amount of sale</param>
        /// <returns></returns>
        public DoExpressCheckoutPaymentResponse SendDoExpressCheckoutPayment(string token, string payerId, string currencyCode, decimal amount)
        {
            DoExpressCheckoutPaymentRequest request = new DoExpressCheckoutPaymentRequest(token, payerId, currencyCode, amount);
            
            string postData = serializer.Serialize(request);
            Logging.LogLongMessage("PayPal Send Request", "Serlialized Request to PayPal API: " + postData);
            
            string response = requestSender.SendRequest(Configuration.Current.PayPalAPIUrl, postData);
            string decodedResponse = System.Web.HttpUtility.UrlDecode(response, System.Text.Encoding.Default);
            Logging.LogLongMessage("PayPal Response Recieved", "Decoded Respose from PayPal API: " + decodedResponse);
            
            return deserializer.Deserialize<DoExpressCheckoutPaymentResponse>(decodedResponse);
        }
	}
}