using System;
using System.Linq;
using System.Windows;

namespace GenWinOffice;

public static class LanguageManager
{
	public static string CurrentLanguage { get; private set; } = "en-US";

	public static void ChangeLanguage(string cultureCode)
	{
		CurrentLanguage = cultureCode;
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		if (cultureCode == "vi-VN")
		{
			resourceDictionary.Source = new Uri("Resources/Strings.vi-VN.xaml", UriKind.Relative);
		}
		else
		{
			resourceDictionary.Source = new Uri("Resources/Strings.en-US.xaml", UriKind.Relative);
		}
		ResourceDictionary? resourceDictionary2 = Application.Current.Resources.MergedDictionaries.FirstOrDefault((ResourceDictionary d) => d.Source != null && d.Source.OriginalString.Contains("Strings."));
		if (resourceDictionary2 != null)
		{
			Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary2);
		}
		Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
	}
}
