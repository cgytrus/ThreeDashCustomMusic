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
    private static string _currentSongPath = string.Empty;
    private static DateTime _currentSongFileTime = DateTime.MinValue;

    private static ManualLogSource? _logger;

    internal static void Setup(ManualLogSource logger) => _logger = logger;

    public static string GetCustomSongPath(string extension) => GetCustomSongPath(LevelEditor.currentID, extension);

    public static string GetCustomSongPath(int id, string extension) => Path.Combine(Application.persistentDataPath,
        Path.ChangeExtension($"level_{id}_song", extension));

    public static void LoadCurrentSong(string path) {
        _logger?.LogInfo($"Loading custom song at {path}");

        if(!File.Exists(path)) {
            currentSong = null;
            _currentSongPath = string.Empty;
            _currentSongFileTime = DateTime.MinValue;
            _logger?.LogError("File does not exist");
            return;
        }

        DateTime writeTime = new FileInfo(path).LastWriteTimeUtc;
        if(_currentSongPath == path && writeTime == _currentSongFileTime) {
            _logger?.LogInfo($"File didn't change, not reloading song (last modification time: {writeTime})");
            return;
        }

        string fullPath = $"file://{path}";
        string extension = Path.GetExtension(path);
        AudioType audioType = ExtensionToAudioType(extension);
        if(audioType == AudioType.UNKNOWN) {
            currentSong = null;
            _currentSongPath = string.Empty;
            _currentSongFileTime = DateTime.MinValue;
            _logger?.LogError("Unknown audio type");
            return;
        }

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType);
        UnityWebRequestAsyncOperation requestOperation = request.SendWebRequest();
        while(!requestOperation.isDone) { }

        if(request.result != UnityWebRequest.Result.Success) {
            currentSong = null;
            _currentSongPath = string.Empty;
            _currentSongFileTime = DateTime.MinValue;
            _logger?.LogError(request.error);
            return;
        }

        currentSong = DownloadHandlerAudioClip.GetContent(request);
        _currentSongPath = path;
        _currentSongFileTime = writeTime;
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
