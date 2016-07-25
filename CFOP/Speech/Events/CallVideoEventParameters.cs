namespace CFOP.Speech.Events
{
    public class CallVideoEventParameters
    {
        public string User { get; private set; }

        public CallVideoEventParameters(string user)
        {
            User = user;
        }
    }
}
