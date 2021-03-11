// (c) Johannes Wolfgruber, 2020

using Autofac;
using AutoMapper;
using NAudio.Wave;
using ReactiveUI;
using Splat;
using Splat.Autofac;
using System;
using System.Reflection;
using SystemAudioRecordingSoftware.Application.Interfaces;
using SystemAudioRecordingSoftware.Application.Services;
using SystemAudioRecordingSoftware.Domain.Model;
using SystemAudioRecordingSoftware.Infrastructure.Services;

namespace SystemAudioRecordingSoftware.Infrastructure.Bootstrapping
{
    public class AppBootstrapper
    {
        public AppBootstrapper(Assembly assembly)
        {
            var containerBuilder = new ContainerBuilder();

            var logger = new DebugLogger {Level = LogLevel.Error};
            containerBuilder.RegisterInstance<ILogger>(logger);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Recording, RecordingDto>();
                cfg.CreateMap<RecordingDto, Recording>();

                cfg.CreateMap<Track, TrackDto>();
                cfg.CreateMap<TrackDto, Track>();
            });

            var mapper = configuration.CreateMapper();
            containerBuilder.RegisterInstance(mapper);

            containerBuilder.Register<Func<IWavePlayer>>(_ => () => new WaveOutEvent {DesiredLatency = 200});
            containerBuilder.RegisterType<DisplayDataProvider>().As<IDisplayDataProvider>();
            containerBuilder.RegisterType<SnippingService>().As<ISnippingService>();
            containerBuilder.RegisterType<RecordsRepository>().As<IRecordsRepository>().SingleInstance();
            containerBuilder.RegisterType<RecordsService>().As<IRecordsService>().SingleInstance();
            containerBuilder.RegisterType<RecordingService>().As<IRecordingService>().SingleInstance();
            containerBuilder.RegisterType<PlaybackService>().As<IPlaybackService>().SingleInstance();

            var autofacResolver = containerBuilder.UseAutofacDependencyResolver();
            containerBuilder.RegisterInstance(autofacResolver);

            Locator.CurrentMutable.InitializeSplat();
            Locator.CurrentMutable.InitializeReactiveUI();

            Locator.CurrentMutable.RegisterViewsForViewModels(assembly);

            var container = containerBuilder.Build();
            autofacResolver.SetLifetimeScope(container);
        }
    }
}