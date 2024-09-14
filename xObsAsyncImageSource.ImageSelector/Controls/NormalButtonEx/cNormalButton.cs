using System.Windows;
using System.Windows.Controls;

namespace xObsAsyncImageSource.ImageSelector.Controls.NormalButtonEx
{
    public partial class cNormalButton : Button
    {
        static cNormalButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cNormalButton), new FrameworkPropertyMetadata(typeof(cNormalButton)));
        }
    }

    public partial class cNormalButton
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            name: "Text",
            propertyType: typeof(string),
            ownerType: typeof(cNormalButton),
            typeMetadata: new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public bool HasChildren
        {
            get { return (bool)GetValue(HasChildrenProperty); }
            set { SetValue(HasChildrenProperty, value); }
        }
        public static readonly DependencyProperty HasChildrenProperty = DependencyProperty.Register(
            name: "HasChildren",
            propertyType: typeof(bool),
            ownerType: typeof(cNormalButton),
            typeMetadata: new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
