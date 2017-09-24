namespace PayPalNetStd
{
    public class PayPalPayment
    {
        public PayPalPayment()
        {
        }
        public long PayerId { get; set; }
        public string ClientCode { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public int Status { get; set; }
        public string PayPal_PaymentId { get; set; }
        public string PayPal_ExecutePaymentId { get; set; }
        public string PayPal_AccessToken { get; set; }
        public string PayPal_PaymentState { get; set; }
        public string PayPal_PayerId { get; set; }
        public string PayPal_PayerEmail { get; set; }
        public string PayPal_PayerCountryCode { get; set; }

        public decimal PayPal_Amount { get; set; }
        public string PayPal_Amount_Currency { get; set; }
        public decimal PayPal_Fees { get; set; }
        public string PayPal_Fees_Currency { get; set; }

        public string PaymentUrl { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public DateTime? CancelationDate { get; set; }

        public Guid TransactionGuid { get; set; }
    }
}