﻿using System;
using Berry.Maui.Core;
using Jita.AniList.Models;
using Juro.Core.Models.Anime;
using Juro.Core.Models.Videos;

namespace AniStream.ViewModels;

public class PlatformMediaController : IDisposable
{
    public PlatformMediaController(
        VideoPlayerViewModel playerViewModel,
        IAnimeInfo anime,
        Episode episode,
        VideoServer videoServer,
        Media media
    ) { }

    public void OnLoaded(IMediaElement mediaElement) { }

    public void Dispose() { }
}
