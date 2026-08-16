using System.Diagnostics;
using System.Reflection;
using PublicApiGenerator;

public class ApiApprovalTests
{
    [Fact]
    [Trait("Category", "API Approval")]
    public void public_api_should_not_change_unintentionally()
    {
        var currentAsm = Assembly.GetExecutingAssembly();
        var dependencies = currentAsm.GetReferencedAssemblies();
        var nameItems = currentAsm.GetName().Name!.Split('.');

        Debug.Assert(nameItems.Last() == "Tests");

        var nameToFind = string.Join(".", nameItems.SkipLast(1));
        var asmForTest = dependencies
            .Select(Assembly.Load)
            .Where(a => !string.Equals(a.FullName, "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5", StringComparison.OrdinalIgnoreCase)) // https://github.com/dotnet/SqlClient/issues/1930#issuecomment-1814595368
            .Where(asm => asm.GetTypes().Any(type => type.Name == "ApiMarker") && asm.GetName().Name!.Equals(nameToFind, StringComparison.InvariantCultureIgnoreCase))
            .Single();

        // https://github.com/PublicApiGenerator/PublicApiGenerator
        string publicApi = asmForTest.GeneratePublicApi(new()
        {
            IncludeAssemblyAttributes = false,
            AllowNamespacePrefixes = ["Microsoft"]
        });

        var location = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = new DirectoryInfo(Path.GetDirectoryName(location)!);
        var projectDirectory = assemblyDirectory;

        while (projectDirectory.Parent is not null && !string.Equals(projectDirectory.Parent.Name, "test", StringComparison.OrdinalIgnoreCase))
        {
            projectDirectory = projectDirectory.Parent;
        }

        Debug.Assert(projectDirectory.Parent is not null && string.Equals(projectDirectory.Parent.Name, "test", StringComparison.OrdinalIgnoreCase));

        var subFolder = projectDirectory.FullName;
        var assemblyName = asmForTest.GetName().Name!;
        var approvedFilePath = Path.Combine(subFolder, $"{assemblyName}.approved.txt");
        var receivedFilePath = Path.Combine(subFolder, $"{assemblyName}.received.txt");
        var normalizedPublicApi = publicApi.ReplaceLineEndings("\n").TrimEnd('\n');
        var normalizedApprovedApi = File.ReadAllText(approvedFilePath).ReplaceLineEndings("\n").TrimEnd('\n');

        if (!string.Equals(normalizedPublicApi, normalizedApprovedApi, StringComparison.Ordinal))
        {
            File.WriteAllText(receivedFilePath, publicApi);

            normalizedPublicApi.ShouldBe(
                normalizedApprovedApi,
                $"To approve the changes run this command:{Environment.NewLine}copy /Y \"{receivedFilePath}\" \"{approvedFilePath}\"");
        }
    }
}
