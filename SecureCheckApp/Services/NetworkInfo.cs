using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SecureCheckApp.Services;

public class NetworkInfo
{
    /// <summary>
    /// Gets the machine's active IPv4 address (excludes loopback and non-operational adapters)
    /// </summary>
    public static string GetLocalIPv4()
    {
        try
        {
            // Get all network interfaces
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var unicastAddresses = ipProperties.UnicastAddresses;

                // Get the first IPv4 address that's not loopback
                foreach (var unicastAddress in unicastAddresses)
                {
                    if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(unicastAddress.Address))
                    {
                        return unicastAddress.Address.ToString();
                    }
                }
            }

            // Fallback: use DNS method
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && 
                    !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }

        return "Not detected";
    }
}
