using System.Globalization;
using Microsoft.Maui.Graphics;
using PosterFly.Models;

namespace PosterFly.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Colors.Green : Colors.Red;
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class HttpMethodToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PosterFly.Models.HttpMethod method)
        {
            return method switch
            {
                PosterFly.Models.HttpMethod.GET => Colors.Green,
                PosterFly.Models.HttpMethod.POST => Colors.Orange,
                PosterFly.Models.HttpMethod.PUT => Colors.Blue,
                PosterFly.Models.HttpMethod.DELETE => Colors.Red,
                PosterFly.Models.HttpMethod.PATCH => Colors.Purple,
                PosterFly.Models.HttpMethod.HEAD => Colors.Gray,
                PosterFly.Models.HttpMethod.OPTIONS => Colors.Brown,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RequestTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RequestType requestType)
        {
            return requestType switch
            {
                RequestType.HTTP => Colors.Blue,
                RequestType.GraphQL => Colors.Pink,
                RequestType.GRPC => Colors.Green,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ScopeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is VariableScope scope)
        {
            return scope switch
            {
                VariableScope.Global => Colors.Blue,
                VariableScope.Collection => Colors.Green,
                VariableScope.Environment => Colors.Orange,
                VariableScope.Request => Colors.Purple,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SecretValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Variable variable)
        {
            return variable.IsSecret ? "***HIDDEN***" : variable.Value;
        }
        
        if (value is string stringValue && parameter?.ToString() == "secret")
        {
            return "***HIDDEN***";
        }
        
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}