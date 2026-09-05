using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using GenWinOffice.Core;
using Microsoft.Win32;

namespace GenWinOffice;

public partial class ScanPage : Page
{
	public ScanPage()
	{
		InitializeComponent();
	}

	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		if (ActivationScanner.LastResult != null && ActivationScanner.LastResult.IsScanned)
		{
			ApplyScanResults(ActivationScanner.LastResult);
		}
		else
		{
			ApplyScanResults(null);
		}
	}

	private async void BtnStartScan_Click(object sender, RoutedEventArgs e)
	{
		BtnStartScan.IsEnabled = false;
		BtnExport.IsEnabled = false;
		ScanProgressBar.Visibility = Visibility.Visible;
		ApplyScanResults(await ActivationScanner.RunDeepScanAsync());
		ScanProgressBar.Visibility = Visibility.Collapsed;
		BtnStartScan.IsEnabled = true;
	}

	private void ApplyScanResults(ScanResult? result)
	{
		if (result == null || !result.IsScanned)
		{
			BtnExport.IsEnabled = false;
			TxtWindowsDetail.Text = GetLang("Scan_NotScanned");
			TxtWindowsStatus.Text = GetLang("Scan_StatusPending");
			SetBadgeColor(BadgeWindows, TxtWindowsStatus, "#2D2D3D", "#A0A0A0");
			TxtOfficeDetail.Text = GetLang("Scan_NotScanned");
			TxtOfficeStatus.Text = GetLang("Scan_StatusPending");
			SetBadgeColor(BadgeOffice, TxtOfficeStatus, "#2D2D3D", "#A0A0A0");
			TxtMethodDetail.Text = GetLang("Scan_NotScanned");
			TxtMethodStatus.Text = GetLang("Scan_MethodPending");
			SetBadgeColor(BadgeMethod, TxtMethodStatus, "#2D2D3D", "#A0A0A0");
			TxtConclusionTitle.Text = GetLang("Scan_ConclusionTitle");
			TxtConclusionDetail.Text = GetLang("Scan_ConclusionDefaultDetail");
			TxtConclusionStatus.Text = GetLang("Scan_ConclusionPending");
			SetBadgeColor(BadgeConclusion, TxtConclusionStatus, "#2D2D3D", "#A0A0A0");
			CardConclusion.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D3D5C"));
			return;
		}
		BtnExport.IsEnabled = true;
		bool num = result.IsKmsDetected || result.IsKms38Detected || result.IsHwidDetected;
		bool flag = result.SuspiciousFilesDetected || result.SuspiciousTasksDetected || result.RegistryTampered || result.HistoryTampered;
		if (num)
		{
			TxtWindowsDetail.Text = GetLang("Scan_WinCrackDetail");
			TxtWindowsStatus.Text = GetLang("Scan_WinCrackStatus");
			SetBadgeColor(BadgeWindows, TxtWindowsStatus, "#381E1E", "#F44336");
		}
		else if (flag)
		{
			TxtWindowsDetail.Text = GetLang("Scan_WinWarningDetail");
			TxtWindowsStatus.Text = GetLang("Scan_WinWarningStatus");
			SetBadgeColor(BadgeWindows, TxtWindowsStatus, "#382B1E", "#FFC107");
		}
		else
		{
			TxtWindowsDetail.Text = GetLang("Scan_WinCleanDetail");
			TxtWindowsStatus.Text = GetLang("Scan_WinCleanStatus");
			SetBadgeColor(BadgeWindows, TxtWindowsStatus, "#1E382B", "#4CAF50");
		}
		if (!result.OfficeInstalled)
		{
			TxtOfficeDetail.Text = GetLang("Scan_OfficeNotInstalledDetail");
			TxtOfficeStatus.Text = GetLang("Scan_OfficeNotInstalledStatus");
			SetBadgeColor(BadgeOffice, TxtOfficeStatus, "#2D2D3D", "#A0A0A0");
		}
		else if (result.OfficeKmsDetected || result.OfficeOhookDetected)
		{
			TxtOfficeDetail.Text = GetLang("Scan_OfficeCrackDetail");
			TxtOfficeStatus.Text = GetLang("Scan_OfficeCrackStatus");
			SetBadgeColor(BadgeOffice, TxtOfficeStatus, "#381E1E", "#F44336");
		}
		else
		{
			TxtOfficeDetail.Text = GetLang("Scan_OfficeCleanDetail");
			TxtOfficeStatus.Text = GetLang("Scan_WinCleanStatus");
			SetBadgeColor(BadgeOffice, TxtOfficeStatus, "#1E382B", "#4CAF50");
		}
		List<string> list = new List<string>();
		if (result.IsHwidDetected)
		{
			list.Add("• HWID / MAS (Digital License Bypass)");
		}
		if (result.IsKms38Detected)
		{
			list.Add("• KMS38 (Extended License to 2038)");
		}
		if (result.IsKmsDetected)
		{
			list.Add("• Online KMS (Untrusted KMS Server)");
		}
		if (result.OfficeOhookDetected)
		{
			list.Add("• Ohook (Office License Hook DLL)");
		}
		if (result.OfficeKmsDetected)
		{
			list.Add("• Office KMS (Emulated KMS Server)");
		}
		if (result.RegistryTampered)
		{
			list.Add("• Registry Tampering (SPP Config Modified)");
		}
		if (result.SuspiciousFilesDetected || result.SuspiciousTasksDetected)
		{
			list.Add("• Crack Tools (KMSpico, KMSAuto, HEU KMS...)");
		}
		if (result.HistoryTampered)
		{
			list.Add("• PowerShell History (Activation Script Traces)");
		}
		if (list.Count > 0)
		{
			TxtMethodDetail.Text = string.Join("\n", list);
			TxtMethodStatus.Text = $"{list.Count} Traces";
			SetBadgeColor(BadgeMethod, TxtMethodStatus, "#381E1E", "#F44336");
		}
		else
		{
			TxtMethodDetail.Text = GetLang("Scan_MethodCleanDetail");
			TxtMethodStatus.Text = GetLang("Scan_MethodCleanStatus");
			SetBadgeColor(BadgeMethod, TxtMethodStatus, "#1E382B", "#4CAF50");
		}
		if (list.Count == 0)
		{
			TxtConclusionTitle.Text = GetLang("Scan_ConclusionSafeTitle");
			TxtConclusionDetail.Text = GetLang("Scan_ConclusionSafeDetail");
			TxtConclusionStatus.Text = GetLang("Scan_ConclusionPass");
			SetBadgeColor(BadgeConclusion, TxtConclusionStatus, "#1E382B", "#4CAF50");
			CardConclusion.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E382B"));
		}
		else
		{
			TxtConclusionTitle.Text = GetLang("Scan_ConclusionRiskTitle");
			TxtConclusionDetail.Text = GetLang("Scan_ConclusionRiskDetail");
			TxtConclusionStatus.Text = GetLang("Scan_ConclusionRisk");
			SetBadgeColor(BadgeConclusion, TxtConclusionStatus, "#381E1E", "#F44336");
			CardConclusion.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#381E1E"));
		}
	}

	private void BtnExport_Click(object sender, RoutedEventArgs e)
	{
		if (ActivationScanner.LastResult == null || !ActivationScanner.LastResult.IsScanned)
		{
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "Text File (*.txt)|*.txt",
			FileName = $"Activation_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
			Title = "Export Scan Report"
		};
		if (saveFileDialog.ShowDialog() != true)
		{
			return;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("==================================================");
			stringBuilder.AppendLine("       SYSTEM ACTIVATION SCAN REPORT              ");
			stringBuilder.AppendLine("==================================================");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("Time          : ");
			handler.AppendFormatted(DateTime.Now, "dd/MM/yyyy HH:mm:ss");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("Machine Name  : ");
			handler.AppendFormatted(Environment.MachineName);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("User Account  : ");
			handler.AppendFormatted(Environment.UserName);
			stringBuilder5.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("OS Version    : ");
			handler.AppendFormatted(Environment.OSVersion);
			stringBuilder6.AppendLine(ref handler);
			stringBuilder.AppendLine("--------------------------------------------------");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder2);
			handler.AppendLiteral("Windows Status : ");
			handler.AppendFormatted(TxtWindowsStatus.Text);
			handler.AppendLiteral(" (");
			handler.AppendFormatted(TxtWindowsDetail.Text);
			handler.AppendLiteral(")");
			stringBuilder7.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder2);
			handler.AppendLiteral("Office Status  : ");
			handler.AppendFormatted(TxtOfficeStatus.Text);
			handler.AppendLiteral(" (");
			handler.AppendFormatted(TxtOfficeDetail.Text);
			handler.AppendLiteral(")");
			stringBuilder8.AppendLine(ref handler);
			stringBuilder.AppendLine("--------------------------------------------------");
			stringBuilder.AppendLine("Detected Methods & Traces:");
			stringBuilder.AppendLine(TxtMethodDetail.Text);
			stringBuilder.AppendLine("--------------------------------------------------");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder2);
			handler.AppendLiteral("CONCLUSION: ");
			handler.AppendFormatted(TxtConclusionTitle.Text);
			stringBuilder9.AppendLine(ref handler);
			stringBuilder.AppendLine(TxtConclusionDetail.Text);
			stringBuilder.AppendLine("==================================================");
			File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString(), Encoding.UTF8);
			MessageBox.Show("Report exported successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private string GetLang(string key)
	{
		return TryFindResource(key)?.ToString() ?? key;
	}

	private void SetBadgeColor(Border badge, TextBlock textBlock, string bgHex, string fgHex)
	{
		badge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex));
		textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgHex));
	}
}
