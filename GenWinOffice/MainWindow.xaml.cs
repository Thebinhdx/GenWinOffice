using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace GenWinOffice;

public partial class MainWindow : FluentWindow
{
	public MainWindow()
	{
        InitializeComponent();
		                                                                              
        // 1. Đọc cài đặt đã lưu
        string savedTheme = string.IsNullOrEmpty(GenWinOffice.SettingsColor.Default.ThemeMode)
            ? "SystemDefault"
            : GenWinOffice.SettingsColor.Default.ThemeMode;

        // 2. Áp dụng màu (Bắt buộc phải truyền 'this' để Window có màu nền, không bị trắng)
        App.ApplyTheme(savedTheme, this);

        // 3. Đợi giao diện sẵn sàng mới Load trang chủ và bật Watcher
        this.Loaded += (s, e) =>
        {
            // PHỤC HỒI DÒNG NÀY ĐỂ HIỂN THỊ GIAO DIỆN CHÍNH
            RootNavigation.Navigate(typeof(HomePage));

            // Bật theo dõi màu Windows nếu dùng SystemDefault
            if (savedTheme == "SystemDefault")
            {
                Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
            }
        };

    }

	private void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		RootNavigation.Navigate(typeof(HomePage));
	}

	private void PaneToggleButton_Click(object sender, RoutedEventArgs e)
	{
		RootNavigation.IsPaneOpen = !RootNavigation.IsPaneOpen;
	}

	private void FluentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (e.NewSize.Width < 1150.0)
		{
			RootNavigation.IsPaneOpen = false;
		}
		else
		{
			RootNavigation.IsPaneOpen = true;
		}
	}
}
