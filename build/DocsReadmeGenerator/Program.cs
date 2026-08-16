using System.Text;
using System.Text.RegularExpressions;

return await DocsReadmeGeneratorProgram.RunAsync(args).ConfigureAwait(false);

internal static partial class DocsReadmeGeneratorProgram
{
    private static readonly string[] _readmeFileNames = ["README.md", "Readme.md", "readme.md"];
    private static readonly HashSet<string> _topLevelExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".github",
        ".vs",
        ".vscode",
        "artifacts",
        "artifacts-test",
        "docs"
    };

    private static readonly HashSet<string> _anyLevelExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "node_modules",
        "obj"
    };

    private static readonly HashSet<string> _binaryAssetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".gif",
        ".jpeg",
        ".jpg",
        ".png",
        ".svg",
        ".webp"
    };

    private static readonly Regex _headingRegex = HeadingPattern();
    private static readonly Regex _linkedImageLinkRegex = LinkedImageLinkPattern();
    private static readonly Regex _imageRegex = ImagePattern();
    private static readonly Regex _linkRegex = LinkPattern();
    private static readonly UTF8Encoding _utf8WithoutBom = new(false);

    public static Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = GeneratorOptions.Parse(args);
            var generator = new DocsReadmeGenerator(options);
            generator.Generate();
            return Task.FromResult(0);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return Task.FromResult(1);
        }
    }

    private sealed class DocsReadmeGenerator(GeneratorOptions options)
    {
        private readonly string _repositoryRoot = Path.GetFullPath(options.RepositoryRoot);
        private readonly string _docsRoot = Path.GetFullPath(options.DocsRoot);
        private readonly string _generatedRoot = Path.GetFullPath(Path.Combine(options.DocsRoot, "reference", "readmes"));
        private readonly string _generatedAssetRoot = Path.GetFullPath(Path.Combine(options.DocsRoot, "reference", "readmes", "assets"));
        private readonly string _repositoryUrl = options.RepositoryUrl.TrimEnd('/');
        private readonly string _branch = options.Branch;
        private readonly Dictionary<string, string> _copiedAssets = new(StringComparer.OrdinalIgnoreCase);

        public void Generate()
        {
            if (!Directory.Exists(_repositoryRoot))
            {
                throw new DirectoryNotFoundException($"Repository root not found: {_repositoryRoot}");
            }

            if (!Directory.Exists(_docsRoot))
            {
                throw new DirectoryNotFoundException($"Docs root not found: {_docsRoot}");
            }

            if (Directory.Exists(_generatedRoot))
            {
                Directory.Delete(_generatedRoot, recursive: true);
            }

            Directory.CreateDirectory(_generatedRoot);

            var entries = EnumerateReadmes()
                .Where(path => !string.Equals(GetNormalizedRelativePath(path), "README.md", StringComparison.OrdinalIgnoreCase))
                .Select(WriteGeneratedReadmePage)
                .OrderBy(static entry => entry.Category, StringComparer.Ordinal)
                .ThenBy(static entry => entry.SourcePath, StringComparer.Ordinal)
                .ToArray();

            WriteGeneratedIndexPage(entries);

            Console.WriteLine($"Generated {entries.Length} README pages in {_generatedRoot}");
        }

        private IEnumerable<string> EnumerateReadmes()
        {
            var pendingDirectories = new Stack<string>();
            pendingDirectories.Push(_repositoryRoot);

            while (pendingDirectories.Count > 0)
            {
                var currentDirectory = pendingDirectories.Pop();

                foreach (var subDirectory in Directory.EnumerateDirectories(currentDirectory))
                {
                    if (!ShouldSkipDirectory(subDirectory))
                    {
                        pendingDirectories.Push(subDirectory);
                    }
                }

                foreach (var filePath in Directory.EnumerateFiles(currentDirectory))
                {
                    if (_readmeFileNames.Contains(Path.GetFileName(filePath), StringComparer.OrdinalIgnoreCase))
                    {
                        yield return Path.GetFullPath(filePath);
                    }
                }
            }
        }

        private bool ShouldSkipDirectory(string directoryPath)
        {
            var relativePath = GetNormalizedRelativePath(directoryPath);
            if (string.IsNullOrEmpty(relativePath) || relativePath == ".")
            {
                return false;
            }

            var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return false;
            }

            if (_topLevelExcludedDirectories.Contains(segments[0]))
            {
                return true;
            }

            if (segments[0].StartsWith(".", StringComparison.Ordinal))
            {
                return true;
            }

            if (segments[0].StartsWith("logs-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return segments.Any(_anyLevelExcludedDirectories.Contains);
        }

        private GeneratedEntry WriteGeneratedReadmePage(string sourceReadmePath)
        {
            var repoRelativeReadmePath = GetNormalizedRelativePath(sourceReadmePath);
            var sourceContent = File.ReadAllText(sourceReadmePath, Encoding.UTF8);
            var pageTitle = GetReadmeTitle(sourceContent, repoRelativeReadmePath);
            var category = GetReadmeCategory(repoRelativeReadmePath);
            var virtualUrl = GetReadmeVirtualUrl(repoRelativeReadmePath);
            var outputDirectory = GetReadmeOutputDirectory(repoRelativeReadmePath);
            var outputPath = Path.Combine(outputDirectory, "index.md");
            var sourceReferencePath = GetReferenceSourcePath(repoRelativeReadmePath);
            var sourceReferenceUrl = ConvertRepoTreePathToGitHubUrl(sourceReferencePath);
            var rewrittenContent = RewriteMarkdownContent(sourceContent, sourceReadmePath).Trim();

            Directory.CreateDirectory(outputDirectory);

            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine("---");
            contentBuilder.AppendLine($"title: \"{EscapeYamlDoubleQuotedString(pageTitle)}\"");
            contentBuilder.AppendLine("show_title: false");
            contentBuilder.AppendLine("---");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine($"> {GetReferenceLead(category)} maintained from the source documentation in [{sourceReferencePath}]({sourceReferenceUrl}).");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(rewrittenContent);

            File.WriteAllText(outputPath, contentBuilder.ToString(), _utf8WithoutBom);

            return new GeneratedEntry(
                pageTitle,
                category,
                repoRelativeReadmePath,
                virtualUrl);
        }

        private void WriteGeneratedIndexPage(IReadOnlyCollection<GeneratedEntry> entries)
        {
            var indexPath = Path.Combine(_generatedRoot, "index.md");
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);

            var orderedCategories = new[]
            {
                "Extensions",
                "Source Packages",
                "Samples",
                "Build",
                "Deployment",
                "Other"
            };

            var groupedEntries = entries
                .GroupBy(static entry => entry.Category, StringComparer.Ordinal)
                .ToDictionary(static group => group.Key, static group => group.OrderBy(static item => item.SourcePath, StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);

            var builder = new StringBuilder();
            builder.AppendLine("---");
            builder.AppendLine("title: Package References");
            builder.AppendLine("permalink: /reference/readmes/");
            builder.AppendLine("---");
            builder.AppendLine();
            builder.AppendLine("This section is generated during the docs build. Package, extension, sample, and other project documentation is published here under stable reference URLs.");
            builder.AppendLine();
            builder.AppendLine($"Total generated pages: **{entries.Count}**.");
            builder.AppendLine();

            foreach (var category in orderedCategories)
            {
                if (!groupedEntries.TryGetValue(category, out var categoryEntries))
                {
                    continue;
                }

                builder.AppendLine($"## {category}");
                builder.AppendLine();

                foreach (var entry in categoryEntries)
                {
                    builder.AppendLine($"- [{entry.Title}]({entry.Url}) - `{GetReferenceSourcePath(entry.SourcePath)}`");
                }

                builder.AppendLine();
            }

            File.WriteAllText(indexPath, builder.ToString(), _utf8WithoutBom);
        }

        private string RewriteMarkdownContent(string content, string sourceReadmePath)
        {
            var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var rewrittenLines = new List<string>(lines.Length);
            var insideCodeFence = false;

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    insideCodeFence = !insideCodeFence;
                    rewrittenLines.Add(line);
                    continue;
                }

                rewrittenLines.Add(insideCodeFence ? line : RewriteMarkdownLine(line, sourceReadmePath));
            }

            return string.Join('\n', rewrittenLines);
        }

        private string RewriteMarkdownLine(string line, string sourceReadmePath)
        {
            var linkedImageReplacements = new Dictionary<string, string>(StringComparer.Ordinal);
            var tokenIndex = 0;

            var processedLine = _linkedImageLinkRegex.Replace(line, match =>
            {
                var rewrittenTarget = ResolveTargetUrl(match.Groups["target"].Value, sourceReadmePath, isImage: false);
                if (rewrittenTarget is null)
                {
                    return match.Value;
                }

                var token = $"__README_LINKED_IMAGE_TOKEN_{tokenIndex++}__";
                linkedImageReplacements[token] = $"[{match.Groups["image"].Value}]({rewrittenTarget}{match.Groups["title"].Value})";
                return token;
            });

            processedLine = _imageRegex.Replace(processedLine, match =>
            {
                var rewrittenTarget = ResolveTargetUrl(match.Groups["target"].Value, sourceReadmePath, isImage: true);
                if (rewrittenTarget is null)
                {
                    return match.Value;
                }

                return $"![{match.Groups["label"].Value}]({rewrittenTarget}{match.Groups["title"].Value})";
            });

            processedLine = _linkRegex.Replace(processedLine, match =>
            {
                var rewrittenTarget = ResolveTargetUrl(match.Groups["target"].Value, sourceReadmePath, isImage: false);
                if (rewrittenTarget is null)
                {
                    return match.Value;
                }

                return $"[{match.Groups["label"].Value}]({rewrittenTarget}{match.Groups["title"].Value})";
            });

            foreach (var replacement in linkedImageReplacements)
            {
                processedLine = processedLine.Replace(replacement.Key, replacement.Value, StringComparison.Ordinal);
            }

            return processedLine;
        }

        private string? ResolveTargetUrl(string rawTarget, string sourceReadmePath, bool isImage)
        {
            if (string.IsNullOrWhiteSpace(rawTarget))
            {
                return null;
            }

            var trimmedTarget = rawTarget.Trim();
            if (trimmedTarget.StartsWith('#') || trimmedTarget.StartsWith('/'))
            {
                return null;
            }

            if (Uri.TryCreate(trimmedTarget.Trim('<', '>'), UriKind.Absolute, out _))
            {
                return null;
            }

            var useAngleBrackets = trimmedTarget.StartsWith('<') && trimmedTarget.EndsWith('>');
            if (useAngleBrackets)
            {
                trimmedTarget = trimmedTarget[1..^1];
            }

            var fragment = string.Empty;
            var hashIndex = trimmedTarget.IndexOf('#', StringComparison.Ordinal);
            if (hashIndex >= 0)
            {
                fragment = trimmedTarget[hashIndex..];
                trimmedTarget = trimmedTarget[..hashIndex];
            }

            if (string.IsNullOrWhiteSpace(trimmedTarget))
            {
                return null;
            }

            var sourceDirectory = Path.GetDirectoryName(sourceReadmePath)!;
            var candidatePath = Path.GetFullPath(Path.Combine(sourceDirectory, trimmedTarget.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsWithinRepositoryRoot(candidatePath))
            {
                return null;
            }

            string? resolvedTargetPath = null;
            if (File.Exists(candidatePath))
            {
                resolvedTargetPath = candidatePath;
            }
            else if (Directory.Exists(candidatePath))
            {
                resolvedTargetPath = TryGetDirectoryReadme(candidatePath);
            }

            if (resolvedTargetPath is null)
            {
                return null;
            }

            var repoRelativeTargetPath = GetNormalizedRelativePath(resolvedTargetPath);
            string resolvedUrl;
            if (IsReadmePath(repoRelativeTargetPath))
            {
                resolvedUrl = GetReadmeVirtualUrl(repoRelativeTargetPath);
            }
            else if (repoRelativeTargetPath.StartsWith("docs/", StringComparison.OrdinalIgnoreCase))
            {
                resolvedUrl = ConvertDocsPathToSiteUrl(repoRelativeTargetPath);
            }
            else if (isImage && _binaryAssetExtensions.Contains(Path.GetExtension(resolvedTargetPath)))
            {
                resolvedUrl = CopyGeneratedAsset(resolvedTargetPath);
            }
            else
            {
                resolvedUrl = ConvertRepoPathToGitHubUrl(repoRelativeTargetPath);
            }

            if (!string.IsNullOrEmpty(fragment))
            {
                resolvedUrl += fragment;
            }

            return useAngleBrackets ? $"<{resolvedUrl}>" : resolvedUrl;
        }

        private string CopyGeneratedAsset(string sourcePath)
        {
            var repoRelativePath = GetNormalizedRelativePath(sourcePath);
            if (_copiedAssets.TryGetValue(repoRelativePath, out var cachedUrl))
            {
                return cachedUrl;
            }

            var destinationPath = Path.Combine(_generatedAssetRoot, repoRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, overwrite: true);

            var assetUrl = $"/reference/readmes/assets/{repoRelativePath}";
            _copiedAssets[repoRelativePath] = assetUrl;
            return assetUrl;
        }

        private string GetNormalizedRelativePath(string path)
        {
            return Path.GetRelativePath(_repositoryRoot, Path.GetFullPath(path)).Replace('\\', '/');
        }

        private bool IsWithinRepositoryRoot(string path)
        {
            var relativePath = Path.GetRelativePath(_repositoryRoot, path);
            return relativePath != ".."
                && !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !relativePath.StartsWith("../", StringComparison.Ordinal);
        }

        private static bool IsReadmePath(string repoRelativePath)
        {
            var fileName = Path.GetFileName(repoRelativePath);
            return _readmeFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
        }

        private static string? TryGetDirectoryReadme(string directoryPath)
        {
            foreach (var readmeFileName in _readmeFileNames)
            {
                var candidatePath = Path.Combine(directoryPath, readmeFileName);
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }

            return null;
        }

        private static string GetReadmeTitle(string content, string repoRelativeReadmePath)
        {
            var headingMatch = _headingRegex.Match(content);
            if (headingMatch.Success)
            {
                return headingMatch.Groups[1].Value.Trim();
            }

            return string.Equals(repoRelativeReadmePath, "README.md", StringComparison.OrdinalIgnoreCase)
                ? "DotNetDiag HealthChecks"
                : Path.GetFileName(Path.GetDirectoryName(repoRelativeReadmePath.TrimEnd('/'))!) ?? repoRelativeReadmePath;
        }

        private static string GetReadmeCategory(string repoRelativeReadmePath)
        {
            if (string.Equals(repoRelativeReadmePath, "README.md", StringComparison.OrdinalIgnoreCase))
            {
                return "Repository";
            }

            var topLevelDirectory = repoRelativeReadmePath.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return topLevelDirectory?.ToLowerInvariant() switch
            {
                "build" => "Build",
                "deploy" => "Deployment",
                "extensions" => "Extensions",
                "samples" => "Samples",
                "src" => "Source Packages",
                _ => "Other"
            };
        }

        private static string GetReferenceLead(string category)
        {
            return category switch
            {
                "Extensions" => "Extension reference page",
                "Samples" => "Sample reference page",
                "Source Packages" => "Package reference page",
                _ => "Reference page"
            };
        }

        private static string GetReferenceSourcePath(string repoRelativeReadmePath)
        {
            if (string.Equals(repoRelativeReadmePath, "README.md", StringComparison.OrdinalIgnoreCase))
            {
                return ".";
            }

            return Path.GetDirectoryName(repoRelativeReadmePath)?.Replace('\\', '/') ?? repoRelativeReadmePath;
        }

        private static string GetReadmeVirtualUrl(string repoRelativeReadmePath)
        {
            var generatedPath = GetGeneratedReadmePath(repoRelativeReadmePath);
            return $"/reference/readmes/{generatedPath.Trim('/')}/";
        }

        private string GetReadmeOutputDirectory(string repoRelativeReadmePath)
        {
            var generatedPath = GetGeneratedReadmePath(repoRelativeReadmePath);
            return Path.Combine(_generatedRoot, generatedPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string GetGeneratedReadmePath(string repoRelativeReadmePath)
        {
            if (string.Equals(repoRelativeReadmePath, "README.md", StringComparison.OrdinalIgnoreCase))
            {
                return "repository";
            }

            var directoryPath = Path.GetDirectoryName(repoRelativeReadmePath)?.Replace('\\', '/') ?? string.Empty;
            return string.Join(
                '/',
                directoryPath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(SanitizePathSegment));
        }

        private static string SanitizePathSegment(string segment)
        {
            return segment.Replace(".", "-", StringComparison.Ordinal);
        }

        private static string ConvertDocsPathToSiteUrl(string repoRelativePath)
        {
            var siteRelativePath = repoRelativePath["docs/".Length..].Replace('\\', '/');

            if (string.Equals(siteRelativePath, "index.md", StringComparison.OrdinalIgnoreCase))
            {
                return "/";
            }

            if (siteRelativePath.EndsWith("/index.md", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{siteRelativePath[..^"index.md".Length].Trim('/')}/";
            }

            if (siteRelativePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{siteRelativePath[..^".md".Length].Trim('/')}/";
            }

            return $"/{siteRelativePath.TrimStart('/')}";
        }

        private string ConvertRepoPathToGitHubUrl(string repoRelativePath)
        {
            var encodedPath = string.Join('/', repoRelativePath.Split('/').Select(Uri.EscapeDataString));
            return $"{_repositoryUrl}/blob/{Uri.EscapeDataString(_branch)}/{encodedPath}";
        }

        private string ConvertRepoTreePathToGitHubUrl(string repoRelativePath)
        {
            var normalizedPath = repoRelativePath.Trim('/');
            if (string.IsNullOrEmpty(normalizedPath) || normalizedPath == ".")
            {
                return $"{_repositoryUrl}/tree/{Uri.EscapeDataString(_branch)}";
            }

            var encodedPath = string.Join('/', normalizedPath.Split('/').Select(Uri.EscapeDataString));
            return $"{_repositoryUrl}/tree/{Uri.EscapeDataString(_branch)}/{encodedPath}";
        }

        private static string EscapeYamlDoubleQuotedString(string value)
        {
            return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
        }
    }

    private sealed record GeneratedEntry(string Title, string Category, string SourcePath, string Url);

    private sealed record GeneratorOptions(string RepositoryRoot, string DocsRoot, string Branch, string RepositoryUrl)
    {
        public static GeneratorOptions Parse(IReadOnlyList<string> args)
        {
            var repositoryRoot = Directory.GetCurrentDirectory();
            string? docsRoot = null;
            var branch = "master";
            var repositoryUrl = "https://github.com/DotNetDiag/HealthChecks";

            for (var index = 0; index < args.Count; index++)
            {
                switch (args[index])
                {
                    case "--repository-root":
                        repositoryRoot = GetRequiredValue(args, ++index, "--repository-root");
                        break;
                    case "--docs-root":
                        docsRoot = GetRequiredValue(args, ++index, "--docs-root");
                        break;
                    case "--branch":
                        branch = GetRequiredValue(args, ++index, "--branch");
                        break;
                    case "--repository-url":
                        repositoryUrl = GetRequiredValue(args, ++index, "--repository-url");
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument: {args[index]}");
                }
            }

            docsRoot ??= Path.Combine(repositoryRoot, "docs");
            return new GeneratorOptions(repositoryRoot, docsRoot, branch, repositoryUrl);
        }

        private static string GetRequiredValue(IReadOnlyList<string> args, int index, string optionName)
        {
            if (index >= args.Count)
            {
                throw new ArgumentException($"Missing value for {optionName}");
            }

            return args[index];
        }
    }

    [GeneratedRegex("(?m)^#\\s+(.+?)\\s*$")]
    private static partial Regex HeadingPattern();

    [GeneratedRegex("\\[(?<image>!\\[[^\\]]*\\]\\([^)]+\\))\\]\\((?<target>[^)\\s]+)(?<title>\\s+\"[^\"]*\")?\\)")]
    private static partial Regex LinkedImageLinkPattern();

    [GeneratedRegex("!\\[(?<label>[^\\]]*)\\]\\((?<target>[^)\\s]+)(?<title>\\s+\"[^\"]*\")?\\)")]
    private static partial Regex ImagePattern();

    [GeneratedRegex("(?<!\\!)\\[(?<label>[^\\]]+)\\]\\((?<target>[^)\\s]+)(?<title>\\s+\"[^\"]*\")?\\)")]
    private static partial Regex LinkPattern();
}
