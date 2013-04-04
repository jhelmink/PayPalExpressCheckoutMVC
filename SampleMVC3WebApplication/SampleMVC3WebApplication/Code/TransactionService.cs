using System;
using System.Web.Routing;
using PayPalMvc;
using SampleMVC3WebApplication.Models;

namespace SampleMVC3WebApplication.Services
{
    public interface ITransactionService
    {
        SetExpressCheckoutResponse SendPayPalSetExpressCheckoutRequest(TicketBasket basket, string serverURL, string userEmail = null);
        GetExpressCheckoutDetailsResponse SendPayPalGetExpressCheckoutDetailsRequest(string token);
        DoExpressCheckoutPaymentResponse SendPayPalDoExpressCheckoutPaymentRequest(TicketBasket basket, string token, string payerId);
    } 

    /// <summary>
    /// The Transaction Service allows your app to store the transactions in your database (create a table to match the PayPalTransaction model)
    /// You should modify this to accept your purchase object, eg a basket in this case
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private PayPalMvc.ITransactionRegistrar _payPalTransactionRegistrar;

        public TransactionService(PayPalMvc.ITransactionRegistrar payPalTransactionRegistrar)
        {
            _payPalTransactionRegistrar = payPalTransactionRegistrar;
        }

        public SetExpressCheckoutResponse SendPayPalSetExpressCheckoutRequest(TicketBasket basket, string serverURL, string userEmail = null)
        {
            try
            {
                WebUILogging.LogMessage("SendPayPalSetExpressCheckoutRequest");
                SetExpressCheckoutResponse response = _payPalTransactionRegistrar.SendSetExpressCheckout(basket.Currency, basket.TotalPrice, basket.PurchaseDescription, basket.Id.ToString(), serverURL, userEmail);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = basket.Id.ToString(),
                    RequestTime = DateTime.Now,
                    RequestStatus = response.ResponseStatus.ToString(),
                    TimeStamp = response.TIMESTAMP,
                    RequestError = response.ErrorToString,
                    Token = response.TOKEN,
                };

                // Store this transaction in your Database

                return response;
            }
            catch (Exception ex)
            {
                WebUILogging.LogException(ex.Message, ex);
            }
            return null;
        }

        public GetExpressCheckoutDetailsResponse SendPayPalGetExpressCheckoutDetailsRequest(string token)
        {
            try
            {
                WebUILogging.LogMessage("SendPayPalGetExpressCheckoutDetailsRequest");
                GetExpressCheckoutDetailsResponse response = _payPalTransactionRegistrar.SendGetExpressCheckoutDetails(token);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = response.TrackingReference,
                    RequestTime = DateTime.Now,
                    RequestStatus = response.ResponseStatus.ToString(),
                    TimeStamp = response.TIMESTAMP,
                    RequestError = response.ErrorToString,
                    Token = response.TOKEN,
                    PayerId = response.PAYERID,
                    RequestData = response.ToString,
                };

                // Store this transaction in your Database

                return response;
            }
            catch (Exception ex)
            {
                WebUILogging.LogException(ex.Message, ex);
            }
            return null;
        }

        public DoExpressCheckoutPaymentResponse SendPayPalDoExpressCheckoutPaymentRequest(TicketBasket basket, string token, string payerId)
        {
            try
            {
                WebUILogging.LogMessage("SendPayPalDoExpressCheckoutPaymentRequest");
                DoExpressCheckoutPaymentResponse response = _payPalTransactionRegistrar.SendDoExpressCheckoutPayment(token, payerId, basket.Currency, basket.TotalPrice);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = basket.Id.ToString(),
                    RequestTime = DateTime.Now,
                    RequestStatus = response.ResponseStatus.ToString(),
                    TimeStamp = response.TIMESTAMP,
                    RequestError = response.ErrorToString,
                    Token = response.TOKEN,
                    RequestData = response.ToString,
                    PaymentTransactionId = response.PaymentTransactionId,
                    PaymentError = response.PaymentErrorToString,
                };

                // Store this transaction in your Database

                return response;
            }
            catch (Exception ex)
            {
                WebUILogging.LogException(ex.Message, ex);
            }
            return null;
        }
    }
}