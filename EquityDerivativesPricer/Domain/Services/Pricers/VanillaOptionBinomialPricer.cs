﻿using EquityDerivativesPricer.Domain.Models;
using EquityDerivativesPricer.Domain.Services.Calculators;

namespace EquityDerivativesPricer.Domain.Services.Pricers
{
	public class VanillaOptionBinomialPricer : IVanillaOptionBinomialPricer
	{
		private readonly int _numberOfTimeSteps = 500;

		private readonly IInterestRateCalculator _interestRateCalculator;

		public VanillaOptionBinomialPricer(IInterestRateCalculator interestRateCalculator)
		{
			_interestRateCalculator = interestRateCalculator;
		}

		public PricingResult Price(PricingConfiguration config, VanillaOption option)
		{
			return option.OptionStyle == OptionStyle.EUROPEAN
				? PriceEuropeanOption(config, option)
				: PriceAmericanOption(config, option);
		}

		public PricingResult PriceAmericanOption(PricingConfiguration config, VanillaOption option)
		{
			var pricingResult = new PricingResult { };

			var multiplier = option.OptionType switch
			{
				OptionType.CALL => 1,
				OptionType.PUT => -1,
				_ => throw new NotImplementedException()
			};

			var riskFreeInterestRate = _interestRateCalculator.GetAnnualRiskFreeRate();
			var annualVolatility = option.Underlying.AnnualVolatility;
			var annualDividendYield = option.Underlying.AnnualDividendYield;
			var spotPrice = option.Underlying.SpotPrice;

			var strike = option.Strike;
			var timeToMaturity = option.Maturity.ToYearFraction();

			var deltaT = timeToMaturity / _numberOfTimeSteps;
			var u = Math.Exp(annualVolatility * Math.Sqrt(deltaT));
			var d = 1 / u;

			// probability of up move
			var q = (Math.Exp((riskFreeInterestRate - annualDividendYield) * deltaT) - d) / (u - d);

			// Stock prices
			var s = new double[_numberOfTimeSteps + 1];

			// Option prices
			var p = new double[_numberOfTimeSteps + 1];

			// Initialize prices at maturity (i.e. payoffs)
			for (var j = 0; j < _numberOfTimeSteps + 1; j++)
			{
				s[j] = spotPrice * Math.Pow(u, j) * Math.Pow(d, _numberOfTimeSteps - j);
				p[j] = Math.Max(multiplier * (s[j] - strike), 0);
			}

			// Backward recursion through the tree
			for (var i = _numberOfTimeSteps - 1; i >= 0; i--)
			{
				for (var j = 0; j < i + 1; j++)
				{
					p[j] = (q * p[j + 1] + (1 - q) * p[j]) * Math.Exp(-riskFreeInterestRate * deltaT);
					p[j] = Math.Max(multiplier * (spotPrice * Math.Pow(u, j) * Math.Pow(d, i - j) - strike), p[j]);
				}
			}

			pricingResult.PresentValue = p[0];

			// TODO: Implement greeks calculation for american options

			return pricingResult;
		}

		public PricingResult PriceEuropeanOption(PricingConfiguration config, VanillaOption option)
		{
			var pricingResult = new PricingResult { };

			var multiplier = option.OptionType switch
			{
				OptionType.CALL => 1,
				OptionType.PUT => -1,
				_ => throw new NotImplementedException()
			};

			var riskFreeInterestRate = _interestRateCalculator.GetAnnualRiskFreeRate();
			var annualVolatility = option.Underlying.AnnualVolatility;
			var annualDividendYield = option.Underlying.AnnualDividendYield;
			var spotPrice = option.Underlying.SpotPrice;

			var strike = option.Strike;
			var timeToMaturity = option.Maturity.ToYearFraction();

			var deltaT = timeToMaturity / _numberOfTimeSteps;
			var u = Math.Exp(annualVolatility * Math.Sqrt(deltaT));
			var d = 1 / u;

			// probability of up move
			var q = (Math.Exp((riskFreeInterestRate - annualDividendYield) * deltaT) - d) / (u - d);

			// Stock prices
			var s = new double[_numberOfTimeSteps + 1];

			// Option prices
			var p = new double[_numberOfTimeSteps + 1];

			// Initialize prices at maturity (i.e. payoffs)
			for (var j = 0; j < _numberOfTimeSteps + 1; j++)
			{
				s[j] = spotPrice * Math.Pow(u, j) * Math.Pow(d, _numberOfTimeSteps - j);
				p[j] = Math.Max(multiplier * (s[j] - strike), 0);
			}

			// Backward recursion through the tree
			for (var i = _numberOfTimeSteps - 1; i >= 0; i--)
			{
				for (var j = 0; j < i + 1; j++)
				{
					p[j] = (q * p[j + 1] + (1 - q) * p[j]) * Math.Exp(-riskFreeInterestRate * deltaT);
				}
			}

			pricingResult.PresentValue = p[0];

			// TODO: Implement greeks calculation for american options

			return pricingResult;
		}
	}
}