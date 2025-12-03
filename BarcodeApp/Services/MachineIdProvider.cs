using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace BarcodeApp.Services;

public class MachineIdProvider
{
    public string GetMachineId()
    {
        try
        {
            var primaryInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                      ni.OperationalStatus == OperationalStatus.Up &&
                                      !string.IsNullOrEmpty(ni.GetPhysicalAddress().ToString()));

            var macAddress = primaryInterface?.GetPhysicalAddress().ToString() ?? 
                           NetworkInterface.GetAllNetworkInterfaces()
                               .FirstOrDefault()?.GetPhysicalAddress().ToString() ?? 
                           Environment.MachineName;

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(macAddress));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        catch
        {
            // Fallback to machine name hash
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

