using System;
using System.Linq;
using System.Net;

namespace xObsAsyncImageSource.SocketUtils.Helper
{
    public static class LocalAddressRepository
    {
        public static IPAddress[] LocalIPs { get; set; }
        public static string LocalAddress => LocalIPs.Any() ? LocalIPs[0].ToString() : $"{IPAddress.Loopback}";

        static LocalAddressRepository()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName())
                                      .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                      .ToArray();

            LocalIPs = new IPAddress[localIPs.Length + 1];
            Array.Copy(localIPs, 0, LocalIPs, 0, localIPs.Length);
            LocalIPs[localIPs.Length] = IPAddress.Loopback;
        }
    }
}
