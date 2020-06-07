// (c) Johannes Wolfgruber, 2020
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using SystemAudioRecordingSoftware.Core.Audio;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Bootstrapping
{
    public static class CoreInitialization
    {
        public static void Execute(IContainerRegistry container)
        {
            container.RegisterSingleton<IPlaybackService, PlaybackService>();
            container.RegisterSingleton<IFilePathProvider, FilePathProvider>();
            container.RegisterSingleton<IRecorderService, RecorderService>();
            container.RegisterSingleton<IAudioEngineService, AudioEngineService>();
        }
    }
}
