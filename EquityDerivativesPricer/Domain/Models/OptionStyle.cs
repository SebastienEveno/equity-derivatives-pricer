using System.Runtime.Serialization;

namespace EquityDerivativesPricer.Domain.Models
{
	public enum OptionStyle
	{
		[EnumMember(Value = "American")]
		American,
		[EnumMember(Value = "European")]
		European
	}
}
