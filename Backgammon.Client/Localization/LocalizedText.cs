using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace Backgammon.Client.Localization
{
    public sealed class LocalizedText : MarkupExtension
    {
        public string ResourceKey { get; set; } = string.Empty;

        public string StringFormat { get; set; } = string.Empty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Binding resourceBinding = new Binding("[" + ResourceKey + "]")
            {
                Source = LocalizationService.Current,
                Mode = BindingMode.OneWay
            };

            if (!string.IsNullOrEmpty(StringFormat))
            {
                resourceBinding.StringFormat = StringFormat;
            }

            return resourceBinding.ProvideValue(serviceProvider);
        }
    }
}
