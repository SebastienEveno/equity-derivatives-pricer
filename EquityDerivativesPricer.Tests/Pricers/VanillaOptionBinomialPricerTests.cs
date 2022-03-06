using EquityDerivativesPricer.Domain.Models;
using EquityDerivativesPricer.Domain.Services.Calculators;
using EquityDerivativesPricer.Domain.Services.Pricers.VanillaOptions.Binomial;
using Moq;
using NUnit.Framework;

namespace EquityDerivativesPricer.Tests.VanillaOptions
{
	[TestFixture]
	public class VanillaOptionBinomialPricerTests
	{
		private IVanillaOptionBinomialPricer _pricer;

		private Mock<IInterestRateCalculator> _interestRateCalculator;

		[OneTimeSetUp]
		public void Init()
		{
			_interestRateCalculator = new Mock<IInterestRateCalculator>();

			_pricer = new VanillaOptionBinomialPricer(_interestRateCalculator.Object);
		}

		[Test]
		public void PriceAtTheMoneyAmericanCallOption_Ok()
		{
			_interestRateCalculator.Reset();

			// arrange
			var underlying = new Underlying
			{
				Name = "AAPL",
				SpotPrice = 100.0,
				AnnualDividendYield = 0.01,
				AnnualVolatility = 0.2,
			};

			var vanillaOption = new VanillaOption
			{
				OptionStyle = OptionStyle.American,
				OptionType = OptionType.Call,
				Strike = 100.0,
				Maturity = Maturity.Parse("1Y"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.BinomialTree,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(7.5106, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.5217, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0202, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(39.4014, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-3.5247, pricingResult.Theta, 0.0001);
			Assert.AreEqual(38.5076, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}

		[Test]
		public void PriceInTheMoneyAmericanCallOption_Ok()
		{
			_interestRateCalculator.Reset();

			// arrange
			var underlying = new Underlying
			{
				Name = "AAPL",
				SpotPrice = 110.0,
				AnnualDividendYield = 0.01,
				AnnualVolatility = 0.2,
			};

			var vanillaOption = new VanillaOption
			{
				OptionStyle = OptionStyle.American,
				OptionType = OptionType.Call,
				Strike = 100.0,
				Maturity = Maturity.Parse("1Y"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.BinomialTree,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(13.6906, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.7064, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0162, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(36.9291, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-3.1660, pricingResult.Theta, 0.0001);
			Assert.AreEqual(49.5059, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}

		[Test]
		public void PriceOutTheMoneyAmericanCallOption_Ok()
		{
			_interestRateCalculator.Reset();

			// arrange
			var underlying = new Underlying
			{
				Name = "AAPL",
				SpotPrice = 90.0,
				AnnualDividendYield = 0.01,
				AnnualVolatility = 0.2,
			};

			var vanillaOption = new VanillaOption
			{
				OptionStyle = OptionStyle.American,
				OptionType = OptionType.Call,
				Strike = 100.0,
				Maturity = Maturity.Parse("1Y"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.BinomialTree,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(3.3232, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.3166, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0198, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(32.4207, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-2.9380, pricingResult.Theta, 0.0001);
			Assert.AreEqual(23.3413, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}

		[Test]
		public void PriceHullAmericanPutOption_Ok()
		{
			// Options, Futures and Other Derivatives, Hull, p. 312
			_interestRateCalculator.Reset();

			// arrange
			var underlying = new Underlying
			{
				Name = "AAPL",
				SpotPrice = 50.0,
				AnnualDividendYield = 0,
				AnnualVolatility = 0.3,
			};

			var vanillaOption = new VanillaOption
			{
				OptionStyle = OptionStyle.American,
				OptionType = OptionType.Put,
				Strike = 52.0,
				Maturity = Maturity.Parse("2Y"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0.05);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.BinomialTree,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(7.4709, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(-0.4191, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0227, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(26.2688, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-1.1365, pricingResult.Theta, 0.0001);
			Assert.AreEqual(-33.5600, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}

		[Test]
		public void PriceHullEuropeanPutOption_Ok()
		{
			// Options, Futures and Other Derivatives, Hull, p. 312
			_interestRateCalculator.Reset();

			// arrange
			var underlying = new Underlying
			{
				Name = "AAPL",
				SpotPrice = 50.0,
				AnnualDividendYield = 0,
				AnnualVolatility = 0.3,
			};

			var vanillaOption = new VanillaOption
			{
				OptionStyle = OptionStyle.European,
				OptionType = OptionType.Put,
				Strike = 52.0,
				Maturity = Maturity.Parse("2Y"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0.05);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.BinomialTree,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(6.7568, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(-0.3612, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0176, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(26.3670, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-0.7482, pricingResult.Theta, 0.0001);
			Assert.AreEqual(-49.5705, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}

		[Test]
		public void PriceHullEuropeanCallOption_WithContinuousDividendYield_Ok()
		{
			// Options, Futures and Other Derivatives, Hull, p. 313
			_interestRateCalculator.Reset();

			// arrange
			var underlying = new Underlying
			{
				Name = "STOCK_INDEX",
				SpotPrice = 810.0,
				AnnualDividendYield = 0.02,
				AnnualVolatility = 0.2,
			};

			var vanillaOption = new VanillaOption
			{
				OptionStyle = OptionStyle.European,
				OptionType = OptionType.Call,
				Strike = 800.0,
				Maturity = Maturity.Parse("6M"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0.05);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.BinomialTree,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(55.8754, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.5981, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0033, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(217.7714, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-55.8134, pricingResult.Theta, 0.0001);
			Assert.AreEqual(211.4960, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}
	}
}