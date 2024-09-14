using System.Windows;
using System.Windows.Controls;

namespace xObsAsyncImageSource.ImageSelector.Controls.ListBoxEx
{
    public partial class cListBox : ListBox
    {
        static cListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cListBox), new FrameworkPropertyMetadata(typeof(cListBox)));
        }
    }

    public partial class cListBox
    {

    }
}
