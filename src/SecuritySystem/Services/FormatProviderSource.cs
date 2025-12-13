namespace SecuritySystem.Services;

public class FormatProviderSource(IFormatProvider formatProvider) : IFormatProviderSource
{
	public IFormatProvider FormatProvider { get; } = formatProvider;
}