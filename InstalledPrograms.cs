using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;

namespace InstalledPrograms
{
    public class Programs
    {
        static readonly string[] RegistryKeys =
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        };

        /// <summary>
        /// Tries to find the installation path based on the specified software name.
        /// </summary>
        /// <param name="containedName">The program name that the registry key value must contain.</param>
        /// <returns>If a registration key is found then the path otherwise; null.</returns>
        public static Task<DirectoryInfo?> GetSoftwareDirectoryAsync(string containedName)
        {
            return Task.Factory.StartNew(() => GetSoftwareDirectory(containedName));
        }
        /// <summary>
        /// Tries to find the installation path asynchronously based on the specified software name.
        /// </summary>
        /// <param name="containedName">The program name that the registry key value must contain.</param>
        /// <returns>If a registration key is found then the path otherwise; null.</returns>
        public static DirectoryInfo? GetSoftwareDirectory(string containedName)
        {
            DirectoryInfo? softwareInfo = null;
            
            foreach (var uninstallKey in RegistryKeys)
            {
                RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                using (var rk = hklm.OpenSubKey(uninstallKey))
                {
                    if (rk is null) continue;

                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (var sk = rk.OpenSubKey(skName))
                        {
                            try
                            {
                                string? softwareName = sk?.GetValue("DisplayName")?.ToString();
                                if ((softwareName is not null) && (softwareName.IndexOf(containedName, StringComparison.OrdinalIgnoreCase) > -1))
                                {
                                    string? path = sk?.GetValue("InstallLocation")?.ToString();

                                    if (path is null)
                                    {
                                        // Fallback location if InstallLocation value not found
                                        var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{softwareName}.exe");

                                        if (key is not null)
                                        {
                                            path = key.GetValue("Path")?.ToString();
                                            key.Close();
                                        }
                                    }

                                    if (string.IsNullOrEmpty(path) == false)
                                    {
                                        softwareInfo = new DirectoryInfo(path);
                                    }

                                    break;
                                }
                            }
                            catch { }
                        }
                    }

                    rk.Close();
                }

                if (softwareInfo != null) break;
            }

            return softwareInfo;
        }
    }
}
