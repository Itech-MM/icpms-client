using System;
using System.ComponentModel;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using icpms_client.Common.UI;

namespace icpms_client.Utils.UI.Dialog.MessageDialog
{
    public partial class StyleMessageBox : INotifyPropertyChanged
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
        
        private MessageType _messageType;
        private BaseWindow? _messageBoxOwner;
        
        public StyleMessageBox(BaseWindow owner, string message, string caption, 
                            MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();
            Owner = owner;
            _messageBoxOwner = owner;
            TitleText.Text = caption;
            MessageText.Text = message;
            SetMessageType(icon);
            CreateButtons(buttons);
            
            Loaded += (_, _) => PlaySystemSound();
        }
        
        private void PlaySystemSound()
        {
            try
            {
                switch (_messageType)
                {
                    case MessageType.Error: SystemSounds.Hand.Play(); break;
                    case MessageType.Question: SystemSounds.Question.Play(); break;
                    case MessageType.Warning: SystemSounds.Exclamation.Play(); break;
                    case MessageType.Information: SystemSounds.Asterisk.Play(); break;
                    default: SystemSounds.Beep.Play(); break;
                }
            }
            catch
            {
                SystemSounds.Beep.Play();
            }
        }

        private void SetMessageType(MessageBoxImage icon)
        {
            // Set message type based on icon
            _messageType = icon switch
            {
                MessageBoxImage.Error => MessageType.Error,
                MessageBoxImage.Question => MessageType.Question,
                MessageBoxImage.Warning => MessageType.Warning,
                MessageBoxImage.Information => MessageType.Information,
                _ => MessageType.Information
            };
            
            // Notify UI about the change
            OnPropertyChanged(nameof(MessageType));
        }

        private void CreateButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, true);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, true);
                    AddButton("Cancel", MessageBoxResult.Cancel);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Yes", MessageBoxResult.Yes, true);
                    AddButton("No", MessageBoxResult.No);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Yes", MessageBoxResult.Yes, true);
                    AddButton("No", MessageBoxResult.No);
                    AddButton("Cancel", MessageBoxResult.Cancel);
                    break;
            }
        }

        private void AddButton(string text, MessageBoxResult result, bool isPrimary = false)
        {
            var button = new Button
            {
                Content = text,
                // Add button type to tag for hover styling
                Tag = isPrimary ? "Primary" : text,
                Style = (Style)FindResource("MessageBoxButton")
            };

            button.Click += (_, _) => 
            {
                Result = result;
                DialogResult = true;
                Close();
            };

            ButtonsPanel.Children.Add(button);
            
            // Set focus to primary button
            if (isPrimary)
            {
                button.Focus();
            }
        }

        public static MessageBoxResult Show(
            BaseWindow owner,
            string message,
            string caption,
            MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            var dialog = new StyleMessageBox(owner, message, caption, buttons, icon);
            owner.ApplyBlur(true);
            dialog.ShowDialog();
            return dialog.Result;
        }

        protected override void OnClosed(EventArgs e)
        {
            _messageBoxOwner?.ApplyBlur(false);
            base.OnClosed(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Properties

        public MessageType MessageType
        {
            get => _messageType;
            set
            {
                if (_messageType == value) return;
                _messageType = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}