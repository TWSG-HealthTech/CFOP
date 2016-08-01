using CFOP.Speech;

namespace CFOP.Common
{
    public interface IConversation
    {
        bool CanHandle(IntentResponse.Intent intent);
        void Handle(IntentResponse.Intent intent);
    }
}
