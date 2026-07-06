namespace HealthChecks.IbmDb2.Tests;

public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "IBM Db2 container tests require the Linux IBM Db2 provider package.";
        }
    }
}
