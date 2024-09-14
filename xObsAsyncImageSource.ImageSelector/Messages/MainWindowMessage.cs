using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows;
using System.Windows.Input;

namespace xObsAsyncImageSource.ImageSelector.Messages
{
    public class WindowActivateMessage : ValueChangedMessage<string>
    {
        public WindowActivateMessage(string value) : base(value)
        {
        }
    }

    public class WindowCloseMessage : ValueChangedMessage<string>
    {
        public WindowCloseMessage(string value) : base(value)
        {
        }
    }

    public class WindowClosingMessage : ValueChangedMessage<string>
    {
        public WindowClosingMessage(string value) : base(value)
        {
        }
    }

    public class WindowKeyDownMessage : ValueChangedMessage<Key>
    {
        public WindowKeyDownMessage(Key value) : base(value)
        {
        }
    }

    public class WindowMaximizeMessage : ValueChangedMessage<string>
    {
        public WindowMaximizeMessage(string value) : base(value)
        {
        }
    }

    public class WindowMinimizeMessage : ValueChangedMessage<string>
    {
        public WindowMinimizeMessage(string value) : base(value)
        {
        }
    }

    public class WindowPosResetMessage : ValueChangedMessage<Vector?>
    {
        public WindowPosResetMessage(Vector? value) : base(value)
        {
        }
    }

    public class WindowShowHideMessage : ValueChangedMessage<string>
    {
        public WindowShowHideMessage(string value) : base(value)
        {
        }
    }

    public class WindowMoveTo1stMonitorMessage : ValueChangedMessage<Vector?>
    {
        public WindowMoveTo1stMonitorMessage(Vector? value) : base(value)
        {
        }
    }

    public class WindowMoveTo2ndMonitorMessage : ValueChangedMessage<Vector?>
    {
        public WindowMoveTo2ndMonitorMessage(Vector? value) : base(value)
        {
        }
    }

    public class WindowSaveToTransparentPngMessage : ValueChangedMessage<string>
    {
        public WindowSaveToTransparentPngMessage(string value) : base(value)
        {
        }
    }
}
