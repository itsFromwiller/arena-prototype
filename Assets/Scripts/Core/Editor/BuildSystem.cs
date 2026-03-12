using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Arena.Core.Builds
{
    public class WebGLAutoDeployToGIT : EditorWindow
    {
        private static string buildPath = "Builds/WebGL"; // Folder where the build will go

        [MenuItem("Build/Build WebGL and Push to GitHub")]
        public static void BuildAndPush()
        {
            // 1. Run the WebGL Build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log("Build succeeded! Starting Git push...");
                RunGitCommands();
            }
            else
            {
                UnityEngine.Debug.LogError("Build failed!");
            }
        }

        private static void RunGitCommands()
        {
            // 2. Execute Git commands sequentially
            // Ensure you have Git installed and accessible in your system PATH
            RunCommand("git", "add .");
            RunCommand("git", "commit -m \"Automated WebGL Build: " + System.DateTime.Now.ToString() + "\"");
            RunCommand("git", "push origin main"); // Ensure 'main' matches your branch name

            UnityEngine.Debug.Log("Git commands executed successfully!");
        }

        private static void RunCommand(string fileName, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(Application.dataPath), // Project root
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(error) && !error.Contains("warning"))
                    UnityEngine.Debug.LogWarning("Git Error/Warning: " + error);
            }
        }
    }
}