using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Appearance;

namespace GenWinOffice;

public partial class SettingsPage : Page
{
	public SettingsPage()
	{
		InitializeComponent();
		base.Loaded += SettingsPage_Loaded;

        ThemeComboBox.SelectionChanged -= ThemeComboBox_SelectionChanged;

        // 2. Đọc cài đặt đã lưu và gán cho UI
        string savedTheme = GenWinOffice.SettingsColor.Default.ThemeMode;
        if (savedTheme == "Light") ThemeComboBox.SelectedIndex = 1;
        else if (savedTheme == "Dark") ThemeComboBox.SelectedIndex = 2;
        else ThemeComboBox.SelectedIndex = 0;

        // 3. Bật lại sự kiện
        ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
    }

	private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
	{
		foreach (ComboBoxItem item in (IEnumerable)LanguageComboBox.Items)
		{
			if (item.Tag?.ToString() == LanguageManager.CurrentLanguage)
			{
				LanguageComboBox.SelectedItem = item;
				break;
			}
		}
	}

	private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (base.IsLoaded && LanguageComboBox.SelectedItem is ComboBoxItem { Tag: var tag })
		{
			string text = tag?.ToString() ?? "en-US";
			if (text != LanguageManager.CurrentLanguage)
			{
				LanguageManager.ChangeLanguage(text);
			}
		}
	}
    private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ThemeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
        {
            string selectedTheme = item.Tag?.ToString() ?? "SystemDefault";

            // Lưu vĩnh viễn vào hệ thống (Nhớ cài đặt)
            GenWinOffice.SettingsColor.Default.ThemeMode = selectedTheme;
            GenWinOffice.SettingsColor.Default.Save();

            // Gọi hàm từ App.xaml.cs
            App.ApplyTheme(selectedTheme, Window.GetWindow(this));
        }
    }

    private void ApplyTheme(string theme)
    {
        var parentWindow = Window.GetWindow(this);

        // 1. Đổi theme của WPF-UI
        if (theme == "Light")
        {
            if (parentWindow != null) Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(parentWindow);
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);
        }
        else if (theme == "Dark")
        {
            if (parentWindow != null) Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(parentWindow);
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
        }
        else // SystemDefault
        {
            if (parentWindow != null) Wpf.Ui.Appearance.SystemThemeWatcher.Watch(parentWindow);
            Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();

            // Xác định Windows đang dùng Sáng hay Tối để nạp file Custom cho đúng
            theme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Light ? "Light" : "Dark";
        }

        // 2. Hoán đổi file CustomColors
        UpdateCustomDictionary(theme);
    }

    private void UpdateCustomDictionary(string theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;

        // Tìm file CustomColors đang nạp hiện tại và xóa nó
        var oldDict = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("CustomColors"));
        if (oldDict != null)
        {
            dictionaries.Remove(oldDict);
        }

        // Nạp file mới dựa theo theme
        string newDictPath = theme == "Light"
        ? "pack://application:,,,/Resources/CustomColorsLight.xaml"
        : "pack://application:,,,/Resources/CustomColorsDark.xaml";
        dictionaries.Add(new ResourceDictionary { Source = new Uri(newDictPath, UriKind.Absolute) });
    }
}
