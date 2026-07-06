using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IBM.Data.Db2;

namespace HealthChecks.IbmDb2.Tests;

internal static class Db2NativeLibraryResolver
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        if (OperatingSystem.IsLinux())
        {
            NativeLibrary.SetDllImportResolver(typeof(DB2Connection).Assembly, ResolveDb2Library);
        }
    }

    private static IntPtr ResolveDb2Library(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!IsDb2Library(libraryName))
        {
            return IntPtr.Zero;
        }

        string libraryPath = Path.Combine(AppContext.BaseDirectory, "clidriver", "lib", "libdb2.so");
        return NativeLibrary.TryLoad(libraryPath, assembly, searchPath, out IntPtr handle)
            ? handle
            : IntPtr.Zero;
    }

    private static bool IsDb2Library(string libraryName) =>
        string.Equals(libraryName, "libdb2.so", StringComparison.Ordinal) ||
        string.Equals(libraryName, "libdb2.so.1", StringComparison.Ordinal) ||
        string.Equals(libraryName, "db2", StringComparison.Ordinal);
}
