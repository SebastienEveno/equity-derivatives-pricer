using EquityDerivativesPricer.Domain.Models;
using EquityDerivativesPricer.Domain.Services.Calculators;
using MathNet.Numerics.Distributions;

namespace EquityDerivativesPricer.Domain.Services.Pricers.VanillaOptions.Analytic
{
	public class VanillaOptionAnalyticPricer : IVanillaOptionAnalyticPricer
	{
		private readonly IInterestRateCalculator _interestRateCalculator;

		public VanillaOptionAnalyticPricer(IInterestRateCalculator interestRateCalculator)
		{
			_interestRateCalculator = interestRateCalculator;
		}

		public PricingResult Price(PricingConfiguration config, VanillaOption option)
		{
			return option.OptionStyle == OptionStyle.European
				? PriceEuropeanOption(config, option)
				: throw new InvalidOperationException(
					$"The \"{NumericalMethod.Analytic}\" numerical method is not available for American vanilla options.");
		}

		private PricingResult PriceEuropeanOption(PricingConfiguration config, VanillaOption option)
		{
			var pricingResult = new PricingResult { };

			var riskFreeInterestRate = _interestRateCalculator.GetAnnualRiskFreeRate();

			var sigma = option.Underlying.AnnualVolatility;
			var spotPrice = option.Underlying.SpotPrice;
			var q = option.Underlying.AnnualDividendYield;

			// We assume for now that we price the option at t = 0, so that timeToMaturity = maturity of the option.
			var timeToMaturity = option.Maturity.ToYearFraction();
			var strike = option.Strike;

			var multiplier = option.OptionType switch
			{
				OptionType.Call => 1,
				OptionType.Put => -1,
				_ => throw new InvalidOperationException($"Error in option type \"{option.OptionType}\".")
			};

			var d_1 = (Math.Log(spotPrice / strike)
				+ (riskFreeInterestRate - q + sigma * sigma / 2) * timeToMaturity)
				/ (sigma * Math.Sqrt(timeToMaturity));
			var d_2 = d_1 - sigma * Math.Sqrt(timeToMaturity);

			pricingResult.PresentValue = multiplier
				* (spotPrice * Math.Exp(-q * timeToMaturity) * Normal.CDF(0, 1, multiplier * d_1)
					- strike * Normal.CDF(0, 1, multiplier * d_2) * Math.Exp(-riskFreeInterestRate * timeToMaturity)
				);

			if (config.IsCalculationWithGreeks)
			{
				pricingResult.Delta = Delta(multiplier, q, timeToMaturity, d_1);
				pricingResult.Gamma = Gamma(spotPrice, q, timeToMaturity, d_1, sigma);
				pricingResult.Vega = Vega(spotPrice, q, timeToMaturity, d_1);
				pricingResult.Theta = Theta(multiplier, q, timeToMaturity, spotPrice, d_1, sigma, riskFreeInterestRate, strike, d_2);
				pricingResult.Rho = Rho(multiplier, timeToMaturity, riskFreeInterestRate, strike, d_2);
			}

			return pricingResult;
		}

		private double Delta(double multiplier, double q, double tau, double d_1)
		{
			return multiplier * Math.Exp(-q * tau) * Normal.CDF(0, 1, multiplier * d_1);
		}

		private double Gamma(double S, double q, double tau, double d_1, double sigma)
		{
			return Math.Exp(-q * tau) * Normal.PDF(0, 1, d_1) / (S * sigma * Math.Sqrt(tau));
		}

		private double Vega(double S, double q, double tau, double d_1)
		{
			return S * Math.Exp(-q * tau) * Normal.PDF(0, 1, d_1) * Math.Sqrt(tau);
		}

		private double Theta(int multiplier, double q, double tau, double S,
			double d_1, double sigma, double r, double K, double d_2)
		{
			return -Math.Exp(-q * tau) * S * Normal.PDF(0, 1, d_1) * sigma / (2 * Math.Sqrt(tau))
				- multiplier * r * K * Math.Exp(-r * tau) * Normal.CDF(0, 1, multiplier * d_2)
				+ multiplier * q * S * Math.Exp(-q * tau) * Normal.CDF(0, 1, multiplier * d_1);
		}

		private double Rho(int multiplier, double tau, double r, double K, double d_2)
		{
			return multiplier * K * tau * Math.Exp(-r * tau) * Normal.CDF(0, 1, multiplier * d_2);
		}
	}
}
