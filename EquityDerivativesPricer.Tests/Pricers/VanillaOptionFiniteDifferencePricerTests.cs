using EquityDerivativesPricer.Domain.Models;
using EquityDerivativesPricer.Domain.Services.Calculators;
using EquityDerivativesPricer.Domain.Services.Pricers.VanillaOptions.FiniteDifferences;
using Moq;
using NUnit.Framework;

namespace EquityDerivativesPricer.Tests.VanillaOptions
{
	[TestFixture]
	public class VanillaOptionFiniteDifferencePricerTests
	{
		private IVanillaOptionFiniteDifferencePricer _pricer;

		private Mock<IInterestRateCalculator> _interestRateCalculator;

		[OneTimeSetUp]
		public void Init()
		{
			_interestRateCalculator = new Mock<IInterestRateCalculator>();

			_pricer = new VanillaOptionFiniteDifferencePricer(_interestRateCalculator.Object);
		}

		[Test]
		public void PriceAtTheMoneyEuropeanCallOption_Ok()
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
				OptionStyle = OptionStyle.European,
				OptionType = OptionType.Call,
				Strike = 100.0,
				Maturity = Maturity.Parse("1Y"),
				Underlying = underlying
			};

			_interestRateCalculator.Setup(x => x.GetAnnualRiskFreeRate()).Returns(0);

			var pricingConfig = new PricingConfiguration
			{
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(7.4278, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.5150, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0197, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(39.3920, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-3.4398, pricingResult.Theta, 0.0001);
			Assert.AreEqual(44.8355, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
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
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(7.5033, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.5221, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0202, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(39.3652, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-3.5285, pricingResult.Theta, 0.0001);
			Assert.AreEqual(38.6016, pricingResult.Rho, 0.0001);

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
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(13.6823, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.7065, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0162, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(37.8540, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-3.1690, pricingResult.Theta, 0.0001);
			Assert.AreEqual(49.6477, pricingResult.Rho, 0.0001);

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
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(3.3245, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.3175, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0198, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(31.4720, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-2.9369, pricingResult.Theta, 0.0001);
			Assert.AreEqual(23.4696, pricingResult.Rho, 0.0001);

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
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(7.4683, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(-0.4187, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0226, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(26.3673, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-1.1349, pricingResult.Theta, 0.0001);
			Assert.AreEqual(-33.5165, pricingResult.Rho, 0.0001);

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
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(6.7613, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(-0.3608, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0176, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(26.5602, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-0.7476, pricingResult.Theta, 0.0001);
			Assert.AreEqual(-49.5662, pricingResult.Rho, 0.0001);

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
				NumericalMethod = NumericalMethod.FiniteDifferences,
				IsCalculationWithGreeks = true
			};

			// act
			var pricingResult = _pricer.Price(pricingConfig, vanillaOption);

			// assert
			Assert.AreEqual(55.9065, pricingResult.PresentValue, 0.0001);
			Assert.AreEqual(0.5982, pricingResult.Delta, 0.0001);
			Assert.AreEqual(0.0033, pricingResult.Gamma, 0.0001);
			Assert.AreEqual(217.6153, pricingResult.Vega, 0.0001);
			Assert.AreEqual(-55.7927, pricingResult.Theta, 0.0001);
			Assert.AreEqual(211.4529, pricingResult.Rho, 0.0001);

			_interestRateCalculator.Verify(x => x.GetAnnualRiskFreeRate(), Times.Once);
		}
	}
}