using PosterFly.ViewModels;

namespace PosterFly.Views;

public partial class CollectionsPage : ContentPage
{
    public CollectionsPage(CollectionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}