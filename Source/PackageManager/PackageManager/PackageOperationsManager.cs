// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace VRBuilder.PackageManager.Editor
{
    /// <summary>
    /// Handles different Unity's Package Manager requests.
    /// </summary>
    [InitializeOnLoad]
    public class PackageOperationsManager
    {
        public class PackageEnabledEventArgs : EventArgs
        {
            public readonly PackageInfo PackageInfo;

            public PackageEnabledEventArgs(PackageInfo packageInfo)
            {
                PackageInfo = packageInfo;
            }
        }

        public class PackageDisabledEventArgs : EventArgs
        {
            public readonly string Package;

            public PackageDisabledEventArgs(string package)
            {
                Package = package;
            }
        }

        public class InitializedEventArgs : EventArgs
        {
        }

        /// <summary>
        /// Emitted when a package was successfully installed.
        /// </summary>
        public static event EventHandler<PackageEnabledEventArgs> OnPackageEnabled;

        /// <summary>
        /// Emitted when a package was successfully removed.
        /// </summary>
        public static event EventHandler<PackageDisabledEventArgs> OnPackageDisabled;

        /// <summary>
        /// Emitted when the package list has been fetched.
        /// </summary>
        public static event EventHandler<InitializedEventArgs> OnInitialized;

        /// <summary>
        /// List of currently loaded packages in the Package Manager.
        /// </summary>
        public static PackageCollection Packages { get; private set; }

        /// <summary>
        /// Set to true when the package list has been fetched.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        static PackageOperationsManager()
        {
            FetchPackageList();
        }

        private static async void FetchPackageList()
        {
            ListRequest listRequest2 = Client.List();

            while (listRequest2.IsCompleted == false)
            {
                await Task.Delay(100);
            }

            if (listRequest2.Status == StatusCode.Failure)
            {
                Debug.LogError($"There was an error trying to retrieve a package list from the Package Manager - Error Code: [{listRequest2.Error.errorCode}] .\n{listRequest2.Error.message}");
            }
            else
            {
                Packages = listRequest2.Result;
                IsInitialized = true;
                OnInitialized?.Invoke(null, new InitializedEventArgs());

                foreach (var package in Packages)
                {
                    Debug.Log($"Package '{package.name}' version '{package.version}' is currently installed.");
                }
            }
        }

        /// <summary>
        /// Adds a package to the Package Manager.
        /// </summary>
        /// <param name="package">A string representing the package to be added.</param>
        /// <param name="version">If provided, the package will be loaded with this specific version.</param>
        public static async void LoadPackage(string package, string version = null)
        {
            if (string.IsNullOrEmpty(package) || IsPackageLoaded(package, version) || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (IsPackageLoaded(package) && string.IsNullOrEmpty(version) == false)
            {
                PackageInfo installedPackage = Packages.First(packageInfo => packageInfo.name == package);
                EditorUtility.DisplayDialog($"{installedPackage.displayName} Upgrade", $"{installedPackage.displayName} will be upgraded from v{installedPackage.version} to v{version}.", "Continue");
            }

            if (package.Contains('@') == false && string.IsNullOrEmpty(version) == false)
            {
                package = $"{package}@{version}";
            }

            Debug.Log(package);
            AddRequest addRequest = Client.Add(package);
            Debug.Log($"Enabling package: {package.Split('@').First()}, Version: {(string.IsNullOrEmpty(version) ? "latest" : version)}.");

            while (addRequest.IsCompleted == false)
            {
                await Task.Delay(100);
            }

            if (addRequest.Status == StatusCode.Failure)
            {
                Debug.LogError($"There was an error trying to enable '{package}' - Error Code: [{addRequest.Error.errorCode}] .\n{addRequest.Error.message}");
            }
            else
            {
                OnPackageEnabled?.Invoke(null, new PackageEnabledEventArgs(addRequest.Result));
                Debug.Log($"The package '{addRequest.Result.displayName}' version '{addRequest.Result.version}' has been automatically added");
                EditorUtility.RequestScriptReload();
            }
        }

        /// <summary>
        /// Removes a package from the Package Manager.
        /// </summary>
        /// <param name="package">A string representing the package to be removed.</param>
        public static async void RemovePackage(string package)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                if (IsPackageLoaded(package))
                {
                    RemoveRequest removeRequest = Client.Remove(package);
                    Debug.Log($"Removing {package}.");

                    while (removeRequest.IsCompleted == false)
                    {
                        await Task.Delay(100);
                    }

                    if (removeRequest.Status >= StatusCode.Failure)
                    {
                        Debug.LogError($"There was an error trying to enable '{package}' - Error Code: [{removeRequest.Error.errorCode}] .\n{removeRequest.Error.message}");
                    }
                    else
                    {
                        OnPackageDisabled?.Invoke(null, new PackageDisabledEventArgs(removeRequest.PackageIdOrName));
                        Debug.Log($"The package '{removeRequest.PackageIdOrName} has been removed");
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the <see cref="PackageOperationsManager"/> has already collected a list of currently available packages and
        /// given <paramref name="package"/> is already on that list with given <paramref name="version"/>.
        /// </summary>
        /// <remarks>If <paramref name="package"/> already contains an embedded version, <paramref name="version"/> will be ignored.</remarks>
        public static bool IsPackageLoaded(string package, string version = null)
        {
            if (string.IsNullOrEmpty(package))
            {
                throw new ArgumentException($"Parameter '{nameof(package)}' is null or empty.");
            }

            if (package.Contains('@'))
            {
                string[] packageData = package.Split('@');
                string packageName = packageData.First();
                string packageVersion = packageData.Last();

                return IsPackageInstalled(packageName, packageVersion);
            }

            return string.IsNullOrEmpty(version) ? IsPackageInstalled(package) : IsPackageInstalled(package, version);
        }

        /// <summary>
        /// Returns the version corresponding to the provided <paramref name="package"/> if this is installed, otherwise it returns null.
        /// </summary>
        public static string GetInstalledPackageVersion(string package)
        {
            return Packages?.FirstOrDefault(packageInfo => packageInfo.name == package)?.version;
        }

        private static bool IsPackageInstalled(string package)
        {
            return Packages.Any(packageInfo => packageInfo.name == package);
        }

        private static bool IsPackageInstalled(string package, string version)
        {
            if (IsPackageInstalled(package))
            {
                if (string.IsNullOrEmpty(version))
                {
                    return true;
                }

                PackageInfo packageInfo = Packages.First(pi => pi.name == package);

                return IsVersionInstalledOrSupported(packageInfo, version);
            }

            return false;
        }

        private static bool IsVersionInstalledOrSupported(PackageInfo packageInfo, string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return true;
            }

            if (TryParseSemanticVersion(packageInfo.version, out SemanticVersion installed) == false
                || TryParseSemanticVersion(version, out SemanticVersion required) == false)
            {
                // If we can't parse versions reliably, we err on the safe side and accept the installed
                // version to avoid endless re-import loops.
                Debug.LogWarning($"Could not reliably compare package versions for '{packageInfo.name}'. Installed='{packageInfo.version}', Required='{version}'. Assuming installed version satisfies requirement.");
                return true;
            }

            int compare = installed.CompareTo(required);
            Debug.Log($"Comparing versions for package '{packageInfo.displayName}' - Installed: v{packageInfo.version}, Required(min): v{version}, Result: {(compare >= 0 ? "OK" : "Too low")}." );

            return compare >= 0;
        }

        private readonly struct SemanticVersion : IComparable<SemanticVersion>
        {
            public readonly int[] Numbers;
            public readonly string[] PreReleaseIdentifiers;

            public SemanticVersion(int[] numbers, string[] preReleaseIdentifiers)
            {
                Numbers = numbers;
                PreReleaseIdentifiers = preReleaseIdentifiers;
            }

            public int CompareTo(SemanticVersion other)
            {
                int maxLen = Math.Max(Numbers.Length, other.Numbers.Length);
                for (int i = 0; i < maxLen; i++)
                {
                    int a = i < Numbers.Length ? Numbers[i] : 0;
                    int b = i < other.Numbers.Length ? other.Numbers[i] : 0;

                    int cmp = a.CompareTo(b);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }

                bool hasPreA = PreReleaseIdentifiers != null && PreReleaseIdentifiers.Length > 0;
                bool hasPreB = other.PreReleaseIdentifiers != null && other.PreReleaseIdentifiers.Length > 0;

                // Release > Pre-release
                if (hasPreA == false && hasPreB == false)
                {
                    return 0;
                }

                if (hasPreA == false)
                {
                    return 1;
                }

                if (hasPreB == false)
                {
                    return -1;
                }

                int preMaxLen = Math.Max(PreReleaseIdentifiers.Length, other.PreReleaseIdentifiers.Length);
                for (int i = 0; i < preMaxLen; i++)
                {
                    if (i >= PreReleaseIdentifiers.Length)
                    {
                        // A ran out of identifiers first => lower precedence
                        return -1;
                    }

                    if (i >= other.PreReleaseIdentifiers.Length)
                    {
                        return 1;
                    }

                    string idA = PreReleaseIdentifiers[i];
                    string idB = other.PreReleaseIdentifiers[i];

                    bool isNumA = int.TryParse(idA, out int numA);
                    bool isNumB = int.TryParse(idB, out int numB);

                    int cmp;
                    if (isNumA && isNumB)
                    {
                        cmp = numA.CompareTo(numB);
                    }
                    else if (isNumA != isNumB)
                    {
                        // Numeric identifiers have lower precedence than non-numeric
                        cmp = isNumA ? -1 : 1;
                    }
                    else
                    {
                        cmp = string.CompareOrdinal(idA, idB);
                    }

                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }

                return 0;
            }
        }

        private static bool TryParseSemanticVersion(string version, out SemanticVersion semanticVersion)
        {
            semanticVersion = default;

            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            // Strip build metadata ("+...")
            string noBuild = version.Split('+')[0].Trim();

            // Split prerelease ("-...")
            string[] preSplit = noBuild.Split(new[] { '-' }, 2);
            string core = preSplit[0];
            string prerelease = preSplit.Length > 1 ? preSplit[1] : null;

            // Unity packages are expected to start with dot-separated numeric identifiers.
            // If there are any leading non-numeric chars, treat as unparsable.
            if (Regex.IsMatch(core, @"^\d+(\.\d+)*$") == false)
            {
                return false;
            }

            string[] numberParts = core.Split('.');
            int[] numbers = new int[numberParts.Length];

            for (int i = 0; i < numberParts.Length; i++)
            {
                if (int.TryParse(numberParts[i], out int parsed) == false)
                {
                    return false;
                }
                numbers[i] = parsed;
            }

            string[] preIds = string.IsNullOrWhiteSpace(prerelease) ? Array.Empty<string>() : prerelease.Split('.');
            semanticVersion = new SemanticVersion(numbers, preIds);
            return true;
        }
    }
}
