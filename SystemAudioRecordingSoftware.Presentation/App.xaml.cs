// (c) Johannes Wolfgruber, 2020

using System.Reflection;
using SystemAudioRecordingSoftware.Infrastructure.Bootstrapping;

namespace SystemAudioRecordingSoftware.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            var _ = new AppBootstrapper(Assembly.GetCallingAssembly());
        }
    }
}