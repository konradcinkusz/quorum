namespace MR.Client.Pages.Components;

public class InputSelectBoolean<T> : InputSelect<T>
{
    protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
    {
        if (typeof(T) == typeof(bool?))
        {
            if (string.IsNullOrEmpty(value) || value == "null")
            {
                result = default;
                validationErrorMessage = null;
                return true;
            }
            else if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                result = (T)(object)true;
                validationErrorMessage = null;
                return true;
            }
            else if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                result = (T)(object)false;
                validationErrorMessage = null;
                return true;
            }
            else
            {
                result = default;
                validationErrorMessage = "The chosen value is not a valid boolean.";
                return false;
            }
        }
        else
        {
            return base.TryParseValueFromString(value, out result, out validationErrorMessage);
        }
    }

    protected override string FormatValueAsString(T value)
    {
        if (typeof(T) == typeof(bool?))
        {
            if (value == null)
            {
                return default;
            }
        }

        return base.FormatValueAsString(value);
    }
}