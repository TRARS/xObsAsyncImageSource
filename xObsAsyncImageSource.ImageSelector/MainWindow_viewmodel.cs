using System.Collections.ObjectModel;
using xObsAsyncImageSource.ImageSelector.Interfaces;
using xObsAsyncImageSource.ImageSelector.ViewModels;

namespace xObsAsyncImageSource.ImageSelector
{
    partial class MainWindow_viewmodel
    {
        public ObservableCollection<IVM> SubViewModelList { get; init; }

        public MainWindow_viewmodel()
        {
            this.SubViewModelList = new()
            {
                new uTitleBarVM(),
                new uRainbowLineVM(),
                new uClientVM(),
            };
        }
    }
}
