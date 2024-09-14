using System.Windows;
using System.Windows.Controls;

namespace xObsAsyncImageSource.ImageSelector.Controls.TextBoxEx
{
    public class cTextBox : TextBox
    {
        static cTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cTextBox), new FrameworkPropertyMetadata(typeof(cTextBox)));
        }
    }
}
