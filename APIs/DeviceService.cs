using PauseMyBluetooth.Models;
using System.Runtime.InteropServices;
using System.Text;

namespace PauseMyBluetooth.APIs;

/// <summary>
/// Enumerates paired Bluetooth devices and toggles their enabled/disabled state
/// directly via SetupAPI + CfgMgr32 — no PowerShell, no external processes.
/// </summary>
public static class DeviceService
{
    // Bluetooth-related PnP enumerator prefixes / class names we care about
    private static readonly HashSet<string> BluetoothEnumerators = new(StringComparer.OrdinalIgnoreCase)
    {
        "BTHENUM",   // Classic Bluetooth
        "BTH",       // Bluetooth radio / bus
        "BTHLE",     // Bluetooth Low Energy
        "BTHLEDEVICE"
    };

    private static readonly HashSet<string> BluetoothClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Bluetooth",
        "BTHLEDevice",
        "AudioEndpoint",
        "MEDIA"
    };

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns all Bluetooth-related PnP devices that are currently present.
    /// </summary>
    public static List<BluetoothDevice> GetPairedDevices()
    {
        var results = new List<BluetoothDevice>();

        nint hDevInfo = NativeMethods.SetupDiGetClassDevs(
            nint.Zero,
            null,
            nint.Zero,
            NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_ALLCLASSES);

        if (hDevInfo == NativeMethods.INVALID_HANDLE_VALUE)
            throw new InvalidOperationException(
                $"SetupDiGetClassDevs fehlgeschlagen: {Marshal.GetLastWin32Error()}");

        try
        {
            var devData = new NativeMethods.SP_DEVINFO_DATA();
            devData.cbSize = (uint)Marshal.SizeOf(devData);

            for (uint i = 0; NativeMethods.SetupDiEnumDeviceInfo(hDevInfo, i, ref devData); i++)
            {
                string instanceId = GetInstanceId(hDevInfo, ref devData);
                string className  = GetStringProperty(hDevInfo, ref devData, NativeMethods.SPDRP_CLASS);
                string enumName   = GetStringProperty(hDevInfo, ref devData, NativeMethods.SPDRP_ENUMERATOR_NAME);

                bool isBluetooth =
                    BluetoothClasses.Contains(className) ||
                    BluetoothEnumerators.Contains(enumName) ||
                    IsBluetoothInstanceId(instanceId);

                if (!isBluetooth) continue;

                // AudioEndpoint / MEDIA: only keep if InstanceId looks like BT
                if ((className.Equals("AudioEndpoint", StringComparison.OrdinalIgnoreCase) ||
                     className.Equals("MEDIA", StringComparison.OrdinalIgnoreCase))
                    && !IsBluetoothInstanceId(instanceId))
                    continue;

                string friendlyName = GetStringProperty(hDevInfo, ref devData, NativeMethods.SPDRP_FRIENDLYNAME);
                if (string.IsNullOrWhiteSpace(friendlyName))
                    friendlyName = GetStringProperty(hDevInfo, ref devData, NativeMethods.SPDRP_DEVICEDESC);
                if (string.IsNullOrWhiteSpace(friendlyName))
                    friendlyName = instanceId;

                var (status, problem) = GetDevNodeStatus(instanceId);

                var instance_path = instanceId.Split('\\','_');

                results.Add(new BluetoothDevice
                {
                    FriendlyName       = friendlyName,
                    InstanceId         = instanceId,
                    InstancePath       = instance_path,
                    Class              = className,
                    Status             = FormatStatus(status, problem),
                    AutoConnectEnabled = IsEnabled(status, problem)
                });
            }
        }
        finally
        {
            NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfo);
        }

        // De-duplicate by InstanceId, sort nicely
        return results
            .GroupBy(d => d.InstanceId)
            .Select(g => g.First())
            .OrderBy(d => d.FriendlyName)
            .ThenBy(d => d.Class)
            .ToList();
    }

    /// <summary>Disable a device via CfgMgr32 (same as Device Manager → Disable).</summary>
    public static void DisableDevice(BluetoothDevice device)
    {
        uint cr = NativeMethods.CM_Locate_DevNode(
            out uint devInst, device.InstanceId, NativeMethods.CM_LOCATE_DEVNODE_NORMAL);

        if (cr != NativeMethods.CR_SUCCESS)
            throw new InvalidOperationException(
                $"CM_Locate_DevNode fehlgeschlagen (CR=0x{cr:X}). Programm als Administrator starten?");

        cr = NativeMethods.CM_Disable_DevNode(devInst, NativeMethods.CM_DISABLE_POLITE);
        if (cr != NativeMethods.CR_SUCCESS)
            throw new InvalidOperationException($"CM_Disable_DevNode fehlgeschlagen (CR=0x{cr:X}).");
    }

    /// <summary>Re-enable a previously disabled device via CfgMgr32.</summary>
    public static void EnableDevice(BluetoothDevice device)
    {
        uint cr = NativeMethods.CM_Locate_DevNode(
            out uint devInst, device.InstanceId, NativeMethods.CM_LOCATE_DEVNODE_NORMAL);

        if (cr != NativeMethods.CR_SUCCESS)
            throw new InvalidOperationException(
                $"CM_Locate_DevNode fehlgeschlagen (CR=0x{cr:X}). Programm als Administrator starten?");

        cr = NativeMethods.CM_Enable_DevNode(devInst, 0);
        if (cr != NativeMethods.CR_SUCCESS)
            throw new InvalidOperationException($"CM_Enable_DevNode fehlgeschlagen (CR=0x{cr:X}).");
    }

    // -----------------------------------------------------------------------
    // Helpers — SetupAPI
    // -----------------------------------------------------------------------

    private static string GetInstanceId(nint hDevInfo, ref NativeMethods.SP_DEVINFO_DATA devData)
    {
        var sb = new StringBuilder(512);
        NativeMethods.SetupDiGetDeviceInstanceId(hDevInfo, ref devData, sb, (uint)sb.Capacity, out _);
        return sb.ToString();
    }

    private static string GetStringProperty(
        nint hDevInfo,
        ref NativeMethods.SP_DEVINFO_DATA devData,
        uint property)
    {
        // First call: get required buffer size
        NativeMethods.SetupDiGetDeviceRegistryProperty(
            hDevInfo, ref devData, property,
            out _, null, 0, out uint required);

        if (required == 0) return string.Empty;

        var buf = new byte[required];
        if (!NativeMethods.SetupDiGetDeviceRegistryProperty(
                hDevInfo, ref devData, property,
                out uint regType, buf, required, out _))
            return string.Empty;

        // REG_SZ / REG_EXPAND_SZ
        if (regType is 1 or 2)
            return Encoding.Unicode.GetString(buf).TrimEnd('\0');

        return string.Empty;
    }

    // -----------------------------------------------------------------------
    // Helpers — CfgMgr32
    // -----------------------------------------------------------------------

    private static (uint status, uint problem) GetDevNodeStatus(string instanceId)
    {
        uint cr = NativeMethods.CM_Locate_DevNode(
            out uint devInst, instanceId, NativeMethods.CM_LOCATE_DEVNODE_NORMAL);

        if (cr != NativeMethods.CR_SUCCESS) return (0, 0);

        NativeMethods.CM_Get_DevNode_Status(out uint status, out uint problem, devInst, 0);
        return (status, problem);
    }

    private static bool IsEnabled(uint status, uint problem)
    {
        return problem == 0;
    }
    //=> (status & NativeMethods.DN_DISABLEABLE) == 0 && problem != 22 /* CM_PROB_DISABLED */;

    private static string FormatStatus(uint status, uint problem)
    {
        if (problem == NativeMethods.CM_PROB_DISABLED) return "Disabled";

        //if ((status & NativeMethods.DN_DISABLEABLE) != 0) return "Disabled";
        if ((status & NativeMethods.DN_STARTED) != 0) return "OK";
        return "Unknown";
    }

    private static bool IsBluetoothInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return false;
        var up = instanceId.ToUpperInvariant();
        return up.StartsWith("BTHENUM\\") ||
               up.StartsWith("BTH\\")     ||
               up.StartsWith("BTHLE\\")   ||
               up.Contains("\\BTHENUM")   ||
               up.Contains("_VID&")       && up.Contains("_PID&");
    }
}
