using Prism.Commands;
using Prism.Mvvm;
using SKYPE4COMLib;
using ICommand = System.Windows.Input.ICommand;

namespace CFOP.VideoCall
{
    public class VideoCallViewModel : BindableBase
    {
        private string _skypeContact;
        public string SkypeContact
        {
            get { return _skypeContact; }
            set
            {
                _skypeContact = value;
                OnPropertyChanged(() => SkypeContact);
                (SkypeVideoCallCommand as DelegateCommand).RaiseCanExecuteChanged();
            }
        }

        public VideoCallViewModel()
        {
            SkypeVideoCallCommand = new DelegateCommand(SkypeVideoCall, () => !string.IsNullOrWhiteSpace(SkypeContact));
        }

        #region Commands

        public ICommand SkypeVideoCallCommand { get; private set; }

        private void SkypeVideoCall()
        {
            var skype = new Skype();
            skype.Attach(7, false);

            //Log in to skype if not yet
            if (!skype.Client.IsRunning)
            {
                skype.Client.Start(true, true);
            }

            skype.Attach(skype.Protocol);

            skype.CallStatus += OnSkypeCallStatusChanged;
            var call = skype.PlaceCall(SkypeContact);
        }

        private void OnSkypeCallStatusChanged(Call call, TCallStatus status)
        {
            if (status == TCallStatus.clsInProgress && call.PartnerHandle == SkypeContact)
            {
                call.StartVideoSend();
            }
        }

        #endregion
    }
}
