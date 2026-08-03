using System.Globalization;
using Microsoft.Maui.Graphics;
using PosterFly.Models;

namespace PosterFly.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Colors.Green : Colors.Red;
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value?.ToString());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class HttpMethodToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is HttpMethod method)
        {
            return method switch
            {
                HttpMethod.GET => Colors.Green,
                HttpMethod.POST => Colors.Orange,
                HttpMethod.PUT => Colors.Blue,
                HttpMethod.DELETE => Colors.Red,
                HttpMethod.PATCH => Colors.Purple,
                HttpMethod.HEAD => Colors.Gray,
                HttpMethod.OPTIONS => Colors.Brown,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RequestTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ScopeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SecretValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}