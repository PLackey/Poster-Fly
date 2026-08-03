using System.Collections.ObjectModel;
using PosterFly.Models;
using PosterFly.Services;

namespace PosterFly.Controls;

public partial class VariableAutoCompleteEntry : ContentView
{
    private readonly IVariableService? _variableService;
    private bool _isShowingSuggestions;
    private string _currentVariablePrefix = string.Empty;

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(VariableAutoCompleteEntry), string.Empty,
        BindingMode.TwoWay, propertyChanged: OnTextPropertyChanged);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(VariableAutoCompleteEntry), string.Empty);

    public static readonly BindableProperty SuggestionsProperty = BindableProperty.Create(
        nameof(Suggestions), typeof(ObservableCollection<Variable>), typeof(VariableAutoCompleteEntry),
        new ObservableCollection<Variable>());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public ObservableCollection<Variable> Suggestions
    {
        get => (ObservableCollection<Variable>)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    public VariableAutoCompleteEntry()
    {
        InitializeComponent();
        Suggestions = new ObservableCollection<Variable>();
        
        // Try to get variable service from service provider
        if (Application.Current?.Handler?.MauiContext?.Services != null)
        {
            _variableService = Application.Current.Handler.MauiContext.Services.GetService<IVariableService>();
        }
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VariableAutoCompleteEntry control && control.MainEntry != null)
        {
            control.MainEntry.Text = newValue?.ToString() ?? string.Empty;
        }
    }

    private async void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Text = e.NewTextValue ?? string.Empty;
        await HandleVariableAutoComplete(e.NewTextValue ?? string.Empty);
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        HintLabel.IsVisible = true;
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        // Delay hiding to allow for suggestion selection
        Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
        {
            HideSuggestions();
            HintLabel.IsVisible = false;
            return false;
        });
    }

    private void OnSuggestionTapped(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Variable variable)
        {
            InsertVariable(variable.Name);
        }
    }

    private async Task HandleVariableAutoComplete(string text)
    {
        if (string.IsNullOrEmpty(text) || _variableService == null)
        {
            HideSuggestions();
            return;
        }

        // Check if user is typing a variable ({{variableName}})
        var cursorPosition = MainEntry.CursorPosition;
        var variableMatch = FindVariableAtPosition(text, cursorPosition);

        if (variableMatch.HasValue)
        {
            var (startIndex, prefix) = variableMatch.Value;
            _currentVariablePrefix = prefix;

            if (prefix.Length > 0)
            {
                try
                {
                    var suggestions = await _variableService.GetAutoCompleteVariablesAsync(prefix);
                    var variables = await _variableService.GetAllVariablesAsync();
                    
                    var filteredVars = variables
                        .Where(v => suggestions.Contains(v.Name))
                        .OrderByDescending(v => v.UsageCount)
                        .Take(5)
                        .ToList();

                    Suggestions.Clear();
                    foreach (var variable in filteredVars)
                    {
                        Suggestions.Add(variable);
                    }

                    ShowSuggestions();
                }
                catch (Exception ex)
                {
                    // Log error but don't show to user
                    System.Diagnostics.Debug.WriteLine($"Variable autocomplete error: {ex.Message}");
                    HideSuggestions();
                }
            }
            else
            {
                // Show most used variables when user types {{
                try
                {
                    var mostUsed = await _variableService.GetVariablesByUsageAsync(5);
                    Suggestions.Clear();
                    foreach (var variable in mostUsed)
                    {
                        Suggestions.Add(variable);
                    }
                    ShowSuggestions();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Variable autocomplete error: {ex.Message}");
                    HideSuggestions();
                }
            }
        }
        else
        {
            HideSuggestions();
        }
    }

    private (int startIndex, string prefix)? FindVariableAtPosition(string text, int cursorPosition)
    {
        if (cursorPosition < 2) return null;

        // Look backwards from cursor to find {{
        var searchStart = Math.Max(0, cursorPosition - 50); // Limit search range
        var textToSearch = text.Substring(searchStart, cursorPosition - searchStart);
        
        var lastOpenBrace = textToSearch.LastIndexOf("{{");
        if (lastOpenBrace == -1) return null;

        var actualStartIndex = searchStart + lastOpenBrace + 2; // +2 to skip {{
        
        // Check if there's a closing brace before cursor
        var remainingText = text.Substring(actualStartIndex, cursorPosition - actualStartIndex);
        if (remainingText.Contains("}}")) return null;

        return (actualStartIndex, remainingText);
    }

    private void InsertVariable(string variableName)
    {
        var currentText = MainEntry.Text ?? string.Empty;
        var cursorPosition = MainEntry.CursorPosition;
        
        // Find the variable being typed
        var variableMatch = FindVariableAtPosition(currentText, cursorPosition);
        
        if (variableMatch.HasValue)
        {
            var (startIndex, _) = variableMatch.Value;
            var beforeVariable = currentText.Substring(0, startIndex - 2); // -2 to include {{
            var afterCursor = currentText.Substring(cursorPosition);
            
            var newText = $"{beforeVariable}{{{{{variableName}}}}}{afterCursor}";
            var newCursorPosition = beforeVariable.Length + variableName.Length + 4; // +4 for {{}}
            
            MainEntry.Text = newText;
            MainEntry.CursorPosition = newCursorPosition;
            Text = newText;
        }
        
        HideSuggestions();
    }

    private void ShowSuggestions()
    {
        if (!_isShowingSuggestions && Suggestions.Any())
        {
            AutoCompleteFrame.IsVisible = true;
            _isShowingSuggestions = true;
        }
    }

    private void HideSuggestions()
    {
        AutoCompleteFrame.IsVisible = false;
        _isShowingSuggestions = false;
        Suggestions.Clear();
    }
}