namespace PauseMyBluetooth.Models;

public class BluetoothDevice
{
    public string FriendlyName { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;

    public string[] InstancePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;

    // A device is considered "auto-connect enabled" when its PnP entries are all enabled.
    // We track the desired user intent here.
    public bool AutoConnectEnabled { get; set; } = true;

    public override string ToString() => FriendlyName;

    
}
