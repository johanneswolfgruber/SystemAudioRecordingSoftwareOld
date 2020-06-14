// (c) Johannes Wolfgruber, 2020

using Splat;
using Splat.NLog;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Bootstrapping
{
    public static class CoreInitialization
    {
        public static void Execute()
        {
            SetupLogging();

            Locator.CurrentMutable.RegisterLazySingleton(() => new PlaybackService(), typeof(IPlaybackService));
            Locator.CurrentMutable.RegisterLazySingleton(() => new FilePathProvider(), typeof(IFilePathProvider));
            Locator.CurrentMutable.RegisterLazySingleton(() => new RecorderService(), typeof(IRecorderService));
            Locator.CurrentMutable.RegisterLazySingleton(() => new AudioEngineService(), typeof(IAudioEngineService));
        }

        private static void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "${specialfolder:folder=Personal}/SystemAudioRecordingSoftware/system_audio_recording_software.txt" };
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            Locator.CurrentMutable.UseNLogWithWrappingFullLogger();
        }
    }
}
