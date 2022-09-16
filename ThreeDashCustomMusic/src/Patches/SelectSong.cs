using JetBrains.Annotations;

using SixDash.Patches;

namespace ThreeDashCustomMusic.Patches;

[UsedImplicitly]
internal class SelectSong : IPatch {
    public void Apply() {
        Song();
        SongText();
        SongButton();
    }

    private static void Song() {
        On.LevelEditor.Awake += (orig, self) => {
            bool usingCustomSong = LevelEditor.songId < 0;
            if(usingCustomSong)
                LevelEditor.songId = 0;

            orig(self);

            if(!usingCustomSong)
                return;
            LevelEditor.songId = -1;
            LevelEditor.song = CustomMusic.currentSong;
        };

        On.LevelEditor.Update += (orig, self) => {
            bool usingCustomSong = LevelEditor.songId < 0;
            if(usingCustomSong)
                LevelEditor.songId = 0;

            orig(self);

            if(!usingCustomSong)
                return;
            LevelEditor.songId = -1;
            LevelEditor.song = CustomMusic.currentSong;
        };
    }

    private static void SongText() {
        On.LevelSettings.UpdateVisuals += (orig, self) => {
            bool usingCustomSong = LevelEditor.songId < 0;
            if(usingCustomSong)
                LevelEditor.songId = 0;

            orig(self);

            if(!usingCustomSong)
                return;
            LevelEditor.songId = -1;
            self.songText.text = "Custom";
            self.songAuthorText.text = string.Empty;
        };
    }

    private static void SongButton() {
        On.LevelSettings.songButton += (_, self, modifier) => {
            LevelEditor.songId += modifier;
            LevelEditor.songId = self.loop(LevelEditor.songId, -1, self.songNames.Length - 1);
        };
    }
}
