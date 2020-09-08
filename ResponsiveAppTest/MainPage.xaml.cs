using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ResponsiveAppTest
{
    public sealed partial class MainPage : Page
    {
        private FakeServer server;

        public MainPage()
        {
            this.InitializeComponent();
            server = new FakeServer();
        }

        public async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Download());
        }

        private void UpdateUI(double progress)
        {
            Debug.WriteLine($"progress: {progress}");
            ProgressBar.Value = progress; // dovoljno, jer se sigurno izvrsava na UI niti
        }

        private async void ScheduleUI(DispatchedHandler function)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, function);
        }

        private async void Download()
        {
            bool done = false;
            StorageFolder local = ApplicationData.Current.LocalFolder;
            StorageFile file = await local.CreateFileAsync("temp", CreationCollisionOption.ReplaceExisting);
            int fileSize = await Task.Run(() => server.BytesRemaining());
            int downloadedBytes = 0;

            var timer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                ScheduleUI(() =>
                {
                    double downloaded = downloadedBytes;                // kopiranje promjenljivih zbog konteksta lambde
                    double progress = downloaded / fileSize * 100;
                    UpdateUI(progress);
                });
            }, TimeSpan.FromSeconds(1));


            while (!done)
            {
                int bytesRemaining = await Task.Run(() => server.BytesRemaining());
                if (bytesRemaining == 0)
                {
                    done = true;
                    timer.Cancel();
                    break;
                }
                else
                {
                    byte[] content = await Task.Run(() => server.GetBytes(10)); // kako optimizovati download?
                    downloadedBytes += content.Length;

                    await Task.Run(async () =>
                    {
                        using (var stream = await file.OpenStreamForWriteAsync())
                        {
                            stream.Seek(0, SeekOrigin.End);
                            await stream.WriteAsync(content, 0, content.Length);
                        }
                    });
                }
            }
        }
    }
}
