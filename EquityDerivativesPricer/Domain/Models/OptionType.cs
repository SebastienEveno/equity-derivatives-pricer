using System.Runtime.Serialization;

namespace EquityDerivativesPricer.Domain.Models
{
	public enum OptionType
	{
		[EnumMember(Value = "Call")]
		Call,
		[EnumMember(Value = "Put")]
		Put
	}
}
