using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.Linq;

public class GameBuilder : MonoBehaviour
{
    [MenuItem("Build/Windows/Debug")]
    public static void BuildWindowsDebug()
    {
        BuildOptions buildOptions = BuildOptions.AllowDebugging |
                                     BuildOptions.ConnectWithProfiler |
                                     BuildOptions.Development;
        Build("debug", BuildTarget.StandaloneWindows64, buildOptions);
    }

    [MenuItem("Build/Windows/Release")]
    public static void BuildWindowsRelease()
    {
        BuildOptions buildOptions = BuildOptions.None;
        Build("release", BuildTarget.StandaloneWindows64, buildOptions);
    }

    public static void Build(string pathName, BuildTarget buildTarget, BuildOptions buildOptions)
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = buildOptions;
        buildPlayerOptions.locationPathName = $"build/windows/{pathName}/{Application.productName}.exe";

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}