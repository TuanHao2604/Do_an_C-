using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Media;

namespace TravelGuideApp.Services
{
    public class NarrationService
    {
        private CancellationTokenSource _cts;

        public async Task SpeakAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
                {
                    Volume = 1.0f
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing TTS: {ex.Message}");
            }
        }

        public void CancelSpeech()
        {
            try
            {
                _cts?.Cancel();
            }
            catch (Exception)
            {
            }
        }
    }
}
