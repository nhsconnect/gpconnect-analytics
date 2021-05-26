namespace gpconnect_analytics.DTO.Response.Configuration
{
    public class Email
    {
        public string SenderAddress { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Encryption { get; set; }
        public bool AuthenticationRequired { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DefaultSubject { get; set; }
        public string RecipientAddress { get; set; }
    }
}
