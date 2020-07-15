// (c) Johannes Wolfgruber, 2020
using ReactiveUI;
using SystemAudioRecordingSoftware.UI.ViewModels;

namespace SystemAudioRecordingSoftware.UI.Views
{
    /// <summary>
    /// Interaction logic for RecordingView.xaml
    /// </summary>
    public partial class RecordingView : RecordingViewBase
    {
        public RecordingView()
        {
            InitializeComponent();
        }
    }

    public class RecordingViewBase : ReactiveUserControl<RecordingViewModel> { }
}
