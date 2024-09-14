using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using xObsAsyncImageSource.ImageSelector.Interfaces;
using xObsAsyncImageSource.ImageSelector.Messages;

namespace xObsAsyncImageSource.ImageSelector.ViewModels
{
    public partial class uTitleBarVM : ObservableObject, IuTitleBarVM
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private Visibility visibility;

        public uTitleBarVM()
        {
            Title = "ImageSelector";
        }
    }

    public partial class uTitleBarVM
    {
        [RelayCommand]
        private void OnClose()
        {
            WeakReferenceMessenger.Default.Send(new WindowCloseMessage("OnClose"));
        }
    }
}
