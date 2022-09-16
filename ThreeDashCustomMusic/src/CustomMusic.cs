using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BepInEx.Logging;

using JetBrains.Annotations;

using SixDash.API;

using UnityEngine;
using UnityEngine.Networking;

namespace ThreeDashCustomMusic;

[PublicAPI]
public static class CustomMusic {
    public static IReadOnlyCollection<string> supportedExtensions = new string[] {
        ".acc", ".aiff", ".it", ".mod", ".mp2", ".mp3", ".ogg", ".s3m", ".wav", ".xm", ".xma", ".vag"
    };
    public static AudioClip? currentSong { get; private set; }

    private static ManualLogSource? _logger;

    internal static void Setup(ManualLogSource logger) => _logger = logger;

    public static string GetCustomSongPath(string extension) => GetCustomSongPath(LevelEditor.currentID, extension);

    public static string GetCustomSongPath(int id, string extension) => Path.Combine(Application.persistentDataPath,
        Path.ChangeExtension($"level_{id}_song", extension));

    public static void LoadCurrentSong(string path) {
        _logger?.LogInfo($"Loading custom song at {path}");
        currentSong = null;

        if(!File.Exists(path)) {
            _logger?.LogError("File does not exist");
            return;
        }

        string fullPath = $"file://{path}";
        string extension = Path.GetExtension(path);
        AudioType audioType = ExtensionToAudioType(extension);
        if(audioType == AudioType.UNKNOWN) {
            _logger?.LogError("Unknown audio type");
            return;
        }

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType);
        UnityWebRequestAsyncOperation requestOperation = request.SendWebRequest();
        while(!requestOperation.isDone) { }

        if(request.result != UnityWebRequest.Result.Success) {
            _logger?.LogError(request.error);
            return;
        }

        currentSong = DownloadHandlerAudioClip.GetContent(request);
    }

    public static AudioType ExtensionToAudioType(string extension) => extension switch {
        ".acc" => AudioType.ACC,
        ".aiff" => AudioType.AIFF,
        ".it" => AudioType.IT,
        ".mod" => AudioType.MOD,
        ".mp2" or ".mp3" => AudioType.MPEG,
        ".ogg" => AudioType.OGGVORBIS,
        ".s3m" => AudioType.S3M,
        ".wav" => AudioType.WAV,
        ".xm" => AudioType.XM,
        ".xma" => AudioType.XMA,
        ".vag" => AudioType.VAG,
        _ => AudioType.UNKNOWN
    };
}
