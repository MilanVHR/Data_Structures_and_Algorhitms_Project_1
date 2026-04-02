using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;

namespace Project.Services
{
    public class UiSoundPlayer : IUiSoundPlayer
    {
        private readonly string _clickSoundPath;
        private readonly string _errorSoundPath;

        public UiSoundPlayer(string baseDirectory)
        {
            _clickSoundPath = Path.Combine(baseDirectory, "Sounds", "sound-4.wav");
            _errorSoundPath = Path.Combine(baseDirectory, "Sounds", "universfield-error-08-206492.wav");
        }

        public void PlayClick()
        {
            Play(_clickSoundPath);
        }

        public void PlayError()
        {
            Play(_errorSoundPath);
        }

        private static void Play(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var player = new SoundPlayer(filePath);
                    player.Play();
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch
            {
                // Sound playback should never crash the app.
            }
        }
    }
}
