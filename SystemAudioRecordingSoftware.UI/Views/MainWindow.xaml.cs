// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;
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
                
                this.BindCommand(ViewModel,
                        viewModel => viewModel.SnipCommand,
                        view => view.SnipButton)
                    .DisposeWith(disposableRegistration);
                
                this.BindCommand(ViewModel,
                        viewModel => viewModel.BurnCommand,
                        view => view.BurnButton)
                    .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.DeleteCommand,
                        view => view.DeleteButton)
                    .DisposeWith(disposableRegistration);

                    this.OneWayBind(ViewModel,
                    viewModel => viewModel.WaveformRenderer,
                    view => view.MainContent.Content)
                    .DisposeWith(disposableRegistration);
            });
        }
        
        /// <summary>
        /// TreesView's SelectedItem is read-only. Hence we can't bind it. There is a way to obtain a selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel?.OnSelectedItemChanged(e.NewValue);
        }
    }

    public class MainWindowBase : ReactiveWindow<MainWindowViewModel> { /* No code needed here */ }
}
