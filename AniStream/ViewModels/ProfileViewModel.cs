﻿using System.Threading.Tasks;
using AniStream.ViewModels.Framework;
using AniStream.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace AniStream.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    [RelayCommand]
    async Task LoginWithAnilist()
    {
        await Shell.Current.GoToAsync(nameof(AnilistLoginView));
        return;

        var clientID = "14733";

        var result = await Browser
            .Default
            .OpenAsync(
                $"https://anilist.co/api/v2/oauth/authorize?client_id={clientID}&response_type=token"
            );
    }
}
