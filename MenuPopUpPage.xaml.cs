using CommunityToolkit.Maui.Views;

namespace Nyaa_Streamer;

public partial class MenuPopUpPage : ContentPage
{
	public MenuPopUpPage()
	{
		InitializeComponent();
	}

    private void OnOption1Clicked(object sender, EventArgs e)
    {

    }

    private void OnOption2Clicked(object sender, EventArgs e)
    {

    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}