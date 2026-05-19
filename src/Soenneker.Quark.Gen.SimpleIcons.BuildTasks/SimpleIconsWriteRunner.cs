using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks;

///<inheritdoc cref="Abstract.ISimpleIconsWriteRunner"/>
public sealed class SimpleIconsWriteRunner : Abstract.ISimpleIconsWriteRunner
{
    private static readonly Regex _csIconPattern = new(
        @"SimpleIcon\.([A-Za-z0-9_]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _razorSimpleIconNamePattern = new(
        @"<SimpleIcon\b[^>]*\bName\s*=\s*""(?!@)([A-Za-z_][A-Za-z0-9_]*)""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> _cSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal",
        "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float",
        "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
        "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return",
        "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try",
        "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    public async ValueTask<int> Run(string[] args, CancellationToken cancellationToken)
    {
        Dictionary<string, string> map = ParseArgs(args);

        if (!map.TryGetValue("--projectDir", out string? projectDir) || string.IsNullOrWhiteSpace(projectDir))
            return Fail("Missing required --projectDir");

        projectDir = Path.GetFullPath(projectDir.Trim().Trim('"'));

        if (!Directory.Exists(projectDir))
            return Fail($"Project directory does not exist: {projectDir}");

        string outputPath = map.TryGetValue("--output", out string? outVal) && !string.IsNullOrWhiteSpace(outVal)
            ? Path.GetFullPath(outVal.Trim().Trim('"'))
            : Path.Combine(projectDir, "obj", "Generated", "SimpleIconSvgMap.g.cs");

        string resourcesDir = map.TryGetValue("--resourcesPath", out string? resPath) && !string.IsNullOrWhiteSpace(resPath)
            ? Path.GetFullPath(resPath.Trim().Trim('"'))
            : Path.Combine(projectDir, "Resources");

        HashSet<string> icons = await CollectIconsFromProject(projectDir, cancellationToken);

        string outputRoot = Path.GetDirectoryName(outputPath) ?? Directory.GetCurrentDirectory();
        string providerPath = Path.Combine(outputRoot, "SimpleIconSvgProvider.g.cs");
        string extensionsPath = Path.Combine(outputRoot, "SimpleIconServiceCollectionExtensions.g.cs");
        string hashPath = Path.Combine(outputRoot, "simpleicons-generator.inputs.hash");
        string inputHash = ComputeInputHash(projectDir, resourcesDir);

        if (CanSkipGeneration(inputHash, hashPath, outputPath, providerPath, extensionsPath))
            return 0;

        Directory.CreateDirectory(outputRoot);

        string content = await GenerateSimpleIconSvgMap(icons, resourcesDir, cancellationToken);
        await File.WriteAllTextAsync(outputPath, content, cancellationToken);
        await File.WriteAllTextAsync(providerPath, GenerateSimpleIconSvgProvider(), cancellationToken);
        await File.WriteAllTextAsync(extensionsPath, GenerateSimpleIconServiceCollectionExtensions(), cancellationToken);
        await File.WriteAllTextAsync(hashPath, inputHash, cancellationToken);

        return 0;
    }

    private static bool CanSkipGeneration(string inputHash, string hashPath, string outputPath, string providerPath, string extensionsPath)
    {
        if (!File.Exists(outputPath) || !File.Exists(providerPath) || !File.Exists(extensionsPath) || !File.Exists(hashPath))
            return false;

        string previousHash = File.ReadAllText(hashPath).Trim();
        return string.Equals(previousHash, inputHash, StringComparison.Ordinal);
    }

    private static string ComputeInputHash(string projectDir, string resourcesDir)
    {
        var entries = new List<string>();
        AddFileMetadataEntries(entries, projectDir, ".cs");
        AddFileMetadataEntries(entries, projectDir, ".razor");
        AddFileMetadataEntries(entries, resourcesDir, ".svg");

        string assemblyLocation = typeof(SimpleIconsWriteRunner).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(assemblyLocation) && File.Exists(assemblyLocation))
            entries.Add(BuildMetadataEntry("buildtasks", assemblyLocation, "buildtasks"));

        entries.Sort(StringComparer.Ordinal);

        string manifest = string.Join('\n', entries);
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(manifest));
        return Convert.ToHexString(bytes);
    }

    private static void AddFileMetadataEntries(List<string> entries, string rootDir, string extension)
    {
        if (!Directory.Exists(rootDir))
            return;

        foreach (string file in Directory.EnumerateFiles(rootDir, "*" + extension, SearchOption.AllDirectories))
        {
            if (IsExcludedProjectPath(file))
                continue;

            entries.Add(BuildMetadataEntry(rootDir, file, extension));
        }
    }

    private static string BuildMetadataEntry(string rootDir, string filePath, string category)
    {
        var info = new FileInfo(filePath);
        string relativePath = Path.GetRelativePath(rootDir, filePath).Replace('\\', '/');
        return string.Create(CultureInfo.InvariantCulture, $"{category}|{relativePath}|{info.Length}|{info.LastWriteTimeUtc.Ticks}");
    }

    private static bool IsExcludedProjectPath(string path)
    {
        return path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/bin/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<HashSet<string>> CollectIconsFromProject(string projectDir, CancellationToken ct)
    {
        var icons = new HashSet<string>(StringComparer.Ordinal);
        IEnumerable<string> files = Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(projectDir, "*.razor", SearchOption.AllDirectories))
            .Where(p => !IsExcludedProjectPath(p));

        foreach (string file in files)
        {
            ct.ThrowIfCancellationRequested();

            string content = await File.ReadAllTextAsync(file, ct);

            foreach (Match match in _csIconPattern.Matches(content))
            {
                if (match.Success && match.Groups.Count >= 2)
                    icons.Add(match.Groups[1].Value);
            }

            if (Path.GetExtension(file).Equals(".razor", StringComparison.OrdinalIgnoreCase))
            {
                foreach (Match match in _razorSimpleIconNamePattern.Matches(content))
                {
                    if (match.Success && match.Groups.Count >= 2)
                        icons.Add(match.Groups[1].Value);
                }
            }
        }

        return icons;
    }

    private static async Task<string> GenerateSimpleIconSvgMap(HashSet<string> iconNames, string resourcesDir, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Soenneker.Quark.Gen.SimpleIcons.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Maps Simple Icons enum names to SVG content from Soenneker.SimpleIcons.Icons.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static partial class SimpleIconSvgMap");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Returns the SVG markup for the given Simple Icons enum name, or null if not found.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static string? GetSvg(string iconName)");
        sb.AppendLine("    {");
        sb.AppendLine("        return iconName switch");
        sb.AppendLine("        {");

        foreach (string iconName in iconNames.OrderBy(x => x, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string resourceName = ToResourceName(iconName);
            string path = Path.Combine(resourcesDir, resourceName + ".svg");
            if (!File.Exists(path))
                continue;

            string svgContent = await File.ReadAllTextAsync(path, cancellationToken);
            string escaped = EscapeForCSharpString(svgContent);
            sb.Append("            \"").Append(iconName).Append("\" => \"").Append(escaped).AppendLine("\",");
        }

        sb.AppendLine("            _ => null");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string ToResourceName(string iconName)
    {
        string resourceName = iconName;

        if (resourceName.StartsWith("Icon", StringComparison.Ordinal) &&
            resourceName.Length > 4 &&
            char.IsDigit(resourceName[4]))
        {
            resourceName = resourceName[4..];
        }

        if (resourceName.EndsWith("Icon", StringComparison.Ordinal) &&
            resourceName.Length > 4)
        {
            string candidate = resourceName[..^4].ToLowerInvariant();
            if (_cSharpKeywords.Contains(candidate))
                resourceName = resourceName[..^4];
        }

        return resourceName.ToLowerInvariant();
    }

    private static string EscapeForCSharpString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n")
            .Replace("\r", "\\n");
    }

    private static string GenerateSimpleIconSvgProvider()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using Soenneker.Quark.Gen.SimpleIcons.Abstractions;");
        sb.AppendLine();
        sb.AppendLine("namespace Soenneker.Quark.Gen.SimpleIcons.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Implements <see cref=\"ISimpleIconsSvgProvider\"/> using the generated <see cref=\"SimpleIconSvgMap\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal sealed class SimpleIconSvgProvider : ISimpleIconsSvgProvider");
        sb.AppendLine("{");
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    public string? GetSvg(string iconName) => SimpleIconSvgMap.GetSvg(iconName);");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GenerateSimpleIconServiceCollectionExtensions()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        sb.AppendLine("using Soenneker.Quark.Gen.SimpleIcons.Abstractions;");
        sb.AppendLine();
        sb.AppendLine("namespace Soenneker.Quark.Gen.SimpleIcons.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Extension methods for registering the generated Simple Icons SVG provider.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class SimpleIconServiceCollectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Registers <see cref=\"ISimpleIconsSvgProvider\"/> and <see cref=\"SimpleIconSvgProvider\"/> as scoped.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IServiceCollection AddSimpleIconsAsScoped(this IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine("        services.TryAddScoped<ISimpleIconsSvgProvider, SimpleIconSvgProvider>();");
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--", StringComparison.Ordinal) && i + 1 < args.Length)
            {
                map[args[i]] = args[i + 1];
                i++;
            }
        }
        return map;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"Soenneker.Quark.Gen.SimpleIcons.BuildTasks: {message}");
        return 1;
    }
}
