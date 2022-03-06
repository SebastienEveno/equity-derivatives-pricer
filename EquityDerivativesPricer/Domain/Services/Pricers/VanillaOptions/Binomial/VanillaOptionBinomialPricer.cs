using EquityDerivativesPricer.Domain.Models;
using EquityDerivativesPricer.Domain.Services.Calculators;

namespace EquityDerivativesPricer.Domain.Services.Pricers.VanillaOptions.Binomial
{
	public class VanillaOptionBinomialPricer : IVanillaOptionBinomialPricer
	{
		private readonly int _numberOfTimeSteps = 500;
		private readonly double _bumpVolatility = 0.01;
		private readonly double _bumpRiskFreeRate = 0.01;

		private readonly IInterestRateCalculator _interestRateCalculator;

		public VanillaOptionBinomialPricer(IInterestRateCalculator interestRateCalculator)
		{
			_interestRateCalculator = interestRateCalculator;
		}

		public PricingResult Price(PricingConfiguration config, VanillaOption option)
		{
			var riskFreeInterestRate = _interestRateCalculator.GetAnnualRiskFreeRate();
			var binomialTreeResult = BinomialTree(option, riskFreeInterestRate);
			var pricingResult = ExtractPricingResult(config, option, riskFreeInterestRate, binomialTreeResult);

			return pricingResult;
		}

		private BinomialTreeResult BinomialTree(VanillaOption option, double riskFreeInterestRate)
		{
			var multiplier = option.OptionType == OptionType.Call
				? 1
				: -1;

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
			var prices = new double[_numberOfTimeSteps + 1, _numberOfTimeSteps + 1];

			// Initialize prices at maturity (i.e. payoffs)
			for (var j = 0; j < _numberOfTimeSteps + 1; j++)
			{
				s[j] = spotPrice * Math.Pow(u, j) * Math.Pow(d, _numberOfTimeSteps - j);

				prices[_numberOfTimeSteps, j] = Math.Max(multiplier * (s[j] - strike), 0);
			}

			// Backward recursion through the tree
			for (var i = _numberOfTimeSteps - 1; i >= 0; i--)
			{
				for (var j = 0; j < i + 1; j++)
				{
					prices[i, j] = (q * prices[i + 1, j + 1] + (1 - q) * prices[i + 1, j]) * Math.Exp(-riskFreeInterestRate * deltaT);

					if (option.OptionStyle == OptionStyle.American)
					{
						prices[i, j] = Math.Max(multiplier * (spotPrice * Math.Pow(u, j) * Math.Pow(d, i - j) - strike), prices[i, j]);
					}
				}
			}

			return new BinomialTreeResult
			{
				OptionPrices = prices,
				U = u,
				D = d,
				DeltaT = deltaT,
				NumberOfTimeSteps = _numberOfTimeSteps
			};
		}

		private PricingResult ExtractPricingResult(
			PricingConfiguration config,
			VanillaOption option,
			double riskFreeInterestRate,
			BinomialTreeResult binomialTreeResult)
		{
			var pricingResult = new PricingResult { };

			pricingResult.PresentValue = binomialTreeResult.OptionPrices[0, 0];

			if (config.IsCalculationWithGreeks)
			{
				pricingResult.Delta = (binomialTreeResult.OptionPrices[1, 1] - binomialTreeResult.OptionPrices[1, 0])
					/ (option.Underlying.SpotPrice * binomialTreeResult.U - option.Underlying.SpotPrice * binomialTreeResult.D);

				pricingResult.Gamma = ((binomialTreeResult.OptionPrices[2, 2] - binomialTreeResult.OptionPrices[2, 1])
					/ (option.Underlying.SpotPrice * binomialTreeResult.U * binomialTreeResult.U - option.Underlying.SpotPrice)
							- (binomialTreeResult.OptionPrices[2, 1] - binomialTreeResult.OptionPrices[2, 0])
							/ (option.Underlying.SpotPrice - option.Underlying.SpotPrice * binomialTreeResult.D * binomialTreeResult.D))
							/ (0.5 * option.Underlying.SpotPrice * (binomialTreeResult.U * binomialTreeResult.U - binomialTreeResult.D * binomialTreeResult.D));

				pricingResult.Theta = (binomialTreeResult.OptionPrices[2, 1] - binomialTreeResult.OptionPrices[0, 0]) / (2 * binomialTreeResult.DeltaT);

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
				var bumpedVolBinomialTreeResult = BinomialTree(optionBumpedVol, riskFreeInterestRate);
				pricingResult.Vega = (bumpedVolBinomialTreeResult.OptionPrices[0, 0] - binomialTreeResult.OptionPrices[0, 0]) / dSigma;

				// Rho
				var dRho = Math.Abs(riskFreeInterestRate) < 0.0001
					? _bumpRiskFreeRate
					: riskFreeInterestRate * _bumpRiskFreeRate;

				var bumpedInterestRate = riskFreeInterestRate + dRho;
				var bumpedRiskFreeInterestRateBinomialTreeResult = BinomialTree(option, bumpedInterestRate);
				pricingResult.Rho = (bumpedRiskFreeInterestRateBinomialTreeResult.OptionPrices[0, 0] - binomialTreeResult.OptionPrices[0, 0])
					/ dRho;
			}

			return pricingResult;
		}
	}
}
