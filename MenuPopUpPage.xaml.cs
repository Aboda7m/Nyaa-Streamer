using CommunityToolkit.Maui.Views;

namespace Nyaa_Streamer;

public partial class MenuPopUpPage : ContentPage
{
    private MainPage _mainPage;
    public MenuPopUpPage(MainPage mainPage)
    {
		InitializeComponent();
        _mainPage = mainPage;
    }

    private async void OnOption1Clicked(object sender, EventArgs e)
    {
        // Call the method from MainPage
        _mainPage.OnDebugClearCacheClicked();
        await Navigation.PopModalAsync(); // Optionally close the popup after the action
    }

    private async void OnOption2Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}