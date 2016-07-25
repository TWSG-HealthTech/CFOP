using System;

namespace CFOP.Service.VideoCall
{
    [Obsolete("for now use skype uri instead")]
    public interface IVideoService
    {
        void Call(string userId);
    }
}
