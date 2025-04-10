using FluentValidation;

namespace MinimalAPIsMovies.Validations;

public static class ValidationUtilities
{
    public static string NonEmptyMessage = "The {PropertyName} field must not be empty.";
    public static string MaximumLengthMessage = "The field '{PropertyName}' must be less than {MaxLength} characters.";
    public static string FirstLetterIsUpperMessage = "The first letter of the field '{PropertyName}' must be uppercase.";
    public static string GreaterThanDateMessage(DateTime minimunDate) => "The field '{PropertyName}' must be greater than " + minimunDate.ToShortDateString();


    public static bool FirstLetterIsUpper(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true;

        var firstLetter = name[0];
        return firstLetter == char.ToUpper(firstLetter);
    }
}
