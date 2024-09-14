using System.Windows;
using System.Windows.Controls;

namespace xObsAsyncImageSource.ImageSelector.Controls.TitleBarButtonEx
{
    public partial class cTitleBarButton : Button
    {
        static cTitleBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cTitleBarButton), new FrameworkPropertyMetadata(typeof(cTitleBarButton)));
        }
    }

    public partial class cTitleBarButton
    {
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            name: "IsActive",
            propertyType: typeof(bool),
            ownerType: typeof(cTitleBarButton),
            typeMetadata: new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public ButtonType Type
        {
            get { return (ButtonType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            name: "Type",
            propertyType: typeof(ButtonType),
            ownerType: typeof(cTitleBarButton),
            typeMetadata: new FrameworkPropertyMetadata(ButtonType.EmptyBtn, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(
            name: "BorderCornerRadius",
            propertyType: typeof(CornerRadius),
            ownerType: typeof(cTitleBarButton),
            typeMetadata: new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
