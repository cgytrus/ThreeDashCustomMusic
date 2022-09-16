using System;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using SixDash.Patches;

using UnityEngine;

namespace ThreeDashCustomMusic.Patches;

[UsedImplicitly]
internal class LoadSong : IPatch {
    private static string _songPath = string.Empty;

    [Serializable]
    private class FakeLevel : Level {
#pragma warning disable 0649
        public string? songExtension;
        public string? songData;
#pragma warning restore 0649
    }

    public void Apply() {
        Local();
        Online();
        On.LevelEditor.Awake += (orig, self) => {
            orig(self);
            if(!string.IsNullOrEmpty(_songPath))
                CustomMusic.LoadCurrentSong(_songPath);
        };
    }

    private static void Local() {
        On.LevelEditor.LoadLevel += (orig, number) => {
            orig(number);
            string levelPath = SaveSelect.GetPath(number);
            foreach(string songPath in CustomMusic.supportedExtensions.Select(ext =>
                Path.ChangeExtension(levelPath, ext))) {
                if(!File.Exists(songPath))
                    continue;
                _songPath = songPath;
                return;
            }
            _songPath = string.Empty;
        };

        On.SaveSelect.DeleteFile += (orig, number) => {
            orig(number);
            if(File.Exists(_songPath))
                File.Delete(_songPath);
        };
    }

    private static void Online() {
        On.OnlineLevelsHub.LoadLevel += (orig, self, id) => {
            if(!self.activated)
                _songPath = string.Empty;
            orig(self, id);
        };

        On.LevelEditor.JSONToLevel += (orig, self, json) => {
            FakeLevel fakeLevel = JsonUtility.FromJson<FakeLevel>(json);
            if(fakeLevel.songExtension is null || fakeLevel.songData is null)
                return orig(self, json);
            _songPath = CustomMusic.GetCustomSongPath(fakeLevel.songExtension);
            if(File.Exists(_songPath))
                return orig(self, json);
            File.WriteAllBytes(_songPath, Convert.FromBase64String(fakeLevel.songData));
            return orig(self, json);
        };

        On.LevelEditor.LevelToJSON += (orig, self, level) => {
            if(string.IsNullOrEmpty(_songPath) || level.songId >= 0)
                return orig(self, level);
            return orig(self, new FakeLevel {
                name = level.name,
                author = level.author,
                difficulty = level.difficulty,
                songId = level.songId,
                songStartTime = level.songStartTime,
                floorId = level.floorId,
                backgroundId = level.backgroundId,
                startingColor = level.startingColor,
                levelData = level.levelData,
                pathData = level.pathData,
                cameraData = level.cameraData,
                songExtension = Path.GetExtension(_songPath),
                songData = Convert.ToBase64String(File.ReadAllBytes(_songPath))
            });
        };
    }
}
