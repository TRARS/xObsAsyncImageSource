using System;
using System.Windows;

namespace xObsAsyncImageSource.ImageSelector
{
    public partial class App : Application
    {
        public App()
        {
            //使SelectionTextBrush生效
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
        }
    }
}
