using System.Collections.Generic;
using System.IO;
using System.Linq;

using B83.Win32;

using JetBrains.Annotations;

using SixDash.Patches;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeDashCustomMusic.Patches;

[UsedImplicitly]
internal class ImportSong : IPatch {
    private static bool _hookInstalled;

    public void Apply() {
        SceneManager.sceneLoaded += (_, _) => {
            if(Object.FindObjectOfType<LevelSettings>() && !_hookInstalled) {
                _hookInstalled = true;
                UnityDragAndDropHook.InstallHook();
                UnityDragAndDropHook.OnDroppedFiles += OnDroppedFiles;
            }
            else if(_hookInstalled) {
                _hookInstalled = false;
                UnityDragAndDropHook.UninstallHook();
                UnityDragAndDropHook.OnDroppedFiles -= OnDroppedFiles;
            }
        };
    }

    private static void OnDroppedFiles(List<string> files, POINT position) {
        if(files.Count < 1)
            return;
        string file = files[0];
        string extension = Path.GetExtension(file);
        if(!CustomMusic.supportedExtensions.Contains(extension))
            return;
        string songPath = Path.ChangeExtension(SaveSelect.GetPath(LevelEditor.currentSave), extension);
        File.Copy(file, songPath, true);
        CustomMusic.LoadCurrentSong(songPath);
        LevelEditor.songId = -1;
    }
}
