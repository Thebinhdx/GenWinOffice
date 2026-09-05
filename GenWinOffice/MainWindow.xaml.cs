using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Controls;

namespace GenWinOffice;

public partial class MainWindow : FluentWindow
{
	public MainWindow()
	{
		InitializeComponent();
		base.Loaded += MainWindow_Loaded;
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
