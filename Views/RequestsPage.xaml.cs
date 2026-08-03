using PosterFly.ViewModels;

namespace PosterFly.Views;

public partial class RequestsPage : ContentPage
{
    public RequestsPage(RequestsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}