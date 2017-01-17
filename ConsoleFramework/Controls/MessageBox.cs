using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using System.ComponentModel;

namespace ConsoleFramework.Controls
{
    public delegate void MessageBoxClosedEventHandler(MessageBoxResult result);

    public class MessageBox : Window
    {
        private readonly TextBlock textBlock;
        private MessageBoxResult result = MessageBoxResult.Button1;
        private string[] _astrBtnCaptions = new string[] { "Yes", "No", "Cancel" };
        private int      _nNbOfBtnsToShow = 1;

        void SetButtonCaptionsFromMessageBoxButtons(MessageBoxButtons buttons)
        {
            switch(buttons)
            {
                case MessageBoxButtons.OK:
                    _astrBtnCaptions[0] = "OK";
                    _nNbOfBtnsToShow    = 1;
                    break;
                case MessageBoxButtons.OKCancel:
                    _astrBtnCaptions[0] = "OK";
                    _astrBtnCaptions[1] = "Cancel";
                    _nNbOfBtnsToShow    = 2;
                    break;
                case MessageBoxButtons.RetryCancel:
                    _astrBtnCaptions[0] = "Retry";
                    _astrBtnCaptions[1] = "Cancel";
                    _nNbOfBtnsToShow    = 2;
                    break;
                case MessageBoxButtons.YesNo:
                    _astrBtnCaptions[0] = "Yes";
                    _astrBtnCaptions[1] = "No";
                    _nNbOfBtnsToShow    = 2;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    _astrBtnCaptions[0] = "Yes";
                    _astrBtnCaptions[1] = "No";
                    _astrBtnCaptions[2] = "Cancel";
                    _nNbOfBtnsToShow    = 3;
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    _astrBtnCaptions[0] = "Abort";
                    _astrBtnCaptions[1] = "Retry";
                    _astrBtnCaptions[2] = "Ignore";
                    _nNbOfBtnsToShow    = 3;
                    break;
            }
        }

        MessageBox(MessageBoxButtons buttons)
        {
            Panel panel = new Panel();
            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(1);
            panel.XChildren.Add(textBlock);

            SetButtonCaptionsFromMessageBoxButtons(buttons);
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(GridUnitType.Star, 1) });
            for (int i = 0; i < _nNbOfBtnsToShow; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(GridUnitType.Star, 1) });
            }
            for (int i = 0; i < _nNbOfBtnsToShow; i++)
            {
                Button button = new Button();
                button.Margin = new Thickness(4, 0, 4, 0);
                button.HorizontalAlignment = HorizontalAlignment.Center;
                button.Caption = _astrBtnCaptions[i];
                button.OnClick += (o, e) => { result = (MessageBoxResult.Button1 + i); this.Close(); };
                grid.Controls.Add(button);
            }
            panel.XChildren.Add(grid);
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            this.Content = panel;
        }

        public string Text
        {
            get { return textBlock.Text; }
            set { textBlock.Text = value; }
        }

        public static void Show(string title, string text, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxClosedEventHandler onClosed = null)
        {
            Control rootControl = ConsoleApplication.Instance.RootControl;
            if (!(rootControl is WindowsHost))
                throw new InvalidOperationException("Default windows host not found, create MessageBox manually");
            WindowsHost windowsHost = (WindowsHost)rootControl;
            MessageBox messageBox = new MessageBox(buttons);
            messageBox.Title = title;
            messageBox.Text = text;
            messageBox.AddHandler(ClosedEvent, new EventHandler((sender, args) =>
            {
                if (null != onClosed)
                {
                    onClosed(MessageBoxResult.Button1);
                }
            }));
            //messageBox.X =
            windowsHost.ShowModal(messageBox);
        }
    }

    public enum MessageBoxResult
    {
        Button1,
        Button2,
        Button3
    }

    public enum MessageBoxButtons
    {
        [Description("The message box contains Abort, Retry, and Ignore buttons.")]
        AbortRetryIgnore,
        [Description("The message box contains an OK button.")]
        OK,
        [Description("The message box contains OK and Cancel buttons.")]
        OKCancel,
        [Description("The message box contains Retry and Cancel buttons.")]
        RetryCancel,
        [Description("The message box contains Yes and No buttons.")]
        YesNo,
        [Description("The message box contains Yes, No, and Cancel buttons.")]
        YesNoCancel,
    }
}
