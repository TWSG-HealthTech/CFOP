namespace CFOP.Speech
{
    public interface IConversation
    {
        bool CanHandle(IntentResponse.Intent intent);
        void Handle(IntentResponse.Intent intent);
        void HandleConfirmation();
        void HandleCancelling();
    }
}
