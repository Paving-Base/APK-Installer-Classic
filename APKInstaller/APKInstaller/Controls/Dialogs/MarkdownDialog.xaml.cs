using APKInstaller.Helpers;
using ModernWpf.Controls;
using System.ComponentModel;
using System.Net.Http;

namespace APKInstaller.Controls.Dialogs
{
    /// <summary>
    /// MarkdownDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MarkdownDialog : ContentDialog, INotifyPropertyChanged
    {
        private bool isInitialized;
        internal bool IsInitialized
        {
            get => isInitialized;
            set
            {
                isInitialized = value;
                RaisePropertyChangedEvent();
            }
        }

        public string ContentUrl
        {
            set
            {
                this.RunOnUIThread(async () =>
                {
                    if (string.IsNullOrEmpty(value)) { return; }
                    IsInitialized = false;
                    value = value.StartsWith("http") ? value : $"https://{value}";
                    using HttpClient client = new HttpClient();
                    try
                    {
                        MarkdownText.Text = await client.GetStringAsync(value);
                        Title = string.Empty;
                    }
                    catch
                    {
                        if (value.Contains("raw.githubusercontent.com"))
                        {
                            try
                            {
                                MarkdownText.Text = (await client.GetStringAsync(value.Replace("raw.githubusercontent.com", "raw.fastgit.org"))).Replace("raw.githubusercontent.com", "raw.fastgit.org");
                                Title = string.Empty;
                            }
                            catch
                            {
                                MarkdownText.Text = value;
                            }
                        }
                        else
                        {
                            MarkdownText.Text = value;
                        }
                    }
                    IsInitialized = true;
                });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public MarkdownDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        //private void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        //{
        //    _ = Launcher.LaunchUriAsync(new Uri(e.Link));
        //}
    }
}
