using PosterFly.ViewModels;

namespace PosterFly.Views;

public partial class RequestsPage : ContentPage
{
    public RequestsPage(RequestsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnHeadersToggleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && HeadersContainer != null)
        {
            HeadersContainer.IsVisible = !HeadersContainer.IsVisible;
            button.Text = HeadersContainer.IsVisible ? "▲ Response Headers" : "▼ Response Headers";
        }
    }
}