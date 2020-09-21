using SpecFlow.Assist.Dynamic.PropertyNameFormatting;

namespace SpecFlow.Assist.Dynamic.Specs.PropertyNameFormatting
{
    public class CustomPropertyNameFormatter : IPropertyNameFormatter
    {
        public string Format(string name) => name.Replace(" ", string.Empty);
    }
}
