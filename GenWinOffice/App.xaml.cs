using System.Windows;

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
}