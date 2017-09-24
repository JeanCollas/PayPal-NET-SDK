Samples for PayPal .NET SDK ported to .Net Standard
===========================

This is not a whole sample, but most of the pieces of code required to make the paypal payment work.
Example: The PayPalService class requires a DbContext, which is not provided in the sample. 
The sample has not been tested, but extracted from a working project.

Registering the PayPalService allows to call methods like:

                PayPalPayment ppp = await _PaypalManager.GetPayment(pmt.PayerId, pmt.ClientCode,
                                        price.Id.ToString(), product.Name,
                                        "", pmt.TotalAmount, pmt.Currency, product.Description,
                                        $"https://{_contextAccessor.HttpContext.Request.Host.ToUriComponent()}/SomePaymentControllerPath/{pmt.BuyerId}/VerifyPublishPayment/{pmt.TransactionGuid}",
                                        $"https://{_contextAccessor.HttpContext.Request.Host.ToUriComponent()}/SomePaymentControllerPath/{pmt.BuyerId}/PublishPaymentCanceled/{pmt.TransactionGuid}",
                                        pmt.TransactionGuid, billAddress, true );


Then execute a payment:

    await _PaypalManager.Execute(ppp, paymentId, token, PayerID);
    
Or check why it has been canceled:

            PaymentStatus ps = await _PaypalManager.CheckStatus(ppp.PayPal_PaymentId, ppp.PayPal_AccessToken);
            switch (ps)
            {
                case PaymentStatus.Authorized:
                    // Check if not already paid
                    break;
                case PaymentStatus.Unknown:
                case PaymentStatus.Canceled:
                case PaymentStatus.Pending:
                    // Ok, canceled, delete request
                    break;
            }

