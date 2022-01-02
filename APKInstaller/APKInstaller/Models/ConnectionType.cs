using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKInstaller.Models
{
    /// <summary>
    /// Enumeration denoting connection type.
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// Connected to wired network
        /// </summary>
        Ethernet,

        /// <summary>
        /// Connected to wireless network
        /// </summary>
        WiFi,

        /// <summary>
        /// Connected to mobile data connection
        /// </summary>
        Data,

        /// <summary>
        /// Connection type not identified
        /// </summary>
        Unknown,
    }
}
