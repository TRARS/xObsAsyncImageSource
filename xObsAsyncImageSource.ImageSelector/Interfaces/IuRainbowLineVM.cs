using System.Windows.Media;

namespace xObsAsyncImageSource.ImageSelector.Interfaces
{
    public interface IuRainbowLineVM : IVM
    {
        Brush BrushColor { get; set; }
        double Width { get; set; }
        double Height { get; set; }
    }
}
