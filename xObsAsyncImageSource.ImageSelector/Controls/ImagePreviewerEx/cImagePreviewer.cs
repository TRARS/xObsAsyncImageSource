using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace xObsAsyncImageSource.ImageSelector.Controls.ImagePreviewerEx
{
    public partial class cImagePreviewer : ListView
    {
        static cImagePreviewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(cImagePreviewer), new FrameworkPropertyMetadata(typeof(cImagePreviewer)));
        }
    }

    public partial class cImagePreviewer
    {
        public double ItemSize
        {
            get { return (double)GetValue(ItemSizeProperty); }
            set { SetValue(ItemSizeProperty, value); }
        }
        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(
            name: "ItemSize",
            propertyType: typeof(double),
            ownerType: typeof(cImagePreviewer),
            typeMetadata: new FrameworkPropertyMetadata(64.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public bool ItemCheckerEnabled
        {
            get { return (bool)GetValue(ItemCheckerEnabledProperty); }
            set { SetValue(ItemCheckerEnabledProperty, value); }
        }
        public static readonly DependencyProperty ItemCheckerEnabledProperty = DependencyProperty.Register(
            name: "ItemCheckerEnabled",
            propertyType: typeof(bool),
            ownerType: typeof(cImagePreviewer),
            typeMetadata: new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public bool ItemBorderEnabled
        {
            get { return (bool)GetValue(ItemBorderEnabledProperty); }
            set { SetValue(ItemBorderEnabledProperty, value); }
        }
        public static readonly DependencyProperty ItemBorderEnabledProperty = DependencyProperty.Register(
            name: "ItemBorderEnabled",
            propertyType: typeof(bool),
            ownerType: typeof(cImagePreviewer),
            typeMetadata: new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public SolidColorBrush ItemBorderColor
        {
            get { return (SolidColorBrush)GetValue(ItemBorderColorProperty); }
            set { SetValue(ItemBorderColorProperty, value); }
        }
        public static readonly DependencyProperty ItemBorderColorProperty = DependencyProperty.Register(
            name: "ItemBorderColor",
            propertyType: typeof(SolidColorBrush),
            ownerType: typeof(cImagePreviewer),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Red), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public SolidColorBrush ItemTextColor
        {
            get { return (SolidColorBrush)GetValue(ItemTextColorProperty); }
            set { SetValue(ItemTextColorProperty, value); }
        }
        public static readonly DependencyProperty ItemTextColorProperty = DependencyProperty.Register(
            name: "ItemTextColor",
            propertyType: typeof(SolidColorBrush),
            ownerType: typeof(cImagePreviewer),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );



        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            name: "BackgroundColor",
            propertyType: typeof(SolidColorBrush),
            ownerType: typeof(cImagePreviewer),
            typeMetadata: new FrameworkPropertyMetadata(new SolidColorBrush(Colors.DarkGray), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    }
}
