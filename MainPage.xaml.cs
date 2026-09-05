namespace PosterFly;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnStartClicked(object? sender, EventArgs e)
	{
		await DisplayAlert("Poster Fly", "Welcome! API testing features coming soon.", "OK");
	}
}
