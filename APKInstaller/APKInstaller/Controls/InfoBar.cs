using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APKInstaller.Controls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:APKInstaller.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:APKInstaller.Controls;assembly=APKInstaller.Controls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:InfoBar/>
    ///
    /// </summary>
    public class InfoBar : ContentControl
    {
        private InfoBar _infoBar;

        public InfoBar()
        {
            DefaultStyleKey = typeof(InfoBar);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
           "Title",
           typeof(string),
           typeof(InfoBar),
           new PropertyMetadata(default(string), OnTitleChanged));

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
           "IsOpen",
           typeof(bool),
           typeof(InfoBar),
           new PropertyMetadata(true, OnIsOpenChanged));

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
           "Message",
           typeof(string),
           typeof(InfoBar),
           new PropertyMetadata(default(string), OnMessageChanged));

        public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
           "Severity",
           typeof(InfoBarSeverity),
           typeof(InfoBar),
           new PropertyMetadata(InfoBarSeverity.Informational, OnSeverityChanged));

        public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
           "IsClosable",
           typeof(bool),
           typeof(InfoBar),
           new PropertyMetadata(false, OnIsClosableChanged));

        public static readonly DependencyProperty ActionButtonProperty = DependencyProperty.Register(
            "ActionButton",
            typeof(object),
            typeof(InfoBar),
            null);

        [Localizable(true)]
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        [Localizable(true)]
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsClosable
        {
            get => (bool)GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        public object ActionButton
        {
            get => GetValue(ActionButtonProperty);
            set => SetValue(ActionButtonProperty, value);
        }

        [Localizable(true)]
        public InfoBarSeverity Severity
        {
            get => (InfoBarSeverity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }

        public override void OnApplyTemplate()
        {
            IsEnabledChanged -= InfoBar_IsEnabledChanged;
            _infoBar = this;
            Update();
            SetEnabledState();
            IsEnabledChanged += InfoBar_IsEnabledChanged;
            base.OnApplyTemplate();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).Update();
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).Update();
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).Update();
        }

        private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).Update();
        }

        private static void OnIsClosableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InfoBar)d).Update();
        }

        private void InfoBar_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetEnabledState();
        }

        private void SetEnabledState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        private void Update()
        {
            if (_infoBar == null)
            {
                return;
            }

            if (_infoBar.ActionButton != null)
            {
                if (_infoBar.ActionButton.GetType() != typeof(Button))
                {
                    // We do not want to override the default AutomationProperties.Name of a button. Its Content property already describes what it does.
                    if (!string.IsNullOrEmpty(_infoBar.Title))
                    {
                        AutomationProperties.SetName((UIElement)_infoBar.ActionButton, _infoBar.Title);
                    }
                }
            }

            if (_infoBar.IsOpen)
            {
                _infoBar.Visibility = Visibility.Visible;
            }
            else
            {
                _infoBar.Visibility = Visibility.Collapsed;
            }
        }
    }

    public enum InfoBarSeverity
    {
        Informational = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }
}
