using System;
using log4net.Appender;
using log4net.Core;

namespace SerialPortSticky
{
    public class UiLogEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public UiLogEventArgs(string message)
        {
            Message = message;
        }
    }

    public class UiLogAppender : AppenderSkeleton
    {
        public event EventHandler<UiLogEventArgs> UiLogReceived;

        protected override void Append(LoggingEvent loggingEvent)
        {
            var message = RenderLoggingEvent(loggingEvent);
            OnUiLogReceived(new UiLogEventArgs(message));
        }

        protected virtual void OnUiLogReceived(UiLogEventArgs e)
        {
            UiLogReceived?.Invoke(this, e);
        }
    }
}
