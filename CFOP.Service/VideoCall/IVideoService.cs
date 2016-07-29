using System;
using CFOP.Service.Common.DTO;

namespace CFOP.Service.VideoCall
{
    public interface IVideoService
    {
        void Call(User user);
    }
}
