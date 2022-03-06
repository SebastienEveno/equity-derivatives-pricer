using EquityDerivativesPricer.Domain.Models;
using EquityDerivativesPricer.Domain.Services.Calculators;

namespace EquityDerivativesPricer.Domain.Services.Pricers.VanillaOptions.FiniteDifferences
{
	public class VanillaOptionFiniteDifferencePricer : IVanillaOptionFiniteDifferencePricer
	{
		private readonly int _numberOfTimeSteps = 200;
		private readonly int _numberOfSpaceSteps = 400;

		private readonly double _bumpVolatility = 0.01;
		private readonly double _bumpRiskFreeRate = 0.01;

		private readonly IInterestRateCalculator _interestRateCalculator;

		public VanillaOptionFiniteDifferencePricer(IInterestRateCalculator interestRateCalculator)
		{
			_interestRateCalculator = interestRateCalculator;
		}

		public PricingResult Price(PricingConfiguration config, VanillaOption option)
		{
			var riskFreeInterestRate = _interestRateCalculator.GetAnnualRiskFreeRate();
			var finiteDifferenceResult = FiniteDifference(option, riskFreeInterestRate);
			var pricingResult = ExtractPricingResult(config, option, riskFreeInterestRate, finiteDifferenceResult);

			return pricingResult;
		}

		private FiniteDifferenceResult FiniteDifference(VanillaOption option, double riskFreeInterestRate)
		{
			var multiplier = option.OptionType == OptionType.Call
				? 1
				: -1;

			var sigma = option.Underlying.AnnualVolatility;
			var annualDividendYield = option.Underlying.AnnualDividendYield;
			var spotPrice = option.Underlying.SpotPrice;

			var strike = option.Strike;
			var timeToMaturity = option.Maturity.ToYearFraction();

			var deltaT = timeToMaturity / _numberOfTimeSteps;
			var deltaZ = sigma * Math.Sqrt(3 * deltaT);

			// Option prices
			var prices = new double[_numberOfTimeSteps + 1, _numberOfSpaceSteps + 1];

			// Initialize prices at maturity (i.e. payoffs)
			for (var j = 0; j < _numberOfSpaceSteps + 1; j++)
			{
				prices[_numberOfTimeSteps, j] = Math.Max(multiplier * (Math.Exp(Math.Log(spotPrice) + (j - _numberOfSpaceSteps / 2) * deltaZ) - strike), 0);
			}

			// Set value of the option at the boundaries of the underlying
			for (var i = 0; i < _numberOfTimeSteps + 1; i++)
			{
				if (option.OptionStyle == OptionStyle.European)
				{
					prices[i, 0] = option.OptionType == OptionType.Call
						? 0
						: strike * Math.Exp(-riskFreeInterestRate * i * deltaT);
					prices[i, _numberOfSpaceSteps] = option.OptionType == OptionType.Call
						? spotPrice
						: 0;
				}
				else
				{
					prices[i, 0] = option.OptionType == OptionType.Call
						? 0
						: strike;
					prices[i, _numberOfSpaceSteps] = option.OptionType == OptionType.Call
						? spotPrice
						: 0;
				}
			}

			var alpha = 1 / (1 + riskFreeInterestRate * deltaT) * (-deltaT / (2 * deltaZ) * (riskFreeInterestRate - annualDividendYield - 0.5 * sigma * sigma)
						+ deltaT * sigma * sigma / (2 * deltaZ * deltaZ));
			var beta = 1 / (1 + riskFreeInterestRate * deltaT) * (1 - deltaT * sigma * sigma / (deltaZ * deltaZ));
			var gamma = 1 / (1 + riskFreeInterestRate * deltaT) * (deltaT / (2 * deltaZ) * (riskFreeInterestRate - annualDividendYield - 0.5 * sigma * sigma)
				+ deltaT * sigma * sigma / (2 * deltaZ * deltaZ));

			for (var i = _numberOfTimeSteps - 1; i >= 0; i--)
			{
				for (var j = 1; j < _numberOfSpaceSteps; j++)
				{
					// Explicit method
					prices[i, j] = alpha * prices[i + 1, j - 1] + beta * prices[i + 1, j] + gamma * prices[i + 1, j + 1];

					if (option.OptionStyle == OptionStyle.American)
					{
						prices[i, j] = Math.Max(multiplier * (Math.Exp(Math.Log(spotPrice) + (j - _numberOfSpaceSteps / 2) * deltaZ) - strike), prices[i, j]);
					}
				}
			}

			return new FiniteDifferenceResult
			{
				OptionPrices = prices,
				NumberOfTimeSteps = _numberOfTimeSteps,
				NumberOfSpaceSteps = _numberOfSpaceSteps
			};
		}

		private PricingResult ExtractPricingResult(
			PricingConfiguration config,
			VanillaOption option,
			double riskFreeInterestRate,
			FiniteDifferenceResult finiteDifferenceResult)
		{
			var pricingResult = new PricingResult { };
			var j = _numberOfSpaceSteps / 2;
			var timeToMaturity = option.Maturity.ToYearFraction();
			var deltaT = timeToMaturity / _numberOfTimeSteps;
			var deltaZ = option.Underlying.AnnualVolatility * Math.Sqrt(3 * deltaT);

			pricingResult.PresentValue = finiteDifferenceResult.OptionPrices[0, j];

			if (config.IsCalculationWithGreeks)
			{
				pricingResult.Delta = (finiteDifferenceResult.OptionPrices[0, j + 1] - finiteDifferenceResult.OptionPrices[0, j - 1])
					/ (2 * deltaZ) / option.Underlying.SpotPrice;
				pricingResult.Gamma = ((finiteDifferenceResult.OptionPrices[0, j + 1] - 2 * finiteDifferenceResult.OptionPrices[0, j] + finiteDifferenceResult.OptionPrices[0, j - 1])
					/ (deltaZ * deltaZ) - (finiteDifferenceResult.OptionPrices[0, j + 1] - finiteDifferenceResult.OptionPrices[0, j - 1])
					/ (2 * deltaZ)) / (option.Underlying.SpotPrice * option.Underlying.SpotPrice);
				pricingResult.Theta = (finiteDifferenceResult.OptionPrices[1, j] - finiteDifferenceResult.OptionPrices[0, j]) / deltaT;

				// Vega
				var dSigma = option.Underlying.AnnualVolatility * _bumpVolatility;
				var bumpedVolatility = option.Underlying.AnnualVolatility + dSigma;
				var optionBumpedVol = new VanillaOption
				{
					Underlying = new Underlying
					{
						SpotPrice = option.Underlying.SpotPrice,
						AnnualDividendYield = option.Underlying.AnnualDividendYield,
						AnnualVolatility = bumpedVolatility
					},
					OptionStyle = option.OptionStyle,
					OptionType = option.OptionType,
					Strike = option.Strike,
					Maturity = option.Maturity
				};
				var bumpedVolFiniteDifferenceResult = FiniteDifference(optionBumpedVol, riskFreeInterestRate);
				pricingResult.Vega = (bumpedVolFiniteDifferenceResult.OptionPrices[0, j] - finiteDifferenceResult.OptionPrices[0, j]) / dSigma;

				// Rho
				var dRho = Math.Abs(riskFreeInterestRate) < 0.0001
					? _bumpRiskFreeRate
					: riskFreeInterestRate * _bumpRiskFreeRate;

				var bumpedInterestRate = riskFreeInterestRate + dRho;
				var bumpedRiskFreeInterestRateFiniteDifferenceResult = FiniteDifference(option, bumpedInterestRate);
				pricingResult.Rho = (bumpedRiskFreeInterestRateFiniteDifferenceResult.OptionPrices[0, j] - finiteDifferenceResult.OptionPrices[0, j])
					/ dRho;
			}

			return pricingResult;
		}
	}
}
