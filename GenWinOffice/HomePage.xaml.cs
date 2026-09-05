using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace GenWinOffice;

public partial class HomePage : Page
{
	public HomePage()
	{
		InitializeComponent();
	}

	[SupportedOSPlatform("windows7.0")]
	private void BtnGoToScan_Click(object sender, RoutedEventArgs e)
	{
		if (Application.Current.MainWindow is MainWindow mainWindow)
		{
			mainWindow.RootNavigation.Navigate(typeof(ScanPage));
		}
	}

	private void BtnGithub_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = "https://github.com/Thebinhdx/GenWinOffice",
				UseShellExecute = true
			});
		}
		catch
		{
		}
	}

	private void Page_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (MainScrollViewer != null)
		{
			if (e.Delta < 0)
			{
				MainScrollViewer.LineDown();
				MainScrollViewer.LineDown();
				MainScrollViewer.LineDown();
			}
			else
			{
				MainScrollViewer.LineUp();
				MainScrollViewer.LineUp();
				MainScrollViewer.LineUp();
			}
			e.Handled = true;
		}
	}
}
