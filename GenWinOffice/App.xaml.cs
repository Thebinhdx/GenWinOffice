using System.Windows;
using Wpf.Ui.Appearance;

namespace GenWinOffice;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Nạp ngôn ngữ mặc định
            LanguageManager.ChangeLanguage("en-US");

            // Tạo và hiện cửa sổ chính trực tiếp
            MainWindow main = new MainWindow();
            main.Show();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Lỗi khởi chạy: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Hàm dùng chung để đổi Theme cho cả hệ thống
    public static void ApplyTheme(string themeMode, Window? window = null)
    {
        // CỰC KỲ QUAN TRỌNG: Chỉ Unwatch/Watch khi Window đã load xong để chống Crash
        bool isSafeToWatch = window != null && window.IsLoaded;

        if (themeMode == "Light")
        {
            if (isSafeToWatch) Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(window);
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);
        }
        else if (themeMode == "Dark")
        {
            if (isSafeToWatch) Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(window);
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
        }
        else // SystemDefault
        {
            if (isSafeToWatch) Wpf.Ui.Appearance.SystemThemeWatcher.Watch(window);
            Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();
        }

        // 2. Chốt lại theme thực tế
        string actualTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Light ? "Light" : "Dark";

        // 3. Xóa CustomColors cũ
        var dictionaries = Current.Resources.MergedDictionaries;
        var oldDicts = dictionaries.Where(d => d.Source != null && d.Source.ToString().Contains("CustomColors")).ToList();

        foreach (var dict in oldDicts)
        {
            dictionaries.Remove(dict);
        }

        // 4. Nạp CustomColors mới
        string newDictPath = actualTheme == "Light"
            ? "pack://application:,,,/Resources/CustomColorsLight.xaml"
            : "pack://application:,,,/Resources/CustomColorsDark.xaml";

        dictionaries.Add(new ResourceDictionary { Source = new Uri(newDictPath, UriKind.Absolute) });
    }
}