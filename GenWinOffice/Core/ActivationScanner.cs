using System;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GenWinOffice.Core;

public class ActivationScanner
{
	public static ScanResult LastResult { get; set; } = new ScanResult();

	public static async Task<ScanResult> RunDeepScanAsync()
	{
		return await Task.Run(delegate
		{
			ActivationScanner activationScanner = new ActivationScanner();
			ScanResult scanResult = new ScanResult();
			activationScanner.CollectSystemInformation(scanResult);
			activationScanner.CheckWindowsKmsServer(scanResult);
			activationScanner.CheckWindowsKms38(scanResult);
			string activePartialKey = activationScanner.CheckWindowsHwid(scanResult);
			activationScanner.CheckWindowsBiosKey(scanResult, activePartialKey);
			activationScanner.CheckSuspiciousFilesAndServices(scanResult);
			activationScanner.CheckSuspiciousTasks(scanResult);
			activationScanner.CheckRegistryTampering(scanResult);
			activationScanner.CheckPowerShellConsoleHistory(scanResult);
			activationScanner.CheckOfficeKms(scanResult);
			activationScanner.CheckOfficeOhook(scanResult);
			activationScanner.CalculateVerdict(scanResult);
			scanResult.ScanTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
			scanResult.IsScanned = true;
			LastResult = scanResult;
			return scanResult;
		});
	}

	public void CollectSystemInformation(ScanResult result)
	{
		result.PcName = Environment.MachineName;
		result.CurrentUser = Environment.UserName;
		result.OsName = GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName");
		result.OsVersion = GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "DisplayVersion");
		result.Edition = GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "EditionID");
		result.Architecture = (Environment.Is64BitOperatingSystem ? "x64" : "x86");
		result.IsLtsc = ((result.OsName.IndexOf("LTSC", StringComparison.OrdinalIgnoreCase) >= 0 || result.OsName.IndexOf("IoT Enterprise LTSC", StringComparison.OrdinalIgnoreCase) >= 0) ? "Yes" : "No");
		result.IsEvaluation = ((result.OsName.IndexOf("Evaluation", StringComparison.OrdinalIgnoreCase) >= 0) ? "Yes" : "No");
		result.InstallDate = GetWindowsInstallDate();
		result.WinActStatus = GetWindowsActivationStatusViaWmi();
		result.OfficeInstalled = CheckOfficeInstalledNative();
		result.OffInstalledStr = (result.OfficeInstalled ? "Installed / Da cai dat" : "Not Installed / Chua cai dat");
		result.OffActStr = (result.OfficeInstalled ? GetOfficeActivationStatusViaWmi() : "N/A");
	}

	public void CheckWindowsKmsServer(ScanResult result)
	{
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT KeyManagementServiceName, LicenseStatus FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND ApplicationId = '55c92734-d682-4d71-983e-d6ec3f16059f'");
			try
			{
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					string kmsHost = item["KeyManagementServiceName"]?.ToString() ?? string.Empty;
					if (!string.IsNullOrEmpty(kmsHost))
					{
						result.KmsHost = kmsHost;
						if (new string[9] { "127.0.0.1", "localhost", "kms.msganti.com", "kms.digiboy.ir", "kms.loli.best", "mskms.orgzh.org", "kms.lotro.cc", "kms.chinancce.com", "kms.shuax.com" }.Any((string b) => kmsHost.Equals(b, StringComparison.OrdinalIgnoreCase)))
						{
							result.IsKmsDetected = true;
							result.KmsVerdict = "suspicious";
						}
						else
						{
							result.IsKmsDetected = true;
							result.KmsVerdict = "unknown_server";
						}
						break;
					}
				}
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch
		{
			result.IsKmsDetected = false;
		}
	}

	public void CheckWindowsKms38(ScanResult result)
	{
		if (Environment.OSVersion.Version.Build >= 26100)
		{
			result.Kms38Status = "not_supported";
			return;
		}
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT EvaluationEndDate FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND LicenseStatus = 1");
			try
			{
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					if ((item["EvaluationEndDate"]?.ToString() ?? string.Empty).StartsWith("2038"))
					{
						result.Kms38Status = "true";
						return;
					}
				}
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch
		{
		}
		result.Kms38Status = "false";
	}

	public string CheckWindowsHwid(ScanResult result)
	{
		string partialKey = string.Empty;
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT PartialProductKey FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND ApplicationId = '55c92734-d682-4d71-983e-d6ec3f16059f' AND LicenseStatus = 1");
			try
			{
				using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator();
				if (managementObjectEnumerator.MoveNext())
				{
					ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
					partialKey = managementObject["PartialProductKey"]?.ToString() ?? string.Empty;
				}
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch
		{
		}
		string[] source = new string[29]
		{
			"7CFBY", "DRR8H", "8HV2C", "QPFCT", "MDWWW", "DYJWX", "P39PB", "M7V2X", "9HKR4", "8HVX7",
			"WXCHW", "8TYMD", "6F4BT", "CKFFD", "RRK69", "YY74H", "J8JXD", "D32MH", "3V66T", "PKCKT",
			"MHBPB", "QPF8P", "2YV77", "WT2RQ", "VMJ2C", "DJ4F6", "T6R4W", "BHDCD", "KD72Y"
		};
		result.IsHwidDetected = source.Any((string k) => k.Equals(partialKey, StringComparison.OrdinalIgnoreCase));
		return partialKey;
	}

	public void CheckWindowsBiosKey(ScanResult result, string activePartialKey)
	{
		string registryValue = GetRegistryValue("SYSTEM\\CurrentControlSet\\Control\\ProductOptions", "OriginalProductKey");
		if (string.IsNullOrEmpty(registryValue))
		{
			registryValue = GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SoftwareProtectionPlatform", "BackupProductKeyDefault");
		}
		if (string.IsNullOrEmpty(registryValue))
		{
			result.BiosStatus = "unknown";
			return;
		}
		string value = ((registryValue.Length >= 5) ? registryValue.Substring(registryValue.Length - 5) : registryValue);
		if (!string.IsNullOrEmpty(activePartialKey))
		{
			if (activePartialKey.Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				result.BiosStatus = "false";
			}
			else
			{
				result.BiosStatus = (result.IsHwidDetected ? "upgrade" : "false1");
			}
		}
	}

	public void CheckSuspiciousFilesAndServices(ScanResult result)
	{
		bool flag = new string[9] { "C:\\Program Files\\KMSpico", "C:\\Program Files (x86)\\KMSpico", "C:\\ProgramData\\KMSAuto", "C:\\ProgramData\\KMSAutoS", "C:\\Windows\\KMSAuto", "C:\\Program Files\\KMSAuto Net", "C:\\Windows\\SECOH-QAD.exe", "C:\\Windows\\SECOH-QAD.dll", "C:\\Windows\\KMSConnection.xml" }.Any((string p) => File.Exists(p) || Directory.Exists(p));
		bool flag2 = false;
		try
		{
			flag2 = ServiceController.GetServices().Any((ServiceController s) => s.ServiceName.Equals("KMSpico_service", StringComparison.OrdinalIgnoreCase) || s.ServiceName.Equals("Service_KMS", StringComparison.OrdinalIgnoreCase));
		}
		catch
		{
		}
		result.SuspiciousFilesDetected = flag | flag2;
	}

	public void CheckSuspiciousTasks(ScanResult result)
	{
		string path = "C:\\Windows\\System32\\Tasks";
		string[] source = new string[9] { "KMSConnection", "KMSpico", "KMSAuto", "KMSAutoS", "KMS38", "Wub", "KMS-Activation", "HEU_KMS", "AIO_KMS" };
		bool suspiciousTasksDetected = false;
		if (Directory.Exists(path))
		{
			try
			{
				string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
				foreach (string path2 in files)
				{
					string fileName = Path.GetFileName(path2);
					if (source.Any((string t) => t.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
					{
						suspiciousTasksDetected = true;
						break;
					}
					if (File.ReadAllText(path2).IndexOf("secoh-qad", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						suspiciousTasksDetected = true;
						break;
					}
				}
			}
			catch
			{
			}
		}
		result.SuspiciousTasksDetected = suspiciousTasksDetected;
	}

	public void CheckRegistryTampering(ScanResult result)
	{
		bool registryTampered = false;
		using (RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SoftwareProtectionPlatform"))
		{
			if (registryKey != null && (registryKey.GetValue("NoGenTicket") != null || registryKey.GetValue("KeyManagementServiceName") != null || registryKey.GetValue("KeyManagementServicePort") != null))
			{
				registryTampered = true;
			}
		}
		result.RegistryTampered = registryTampered;
	}

	public void CheckPowerShellConsoleHistory(ScanResult result)
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\PowerShell\\PSReadLine\\ConsoleHost_history.txt");
		if (File.Exists(path))
		{
			try
			{
				string text = File.ReadAllText(path);
				if (text.IndexOf("get.activated.win", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("massgrave", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("irm https://get.activated.win", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					result.HistoryTampered = true;
					return;
				}
			}
			catch
			{
			}
		}
		result.HistoryTampered = false;
	}

	public void CheckOfficeKms(ScanResult result)
	{
		if (!result.OfficeInstalled)
		{
			return;
		}
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT KeyManagementServiceName FROM OfficeSoftwareProtectionProduct WHERE PartialProductKey IS NOT NULL");
			try
			{
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					if (!string.IsNullOrEmpty(item["KeyManagementServiceName"]?.ToString() ?? string.Empty))
					{
						result.OfficeKmsDetected = true;
						return;
					}
				}
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch
		{
		}
		result.OfficeKmsDetected = false;
	}

	public void CheckOfficeOhook(ScanResult result)
	{
		string[] source = new string[4] { "C:\\Program Files\\Microsoft Office\\root\\vfs\\System\\sppcs.dll", "C:\\Program Files (x86)\\Microsoft Office\\root\\vfs\\System\\sppcs.dll", "C:\\Program Files\\Microsoft Office 15\\root\\vfs\\System\\sppcs.dll", "C:\\Program Files (x86)\\Microsoft Office 15\\root\\vfs\\System\\sppcs.dll" };
		result.OfficeOhookDetected = source.Any(File.Exists);
	}

	public void CalculateVerdict(ScanResult result)
	{
		result.WindowsScore = 1;
		if (result.SuspiciousFilesDetected || result.SuspiciousTasksDetected || result.RegistryTampered || result.HistoryTampered || result.BiosStatus == "upgrade")
		{
			result.WindowsScore = 2;
		}
		if (result.IsKmsDetected || result.IsKms38Detected || result.IsHwidDetected)
		{
			result.WindowsScore = 3;
		}
		result.OfficeScore = 1;
		if (result.OfficeKmsDetected || result.OfficeOhookDetected)
		{
			result.OfficeScore = 3;
		}
	}

	private string GetRegistryValue(string subKey, string valueName)
	{
		try
		{
			using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey(subKey);
			return registryKey?.GetValue(valueName)?.ToString() ?? string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	private string GetWindowsInstallDate()
	{
		try
		{
			if (long.TryParse(GetRegistryValue("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "InstallDate"), out var result))
			{
				return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(result).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	private string GetWindowsActivationStatusViaWmi()
	{
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND ApplicationId = '55c92734-d682-4d71-983e-d6ec3f16059f'");
			try
			{
				using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator();
				if (managementObjectEnumerator.MoveNext())
				{
					return (uint)(((ManagementObject)managementObjectEnumerator.Current)["LicenseStatus"] ?? ((object)0)) switch
					{
						1u => "Activated / Da kich hoat", 
						2u => "Grace Period / Dang trong thoi gian dung thu", 
						_ => "Unactivated / Chua kich hoat", 
					};
				}
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch
		{
		}
		return "Unactivated / Chua kich hoat";
	}

	private bool CheckOfficeInstalledNative()
	{
		using RegistryKey? registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Office");
		return registryKey != null;
	}

	private string GetOfficeActivationStatusViaWmi()
	{
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT LicenseStatus FROM OfficeSoftwareProtectionProduct WHERE PartialProductKey IS NOT NULL");
			try
			{
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					if ((uint)(item["LicenseStatus"] ?? ((object)0)) == 1)
					{
						return "Activated / Da kich hoat";
					}
				}
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch
		{
		}
		return "Unactivated / Chua kich hoat";
	}
}
