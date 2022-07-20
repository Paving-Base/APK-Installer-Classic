using APKInstaller.Helpers;
using APKInstaller.Models;
using ModernWpf;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace APKInstaller.Controls
{
    /// <summary>
    /// WebXAML.xaml 的交互逻辑
    /// </summary>
    public partial class WebXAML : UserControl
    {
        public static readonly DependencyProperty ContentInfoProperty = DependencyProperty.Register(
           "ContentInfo",
           typeof(GitInfo),
           typeof(WebXAML),
           new PropertyMetadata(default(GitInfo), OnContentChanged));

        public GitInfo ContentInfo
        {
            get => (GitInfo)GetValue(ContentInfoProperty);
            set => SetValue(ContentInfoProperty, value);
        }

        public static readonly DependencyProperty ContentUrlProperty = DependencyProperty.Register(
           "ContentUrl",
           typeof(Uri),
           typeof(WebXAML),
           new PropertyMetadata(default(Uri), OnContentChanged));

        public Uri ContentUrl
        {
            get => (Uri)GetValue(ContentUrlProperty);
            set => SetValue(ContentUrlProperty, value);
        }

        public static readonly DependencyProperty ContentXAMLProperty = DependencyProperty.Register(
           "ContentXAML",
           typeof(string),
           typeof(WebXAML),
           new PropertyMetadata(default(string), OnContentChanged));

        public string ContentXAML
        {
            get => (string)GetValue(ContentXAMLProperty);
            set => SetValue(ContentXAMLProperty, value);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as WebXAML).UpdateContent(e.NewValue);

        public WebXAML() => InitializeComponent();

        private async void UpdateContent(object Content)
        {
            await Task.Run(async () =>
            {
                if (Content == null) { return; }
                if (Content is GitInfo ContentInfo && ContentInfo != default(GitInfo))
                {
                    string value = ContentInfo.FormatURL(GitInfo.GITHUB_API);
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) { return; }
                    using (HttpClient client = new HttpClient())
                    {
                        UIElement UIElement = null;
                        try
                        {
                            Stream xaml = await client.GetStreamAsync(value);
                            UIElement = (UIElement)await this.ExecuteOnUIThreadAsync(() => { return XamlReader.Load(xaml); });
                        }
                        catch
                        {
                            try
                            {
                                Stream xaml = await client.GetStreamAsync(ContentInfo.FormatURL(GitInfo.FASTGIT_API));
                                UIElement = (UIElement)await this.ExecuteOnUIThreadAsync(() => { return XamlReader.Load(xaml); });
                            }
                            catch
                            {
                                try
                                {
                                    Stream xaml = await client.GetStreamAsync(ContentInfo.FormatURL(GitInfo.JSDELIVR_API));
                                    UIElement = (UIElement)await this.ExecuteOnUIThreadAsync(() => { return XamlReader.Load(xaml); });
                                }
                                catch
                                {
                                    UIElement = null;
                                }
                            }
                        }
                        finally
                        {
                            if (UIElement != null)
                            {
                                this.RunOnUIThread(() => { this.Content = UIElement; });
                            }
                        }
                    }
                }
                else if (Content is string ContentXAML && ContentXAML != default)
                {
                    UIElement UIElement = null;
                    try
                    {
                        UIElement = (UIElement)await this.ExecuteOnUIThreadAsync(() => { return XamlReader.Parse(ContentXAML); });
                    }
                    catch
                    {
                        UIElement = null;
                    }
                    finally
                    {
                        if (UIElement != null)
                        {
                            this.RunOnUIThread(() => { this.Content = UIElement; });
                        }
                    }
                }
                else if (Content is Uri ContentUri && ContentUri != default)
                {
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) { return; }
                    using (HttpClient client = new HttpClient())
                    {
                        UIElement UIElement = null;
                        try
                        {
                            Stream xaml = await client.GetStreamAsync(ContentUri);
                            UIElement = (UIElement)await this.ExecuteOnUIThreadAsync(() => { return XamlReader.Load(xaml); });
                        }
                        catch
                        {
                            try
                            {
                                Stream xaml = await client.GetStreamAsync(ContentUri.ToString().Replace("://raw.githubusercontent.com", "://raw.fastgit.org"));
                                UIElement = (UIElement)await this.ExecuteOnUIThreadAsync(() => { return XamlReader.Load(xaml); });
                            }
                            catch
                            {
                                UIElement = null;
                            }
                        }
                        finally
                        {
                            if (UIElement != null)
                            {
                                this.RunOnUIThread(() => { this.Content = UIElement; });
                            }
                        }
                    }
                }
            });
        }
    }
}
