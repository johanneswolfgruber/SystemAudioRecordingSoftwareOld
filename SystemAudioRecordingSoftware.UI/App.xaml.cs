// (c) Johannes Wolfgruber, 2020
using Prism.Ioc;
using System.Windows;
using SystemAudioRecordingSoftware.Core.Bootstrapping;
using SystemAudioRecordingSoftware.UI.Views;

namespace SystemAudioRecordingSoftware.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            CoreInitialization.Execute(containerRegistry);
        }
    }
}
