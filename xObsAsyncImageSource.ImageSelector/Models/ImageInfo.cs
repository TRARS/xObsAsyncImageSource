using CommunityToolkit.Mvvm.ComponentModel;

namespace xObsAsyncImageSource.ImageSelector.Models
{
    public partial class ImageInfo : ObservableObject
    {
        [ObservableProperty]
        private bool isChecked;

        [ObservableProperty]
        private string imageSource;

        [ObservableProperty]
        private string imageName;
    }
}
