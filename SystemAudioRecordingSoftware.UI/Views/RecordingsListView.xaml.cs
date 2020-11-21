// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows;
using SystemAudioRecordingSoftware.UI.ViewModels;

namespace SystemAudioRecordingSoftware.UI.Views
{
    /// <summary>
    /// Interaction logic for RecordingView.xaml
    /// </summary>
    public partial class RecordingsListView
    {
        public RecordingsListView()
        {
            InitializeComponent();

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                {
                    throw new InvalidOperationException("The ViewModel must be set before activating the View");
                }

                this.BindCommand(ViewModel,
                        viewModel => viewModel.ImportCommand,
                        view => view.ImportButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.ExportCommand,
                        view => view.ExportButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.DeleteCommand,
                        view => view.DeleteButton)
                    .DisposeWith(disposables);
            });
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel?.OnSelectedItemChanged(e.NewValue);
        }
    }

    public class RecordingsListViewBase : ReactiveUserControl<RecordingsListViewModel>
    {
    }
}