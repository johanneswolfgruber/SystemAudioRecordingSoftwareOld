// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using System.Reactive.Disposables;
using SystemAudioRecordingSoftware.UI.ViewModels;

namespace SystemAudioRecordingSoftware.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MainWindowBase
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();

            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.Title,
                    view => view.Title)
                    .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.RecordCommand,
                    view => view.RecordButton)
                    .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.StopCommand,
                    view => view.StopButton)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Visualization,
                    view => view.VisualizationContent.Content)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Recordings,
                    view => view.RecordingsList.ItemsSource)
                    .DisposeWith(disposableRegistration);
            });
        }
    }

    public class MainWindowBase : ReactiveWindow<MainWindowViewModel> { /* No code needed here */ }
}
