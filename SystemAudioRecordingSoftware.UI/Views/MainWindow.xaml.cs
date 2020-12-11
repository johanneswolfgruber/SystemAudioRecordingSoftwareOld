// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using System.Reactive.Disposables;
using SystemAudioRecordingSoftware.UI.ViewModels;

namespace SystemAudioRecordingSoftware.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
            DataContext = ViewModel;

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel,
                        viewModel => viewModel.Title,
                        view => view.Title)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                        viewModel => viewModel.Title,
                        view => view.TitleText.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.RecordCommand,
                        view => view.RecordButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.StopCommand,
                        view => view.StopButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.SnipCommand,
                        view => view.SnipButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.BurnCommand,
                        view => view.BurnButton)
                    .DisposeWith(disposables);

                // this.Bind(ViewModel,
                //         viewModel => viewModel.AudioData,
                //         view => view.WaveformControl.DisplayAudioData)
                //     .DisposeWith(disposables);
                //
                // this.Bind(ViewModel,
                //         viewModel => viewModel.LengthInSeconds,
                //         view => view.WaveformControl.LengthInSeconds)
                //     .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.RecordingsList,
                        view => view.RecordingsList.ViewModel)
                    .DisposeWith(disposables);
            });
        }
    }

    public class MainWindowBase : ReactiveWindow<MainWindowViewModel>
    {
        /* No code needed here */
    }
}