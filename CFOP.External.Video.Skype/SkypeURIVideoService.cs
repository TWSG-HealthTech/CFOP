using System.Diagnostics;
using CFOP.Service.Common.DTO;
using CFOP.Service.VideoCall;

namespace CFOP.External.Video.Skype
{
    public class SkypeURIVideoService : IVideoService
    {
        public void Call(User user)
        {
            var skypeProcess = Process.Start($"skype:{user.Skype}?call&video=true");
        }
    }
}
