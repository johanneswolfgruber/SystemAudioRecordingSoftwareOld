// (c) Johannes Wolfgruber, 2020

using ReactiveUI;
using Splat;
using System.Reflection;
using System.Windows;
using SystemAudioRecordingSoftware.Core.Bootstrapping;

namespace SystemAudioRecordingSoftware.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            CoreInitialization.Execute();

            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
        }
    }
}
