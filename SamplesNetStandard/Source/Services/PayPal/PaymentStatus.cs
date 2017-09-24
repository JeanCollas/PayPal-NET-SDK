namespace PayPalNetStd
{
    public enum PaymentStatus : int
    {
        Unknown=-1,
        Pending = 0,
        Authorized = 1,
        Paid = 2,
        Voided = 3,
        Refunded = 4,
        Canceled=5
    }
}