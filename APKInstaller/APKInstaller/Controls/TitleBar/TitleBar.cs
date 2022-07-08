using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace APKInstaller.Controls
{
    [ContentProperty("CustomContent")]
    [TemplatePart(Name = "LayoutRoot", Type = typeof(Grid))]
    [TemplatePart(Name = "TitleText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "CustomContentPresenter", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "DragRegion", Type = typeof(Grid))]
    [TemplatePart(Name = "BackButton", Type = typeof(Button))]
    [TemplatePart(Name = "Icon", Type = typeof(Viewbox))]
    public partial class TitleBar : Control
    {
        private Grid m_layoutRoot;
        private TextBlock m_titleTextBlock;
        private FrameworkElement m_customArea;
        private Viewbox m_icon;

        private readonly bool m_isTitleSquished = false;
        private readonly bool m_isIconSquished = false;

        private readonly double m_titleWidth;
        private readonly double m_iconWidth;

        static TitleBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBar), new FrameworkPropertyMetadata(typeof(TitleBar)));
        }

        public TitleBar()
        {
            SetValue(TemplateSettingsPropertyKey, new TitleBarTemplateSettings());
        }

        public override void OnApplyTemplate()
        {
            m_layoutRoot = (Grid)GetTemplateChild("LayoutRoot");

            m_icon = (Viewbox)GetTemplateChild("Icon");
            m_titleTextBlock = (TextBlock)GetTemplateChild("TitleText");
            m_customArea = (FrameworkElement)GetTemplateChild("CustomContentPresenter");

            Button backButton = (Button)GetTemplateChild("BackButton");
            if (backButton != null)
            {
                backButton.Click += OnBackButtonClick;
            }

            Button refreshButton = (Button)GetTemplateChild("RefreshButton");
            if (refreshButton != null)
            {
                refreshButton.Click += OnRefreshButtonClick;
            }

            UpdateHeight();
            UpdateBackButton();
            UpdateIcon();
            UpdateTitle();
            UpdateRefreshButton();

            base.OnApplyTemplate();
        }

        public void SetProgressValue(double value)
        {
            TitleBarTemplateSettings templateSettings = TemplateSettings;
            templateSettings.ProgressValue = value;
            templateSettings.IsProgressIndeterminate = false;
        }

        public void ShowProgressRing()
        {
            TitleBarTemplateSettings templateSettings = TemplateSettings;
            templateSettings.IsProgressActive = true;
            templateSettings.IsProgressIndeterminate = true;
            VisualStateManager.GoToState(this, "ProgressVisible", false);
        }

        public void HideProgressRing()
        {
            TitleBarTemplateSettings templateSettings = TemplateSettings;
            VisualStateManager.GoToState(this, "ProgressCollapsed", false);
            templateSettings.IsProgressActive = false;
        }

        public void OnBackButtonClick(object sender, RoutedEventArgs args)
        {
            BackRequested?.Invoke(this, null);
        }

        public void OnRefreshButtonClick(object sender, RoutedEventArgs args)
        {
            RefreshRequested?.Invoke(this, null);
        }

        public void OnIconSourcePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateIcon();
        }

        public void OnIsBackButtonVisiblePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateBackButton();
        }

        public void OnIsRefreshButtonVisiblePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRefreshButton();
        }

        public void OnCustomContentPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateHeight();
        }

        public void OnTitlePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateTitle();
        }

        public void UpdateBackButton()
        {
            VisualStateManager.GoToState(this, IsBackButtonVisible ? "BackButtonVisible" : "BackButtonCollapsed", false);
        }

        public void UpdateRefreshButton()
        {
            VisualStateManager.GoToState(this, IsRefreshButtonVisible ? "RefreshButtonVisible" : "RefreshButtonCollapsed", false);
        }

        public void UpdateHeight()
        {
            VisualStateManager.GoToState(this, (CustomContent == null && AutoSuggestBox == null && PaneFooter == null) ? "CompactHeight" : "ExpandedHeight", false);
        }

        public void UpdateIcon()
        {
            UIElement source = IconSource;
            if (source != null)
            {
                VisualStateManager.GoToState(this, "IconVisible", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "IconCollapsed", false);
            }
        }

        public void UpdateTitle()
        {
            string titleText = Title;
            if (string.IsNullOrEmpty(titleText))
            {
                VisualStateManager.GoToState(this, "TitleTextCollapsed", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "TitleTextVisible", false);
            }
        }
    }
}
