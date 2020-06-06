// (c) Johannes Wolfgruber, 2020
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SystemAudioRecordingSoftware.Core.File;

namespace SystemAudioRecordingSoftware.Core.Tests
{
    internal class FilePathProviderTests
    {
        private string _uniqueFolderName = string.Empty;

        [Test]
        public void FilePathProvider_Returns_Correct__Default_Paths_When_Not_Specified()
        {
            var provider = new FilePathProvider();

            var expectedFolder = Path.Combine(Path.GetTempPath(), "SystemAudioRecordingSoftware");
            var expectedFile = Path.Combine(expectedFolder, "default.wav");

            provider.CurrentRecordingFolder.Should().Be(expectedFolder);
            provider.CurrentRecordingFile.Should().Be(expectedFile);
        }

        [Test]
        public void SetRecordingFile_With_Wav_Extension_Does_Not_Add_Wav_Extension_Twice()
        {
            var provider = new FilePathProvider();

            provider.SetRecordingFile("test.wav");

            provider.CurrentRecordingFile.Should().EndWith(".wav");
        }

        [Test]
        public void SetRecordingFile_With_Wrong_Extension_Adds_Wav_Extension_Instead()
        {
            var provider = new FilePathProvider();

            provider.SetRecordingFile("test.mp3");

            provider.CurrentRecordingFile.Should().EndWith(".wav");
        }

        [Test]
        public void SetRecordingFile_WithoutExtension_Adds_Wav_Extension()
        {
            var provider = new FilePathProvider();

            provider.SetRecordingFile("test");

            provider.CurrentRecordingFile.Should().EndWith(".wav");
        }

        [Test]
        public void SetRecordingFolder_Creates_Directory()
        {
            var provider = new FilePathProvider();
            var folder = Path.Combine(Path.GetTempPath(), _uniqueFolderName);

            Directory.Exists(folder).Should().BeFalse();

            provider.SetRecordingFolder(folder);

            Directory.Exists(folder).Should().BeTrue();
        }

        [Test]
        public void SetRecordingFolder_Sets_CurrentRecordingFolder()
        {
            var provider = new FilePathProvider();
            var folder = Path.Combine(Path.GetTempPath(), _uniqueFolderName);

            provider.SetRecordingFolder(folder);

            provider.CurrentRecordingFolder.Should().Be(folder);
        }

        [Test]
        public void SetRecordingFolder_Updates_CurrentRecordingFile()
        {
            var provider = new FilePathProvider();

            var folder = Path.Combine(Path.GetTempPath(), _uniqueFolderName);
            var file = Path.Combine(folder, "default.wav");

            provider.SetRecordingFolder(folder);

            provider.CurrentRecordingFile.Should().Be(file);
        }

        [SetUp]
        public void SetUp()
        {
            _uniqueFolderName = Guid.NewGuid().ToString();
        }

        [TearDown]
        public void TearDown()
        {
            var folder = Path.Combine(Path.GetTempPath(), _uniqueFolderName);

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder);
            }

            Directory.Exists(folder).Should().BeFalse();
        }
    }
}
