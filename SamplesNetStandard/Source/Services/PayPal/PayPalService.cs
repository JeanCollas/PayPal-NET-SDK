using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PayPal.Api;
using Microsoft.Extensions.Options;

namespace PayPalNetStd
{
    public class PayPalService
    {
        private readonly AppDbContext DbContext;
        Dictionary<string, string> Config;
        private PayPal.Api.OAuthTokenCredential PPAuth;

        private AppDbContext Context { get { return DbContext; } }
        public PayPalService(AppDbContext context,
            IOptions<PaypalConfig> config)
        {
            DbContext = context;
            Config = new Dictionary<string, string>() { { "mode", config.Value.Mode }, { "clientId", config.Value.ClientId }, { "clientSecret", config.Value.ClientSecret } };
            PPAuth = new PayPal.Api.OAuthTokenCredential(Config);
        }

        static CultureInfo CultureEnUs = new System.Globalization.CultureInfo("en-US");

        //static string Mode = "sandbox";
        //static string ClientId = "XXX";
        //static string ClientSecret = "YYYYY";
        //static PayPalService()
        //{
        //    Config = new Dictionary<string, string>() { { "mode", Mode }, { "clientId", ClientId }, { "clientSecret", ClientSecret } };
        //}


        public async Task<PayPalPayment> GetPayment(
            long payerId, string clientCode, string itemPriceId,
            string itemName, string invoiceNumber, decimal totalAmount, string currency, string description, string returnUrl, string cancelUrl,
            Guid transactId, BillingAddress bAddress, bool insertInDB)
        {


            PayPalPayment ppp = new PayPalPayment()
            {
                //CreationDate =,ApprovalDate=,CancelationDate=,ExecutionDate=,Id =,
                PayerId = payerId,
                ClientCode = clientCode,
                TransactionGuid = transactId,
                Amount = totalAmount,
                Currency = currency
                //PaymentState =     ,
                //PayPalAccessToken =,
                //PayPalPaymentId =

                //Id =,
            };

            // Authenticate with PayPal 
            // TODO: Access token not required to be created every time
            var accessToken = PPAuth.GetAccessToken();
            var apiContext = new PayPal.Api.APIContext(accessToken);

            string totalStr = totalAmount.ToString("0.00", CultureEnUs);

            var item = new PayPal.Api.Item
            {
                name = itemName,
                currency = currency,
                price = totalStr,
                quantity = "1",
                sku = itemPriceId
            };

            var amount = new PayPal.Api.Amount
            {
                currency = currency,
                total = totalStr,
                details = new PayPal.Api.Details
                {
                    tax = "0",
                    shipping = "0",
                    subtotal = totalStr,
                }
            };

                PayPal.Api.ShippingAddress paypalAddr = new PayPal.Api.ShippingAddress()
                {
                    city = bAddress.City,
                    country_code = bAddress.CountryCode,
                    line1 = bAddress.Street,
                    line2 = bAddress.Street2,
                    postal_code = bAddress.Zip,
                    state = bAddress.State
                };

            // Make an API call
            var createdPayment = await PayPal.Api.Payment.CreateAsync(apiContext, new PayPal.Api.Payment
            {
                intent = "sale",
                payer = new PayPal.Api.Payer { payment_method = "paypal" },

                transactions = new List<PayPal.Api.Transaction>
                {
                    new PayPal.Api.Transaction
                    {
                        description = description,
                        invoice_number = invoiceNumber,
                        amount = amount,
                        //notify_url=,
                        
                        item_list = new PayPal.Api.ItemList
                        {
                            items = new List<PayPal.Api.Item>
                            {
                                item
                            },
                            shipping_address=paypalAddr
                        }
                    }
                },
                redirect_urls = new PayPal.Api.RedirectUrls
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            });


            ppp.PayPal_PaymentId = createdPayment.id;
            ppp.PayPal_AccessToken = createdPayment.token;

            ppp.PaymentUrl = createdPayment.GetHateoasLink("approval_url")?.href;
            ppp.PayPal_PaymentState = createdPayment.state;

            if (insertInDB)
            {
                DbContext.PayPalPayments.Add(ppp);
                await DbContext.SaveChangesAsync();
            }
            return ppp;
        }

        PayPal.Api.APIContext GetContext()
        {
            // Authenticate with PayPal 
            // TODO: Access token not required to be created every time
            var auth = new PayPal.Api.OAuthTokenCredential(Config);
            var accessToken = auth.GetAccessToken();
            return new PayPal.Api.APIContext(accessToken);
        }

        public async Task<string> GetPaymentLink(string itemName, string invoiceNumber, decimal totalAmount, string currency, string description, string returnUrl, string cancelUrl)
        {
            var apiContext = GetContext();

            string totalStr = totalAmount.ToString("0.00", CultureEnUs);

            var item = new PayPal.Api.Item
            {
                name = itemName,
                currency = currency,
                price = totalStr,
                quantity = "1",
                sku = "sku"
            };

            var amount = new PayPal.Api.Amount
            {
                currency = currency,
                total = totalStr,
                details = new PayPal.Api.Details
                {
                    tax = "0",
                    shipping = "0",
                    subtotal = totalStr,
                }
            };



            // Make an API call
            var createdPayment = await PayPal.Api.Payment.CreateAsync(apiContext, new PayPal.Api.Payment
            {
                intent = "sale",
                payer = new PayPal.Api.Payer { payment_method = "paypal" },

                transactions = new List<PayPal.Api.Transaction>
                {
                    new PayPal.Api.Transaction
                    {
                        description = description,
                        invoice_number = invoiceNumber,
                        amount = amount,
                        //notify_url=,
                        
                        item_list = new PayPal.Api.ItemList
                        {
                            items = new List<PayPal.Api.Item>
                            {
                                item
                            }
                        }
                    }
                },
                redirect_urls = new PayPal.Api.RedirectUrls
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            });


            var lnks = createdPayment.GetHateoasLink("approval_url");
            return lnks?.href;


            var links = createdPayment.links.GetEnumerator();
            while (links.MoveNext())
            {
                var link = links.Current;
                if (link.rel.ToLower().Trim().Equals("approval_url"))
                {
                    return link.href;
	//                    Console.WriteLine(link.href);
	//                    this.flow.RecordRedirectUrl("Redirect to PayPal to approve the payment...", link.href);
                }
            }
            return null;



        }

        public async Task<PaymentStatus> CheckStatus(string payPal_PaymentId, string payPal_AccessToken)
        {
            var apiContext = GetContext();
            var pmt = await PayPal.Api.Payment.GetAsync(apiContext, payPal_PaymentId);
            return StateToStatus(pmt.state);
        }

        const string STATE_CREATED = "created";
        const string STATE_APPROVED = "approved";
        const string STATE_FAILED = "failed";
        const string STATE_COMPLETED = "completed";
        const string PAYMENT_ALREADY_DONE = "PAYMENT_ALREADY_DONE";

        public static PaymentStatus StateToStatus(string state)
        {
            switch (state)
            {
                case STATE_CREATED:
                    return PaymentStatus.Pending;
                case STATE_APPROVED:
                    return PaymentStatus.Authorized;
                case STATE_FAILED:
                    return PaymentStatus.Canceled;
                    //                    failure_reason enum
                    //Failure reason code returned when the payment failed for some valid reasons.
                    //Possible values: UNABLE_TO_COMPLETE_TRANSACTION, INVALID_PAYMENT_METHOD, PAYER_CANNOT_PAY, CANNOT_PAY_THIS_PAYEE, REDIRECT_REQUIRED, PAYEE_FILTER_RESTRICTIONS

            }
            return PaymentStatus.Unknown;
        }

        public async Task<PayPalPayment> Execute(PayPalPayment ppp, string paymentId, string token, string payerID)
        {

            var apiContext = GetContext();
            var pe = new PaymentExecution() { payer_id = payerID };
            PayPal.Api.Payment pmt=null;
            try
            {
                pmt = await PayPal.Api.Payment.ExecuteAsync(apiContext, paymentId, pe);
            }catch(PayPal.PaymentsException exc)
            {
                if (exc.Details.name == PAYMENT_ALREADY_DONE)
                    throw new PayPalExecuteException() { Error = PayPalExecuteException.PayPalExecuteErrors.AlreadyDone };
                //if(exc.Error)
                //PAYMENT_ALREADY_DONE
            }catch(PayPal.HttpException exc)
            {
                throw new PayPalExecuteException() { Error = PayPalExecuteException.PayPalExecuteErrors.PayPalErrorTryAgain };
            }

            if (pmt.state == STATE_CREATED)
                throw new PayPalExecuteException() { Error = PayPalExecuteException.PayPalExecuteErrors.Pending };
            if (pmt.state == STATE_FAILED)
                throw new PayPalExecuteException() { Error = PayPalExecuteException.PayPalExecuteErrors.Failed };

            ppp.PayPal_ExecutePaymentId= pmt.id;
            ppp.PayPal_AccessToken = pmt.token;
            ppp.PayPal_PayerId = pmt.payer.payer_info.payer_id;
            ppp.PayPal_PayerEmail = pmt.payer.payer_info.email;
            ppp.PayPal_PayerCountryCode = pmt.payer.payer_info.country_code;
            var note = pmt.note_to_payer;

            decimal totalAmount = 0;
            decimal fees = 0;
			
//            float payeeReceived = 0;

            foreach(var trans in pmt.transactions)
            {
                foreach(var ress in trans.related_resources)
                {
                    if (ress.sale.state == STATE_COMPLETED)
                    {
                        totalAmount += decimal.Parse(ress.sale.amount.total, CultureEnUs);
                        fees += decimal.Parse(ress.sale.transaction_fee.value, CultureEnUs);
                        ppp.PayPal_Amount_Currency = ress.sale.amount.currency;
                        ppp.PayPal_Fees_Currency = ress.sale.transaction_fee.currency;
                        //payeeReceived += float.Parse(ress.sale.receivable_amount.value);
                    }
                }
            }
            ppp.PayPal_Amount = totalAmount;
            ppp.PayPal_Fees = fees;

            if(ppp.PayPal_Amount==ppp.Amount)
            {
                ppp.Status = (int)PaymentStatus.Paid;
            }
            return ppp;
        }
    }

}
