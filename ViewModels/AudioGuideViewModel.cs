using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TravelGuideApp.Models;
using TravelGuideApp.Services;

namespace TravelGuideApp.ViewModels
{
    public partial class AudioGuideViewModel : BaseViewModel
    {
        private readonly NarrationService _narrationService;

        [ObservableProperty]
        POI item;

        [ObservableProperty]
        string statusText;

        public AudioGuideViewModel(NarrationService narrationService)
        {
            Title = "Audio Guide";
            _narrationService = narrationService;
        }

        public void Init(POI poi)
        {
            Item = poi;
            StatusText = $"Bạn đang đến {poi.Name}...";
            PlayAudioCommand.Execute(null);
        }

        [RelayCommand]
        public async Task PlayAudioAsync()
        {
            if (Item != null)
            {
                await _narrationService.SpeakAsync(Item.Description);
            }
        }

        [RelayCommand]
        public void StopAudio()
        {
            _narrationService.CancelSpeech();
        }
    }
}
