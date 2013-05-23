﻿using System;
using System.Web.Routing;
using PayPalMvc;
using SampleMVC3WebApplication.Models;
using System.Collections.Generic;

namespace SampleMVC3WebApplication.Services
{
    public interface ITransactionService
    {
        SetExpressCheckoutResponse SendPayPalSetExpressCheckoutRequest(ApplicationCart cart, string serverURL, string userEmail = null);
        GetExpressCheckoutDetailsResponse SendPayPalGetExpressCheckoutDetailsRequest(string token);
        DoExpressCheckoutPaymentResponse SendPayPalDoExpressCheckoutPaymentRequest(ApplicationCart cart, string token, string payerId);
    } 

    /// <summary>
    /// The Transaction Service allows your app to store the transactions in your database (create a table to match the PayPalTransaction model)
    /// You can also use it to transform your systems purchase object (eg cart, basket, or single item) into one that will fit with PayPal
    /// You should copy this into your project and modify it to accept your purchase object
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private PayPalMvc.ITransactionRegistrar _payPalTransactionRegistrar = new PayPalMvc.TransactionRegistrar();

        public SetExpressCheckoutResponse SendPayPalSetExpressCheckoutRequest(ApplicationCart cart, string serverURL, string userEmail = null)
        {
            try
            {
                WebUILogging.LogMessage("SendPayPalSetExpressCheckoutRequest");

                // Optional handling of cart items: If there is only a single item being sold we don't need a list of expressCheckoutItems
                // Note: Items are currently NOT stored by PayPal in the order history so you need to keep your own records of what items were in the cart
                List<ExpressCheckoutItem> expressCheckoutItems = null;
                if (cart.Items != null)
                {
                    expressCheckoutItems = new List<ExpressCheckoutItem>();
                    foreach (ApplicationCartItem item in cart.Items)
                        expressCheckoutItems.Add(new ExpressCheckoutItem(item.Quantity, item.Price, item.Name, item.Description));
                }

                SetExpressCheckoutResponse response = _payPalTransactionRegistrar.SendSetExpressCheckout(cart.Currency, cart.TotalPrice, cart.PurchaseDescription, cart.Id.ToString(), serverURL, expressCheckoutItems, userEmail);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = cart.Id.ToString(),
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

        public DoExpressCheckoutPaymentResponse SendPayPalDoExpressCheckoutPaymentRequest(ApplicationCart cart, string token, string payerId)
        {
            try
            {
                WebUILogging.LogMessage("SendPayPalDoExpressCheckoutPaymentRequest");
                DoExpressCheckoutPaymentResponse response = _payPalTransactionRegistrar.SendDoExpressCheckoutPayment(token, payerId, cart.Currency, cart.TotalPrice);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = cart.Id.ToString(),
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