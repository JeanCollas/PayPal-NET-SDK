namespace PayPalNetStd
{
    public class BillingAddress
    {
        public BillingAddress()
        {
        }
        public long Id { get; set; }
        public string Name { get; set; }
        public string StreetNum { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatorId { get; set; }
        public DateTime? DeletionDate { get; set; }
   }
}