using System;
using CFOP.Service.Common.Models;

namespace CFOP.Service.VideoCall
{
    public interface IVideoService
    {
        void Call(User user, Action finishCallback);
    }
}
