using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GenWinOffice;

public partial class SettingsPage : Page
{
	public SettingsPage()
	{
		InitializeComponent();
		base.Loaded += SettingsPage_Loaded;
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
}
