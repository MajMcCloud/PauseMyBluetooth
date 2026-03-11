using System.Runtime.InteropServices;
using System.Text;

namespace PauseMyBluetooth.APIs;

/// <summary>
/// P/Invoke declarations for SetupAPI (setupapi.dll) and
/// Configuration Manager (cfgmgr32.dll).
/// These are the same APIs that Device Manager uses internally.
/// </summary>
internal static class NativeMethods
{
    // ── setupapi.dll ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns a handle to a device information set for all devices in the
    /// given class that are present on the machine.
    /// Pass <see cref="DIGCF_ALLCLASSES"/> to enumerate across all classes.
    /// </summary>
    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint SetupDiGetClassDevs(
        nint ClassGuid,       // null → all classes
        string? Enumerator,
        nint hwndParent,
        uint Flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern bool SetupDiDestroyDeviceInfoList(nint DeviceInfoSet);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern bool SetupDiEnumDeviceInfo(
        nint DeviceInfoSet,
        uint MemberIndex,
        ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetupDiGetDeviceInstanceId(
        nint DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        StringBuilder DeviceInstanceId,
        uint DeviceInstanceIdSize,
        out uint RequiredSize);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SetupDiGetDeviceRegistryProperty(
        nint DeviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        uint Property,
        out uint PropertyRegDataType,
        byte[]? PropertyBuffer,
        uint PropertyBufferSize,
        out uint RequiredSize);

    // ── cfgmgr32.dll ─────────────────────────────────────────────────────

    /// <summary>Locate the CM device node for a given instance ID.</summary>
    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    public static extern uint CM_Locate_DevNode(
        out uint pdnDevInst,
        string pDeviceID,
        uint ulFlags);

    /// <summary>Enable or disable a device node.</summary>
    [DllImport("cfgmgr32.dll")]
    public static extern uint CM_Enable_DevNode(uint dnDevInst, uint ulFlags);

    [DllImport("cfgmgr32.dll")]
    public static extern uint CM_Disable_DevNode(uint dnDevInst, uint ulFlags);

    /// <summary>Query the current status / problem code of a device node.</summary>
    [DllImport("cfgmgr32.dll")]
    public static extern uint CM_Get_DevNode_Status(
        out uint pulStatus,
        out uint pulProblemNumber,
        uint dnDevInst,
        uint ulFlags);

    // ── Constants ─────────────────────────────────────────────────────────

    public const uint DIGCF_PRESENT   = 0x00000002;
    public const uint DIGCF_ALLCLASSES = 0x00000004;

    // SetupDiGetDeviceRegistryProperty property codes
    public const uint SPDRP_DEVICEDESC  = 0x00000000; // FriendlyName fallback
    public const uint SPDRP_FRIENDLYNAME = 0x0000000C;
    public const uint SPDRP_CLASS       = 0x00000007;
    public const uint SPDRP_CLASSGUID   = 0x00000008;
    public const uint SPDRP_ENUMERATOR_NAME = 0x00000016;

    // CM_Locate_DevNode flags
    public const uint CM_LOCATE_DEVNODE_NORMAL = 0x00000000;

    // CM device status bits
    public const uint DN_STARTED   = 0x00000008; // device is started/running
    public const uint DN_DISABLEABLE = 0x00002000; // device can be disabled

    //CM device problem codes
    public const uint CM_PROB_DISABLED = 0x00000016; //22 "This device is disabled."

    // CM return codes
    public const uint CR_SUCCESS = 0x00000000;

    // CM_Disable_DevNode flags
    public const uint CM_DISABLE_POLITE       = 0x00000000;
    public const uint CM_DISABLE_ABSOLUTE     = 0x00000001; // force disable

    // Sentinel for invalid HDEVINFO handle
    public static readonly nint INVALID_HANDLE_VALUE = new(-1);

    // ── Structs ───────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVINFO_DATA
    {
        public uint  cbSize;
        public Guid  ClassGuid;
        public uint  DevInst;
        public nint Reserved;
    }
}
