// Assets/Editor/BuildInfoGenerator.cs

using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;


public static class BuildInfoGenerator
{
    [MenuItem("Tools/Generate Build Info")]
    public static void Generate() {
        string filePath = "Assets/Scripts/Generated/BuildInfo.cs";

        string version = PlayerSettings.bundleVersion;
        string buildDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        string branch = ExecuteGitCommand("rev-parse --abbrev-ref HEAD");
        string commit = ExecuteGitCommand("rev-parse --short HEAD");

        string fullVersion = $"v{version} [{branch} - {commit}]\\n{buildDate}";

        string content =
$@"public static class BuildInfo
{{
    public const string Version = ""{version}"";
    public const string BuildDate = ""{buildDate}"";
    public const string Branch = ""{branch}"";
    public const string Commit = ""{commit}"";
    public const string FullVersion = ""{fullVersion}"";    
}}";

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, content);

        AssetDatabase.Refresh();
    }

    private static string ExecuteGitCommand(string arguments) {
        Process process = new Process();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        return output;
    }
}