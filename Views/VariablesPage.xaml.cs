using PosterFly.ViewModels;

namespace PosterFly.Views;

public partial class VariablesPage : ContentPage
{
    public VariablesPage(VariablesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}