    public class PayPalExecuteException : Exception
    {
        public PayPalExecuteErrors Error { get; set; }

        public enum PayPalExecuteErrors
        {
            Pending,
            Failed,
            AlreadyDone,
            PayPalErrorTryAgain
        }
    }
