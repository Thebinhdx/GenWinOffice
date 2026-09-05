namespace GenWinOffice.Core;

public class ScanResult
{
	public bool IsScanned { get; set; }

	public int WindowsScore { get; set; }

	public int OfficeScore { get; set; }

	public string ScanTime { get; set; } = string.Empty;

	public string PcName { get; set; } = string.Empty;

	public string CurrentUser { get; set; } = string.Empty;

	public string OsName { get; set; } = string.Empty;

	public string OsVersion { get; set; } = string.Empty;

	public string Edition { get; set; } = string.Empty;

	public string Architecture { get; set; } = string.Empty;

	public string IsLtsc { get; set; } = string.Empty;

	public string IsEvaluation { get; set; } = string.Empty;

	public string InstallDate { get; set; } = string.Empty;

	public string WinActStatus { get; set; } = string.Empty;

	public bool OfficeInstalled { get; set; }

	public string OffInstalledStr { get; set; } = string.Empty;

	public string OffActStr { get; set; } = string.Empty;

	public string KmsHost { get; set; } = string.Empty;

	public bool IsKmsDetected { get; set; }

	public string KmsVerdict { get; set; } = string.Empty;

	public string Kms38Status { get; set; } = string.Empty;

	public bool IsKms38Detected => Kms38Status == "true";

	public bool IsHwidDetected { get; set; }

	public string BiosStatus { get; set; } = string.Empty;

	public bool SuspiciousFilesDetected { get; set; }

	public bool SuspiciousTasksDetected { get; set; }

	public bool RegistryTampered { get; set; }

	public bool HistoryTampered { get; set; }

	public bool OfficeKmsDetected { get; set; }

	public bool OfficeOhookDetected { get; set; }
}
