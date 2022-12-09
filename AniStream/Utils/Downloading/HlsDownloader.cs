﻿using System;
using System.Linq;
using System.Collections.Specialized;
using Android.App;
using Xamarin.Essentials;
using System.Threading.Tasks;
using AnimeDl;
using Laerdal.FFmpeg.Android;
using DotNetTools.JGrabber.Grabbed;
using Android.Content;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using System.Threading;
using AniStream.Services;

namespace AniStream.Utils.Downloading;

public class HlsDownloader
{
    private readonly Service _service;
    private readonly AnimeClient _client = new(WeebUtils.AnimeSite);
    private readonly int _notificationId;

    public HlsDownloader(Service service)
    {
        _service = service;
        _notificationId = (int)DateTime.Now.Ticks;
    }

    public async Task DownloadAsync(
        string fileName,
        GrabbedHlsStream stream,
        NameValueCollection headers,
        CancellationToken cancellationToken = default)
    {
        var cacheDir = FileSystem.CacheDirectory;

        var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);

        var filePath = System.IO.Path.Combine(cacheDir, $"{fileNameWithoutExtension}.ts");
        var newFilePath = System.IO.Path.Combine(cacheDir, fileName);
        var saveFilePath = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)!.AbsolutePath, $"{fileNameWithoutExtension}.mp4");

        using (var progress = new DownloaderProgress(_service, _notificationId, fileName))
        {
            try
            {
                await _client.DownloadTsAsync(stream, headers, filePath, progress, cancellationToken);
            }
            catch
            {
                //Cancelled from cancellationToken

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                return;
            }
        }

        ShowProcessingNotification(fileName);

        //This allows ffmpeg to return result and continue operations in app
        Config.IgnoreSignal(Laerdal.FFmpeg.Android.Signal.Sigxcpu);

        if (cancellationToken.IsCancellationRequested)
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            if (System.IO.File.Exists(newFilePath))
                System.IO.File.Delete(newFilePath);

            return;
        }

        var cmd = $@"-i ""{filePath}"" -acodec copy -vcodec copy ""{newFilePath}""";
        var returnCode = FFmpeg.Execute(cmd);
        if (returnCode == Config.ReturnCodeSuccess)
        {
            await _service.CopyFileUsingMediaStore(newFilePath, saveFilePath, cancellationToken);
            
            if (!cancellationToken.IsCancellationRequested)
                ShowCompletedNotification(fileName, "Completed");
        }
        else
        {
            if (!cancellationToken.IsCancellationRequested)
                ShowCompletedNotification(fileName, "Failed to convert video");
        }

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        if (System.IO.File.Exists(newFilePath))
            System.IO.File.Delete(newFilePath);

        if (cancellationToken.IsCancellationRequested && System.IO.File.Exists(saveFilePath))
            System.IO.File.Delete(saveFilePath);
    }

    private void ShowProcessingNotification(string title)
    {
        var intent = new Intent(_service, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(_service, 0, intent, PendingIntentFlags.Immutable);

        var cancelIntent = new Intent(_service, typeof(DownloadService));
        cancelIntent.SetAction("cancel_download");
        var cancelPendingIntent = PendingIntent.GetForegroundService(_service, 3462, cancelIntent, PendingIntentFlags.UpdateCurrent);

        var channelId = $"{_service.PackageName}.general";

        var color = ContextCompat.GetColor(_service, Resource.Color.colorPrimary);

        var builder = new NotificationCompat.Builder(_service, channelId)
            .SetSmallIcon(Resource.Drawable.logo_notification)
            .SetOngoing(true)
            .SetContentTitle(title)
            .SetContentText("Converting video")
            .SetContentIntent(pendingIntent)
            .SetProgress(100, 100, true)
            .SetColor(color)
            .AddAction(Resource.Drawable.logo_notification, "Cancel", cancelPendingIntent)
            .SetPriority(NotificationCompat.PriorityLow);

        _service.StartForeground(_notificationId, builder.Build());
    }

    private void ShowCompletedNotification(string title, string message)
    {
        var intent = new Intent(_service, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(_service, 0, intent, PendingIntentFlags.Immutable);

        var channelId = $"{_service.PackageName}.general";

        var color = ContextCompat.GetColor(_service, Resource.Color.colorPrimary);

        var builder = new NotificationCompat.Builder(_service, channelId)
            .SetSmallIcon(Resource.Drawable.logo_notification)
            .SetOngoing(false)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetContentIntent(pendingIntent)
            .SetColor(color)
            .SetPriority(NotificationCompat.PriorityLow);

        var notificationId = (int)DateTime.Now.Ticks;

        var notificationManager = NotificationManagerCompat.From(_service);
        //var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        notificationManager.Notify(notificationId, builder.Build());

        _service.StopSelf();
    }
}