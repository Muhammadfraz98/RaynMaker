﻿using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using RaynMaker.Blade.AnalysisSpec.Providers;
using RaynMaker.Blade.DataSheetSpec;
using RaynMaker.Blade.Engine;
using RaynMaker.Blade.Entities;

namespace RaynMaker.Blade.Tests.AnalysisSpec.Providers
{
    [TestFixture]
    public class GenericCurrentRatioProviderTests
    {
        private const string LhsSeriesName = "S1";
        private const string RhsSeriesName = "S2";

        private static readonly Currency Euro = new Currency { Name = "Euro" };
        private static readonly Currency Dollar = new Currency { Name = "Dollar" };

        private Mock<IFigureProviderContext> myContext;
        private GenericCurrentRatioProvider myProvider;
        private IDatumSeries myLhsSeries;
        private IDatumSeries myRhsSeries;

        [SetUp]
        public void SetUp()
        {
            myContext = new Mock<IFigureProviderContext> { DefaultValue = DefaultValue.Mock };
            myContext.Setup( x => x.GetSeries( LhsSeriesName ) ).Returns( () => myLhsSeries );
            myContext.Setup( x => x.GetSeries( RhsSeriesName ) ).Returns( () => myRhsSeries );

            myProvider = new GenericCurrentRatioProvider( "dummy", LhsSeriesName, RhsSeriesName, ( lhs, rhs ) => lhs + rhs );
        }

        [TearDown]
        public void TearDown()
        {
            myContext = null;
            myProvider = null;
            myLhsSeries = null;
            myRhsSeries = null;
        }

        [Test]
        public void ProvideValue_LhsSeriesEmpty_ReturnsMissingData()
        {
            myLhsSeries = Series.Empty;
            myRhsSeries = new Series( CreateDatum( 2015, 1 ) );
            myRhsSeries.Freeze();

            var result = myProvider.ProvideValue( myContext.Object );

            Assert.That( result, Is.InstanceOf<MissingData>() );
            Assert.That( ( ( MissingData )result ).Datum, Is.EqualTo( LhsSeriesName ) );
        }

        [Test]
        public void ProvideValue_RhsSeriesEmpty_ReturnsMissingData()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 1 ) );
            myLhsSeries.Freeze();
            myRhsSeries = Series.Empty;

            var result = myProvider.ProvideValue( myContext.Object );

            Assert.That( result, Is.InstanceOf<MissingData>() );
            Assert.That( ( ( MissingData )result ).Datum, Is.EqualTo( RhsSeriesName ) );
        }

        [Test]
        public void ProvideValue_LhsNotFrozen_Throws()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 1 ) );
            myRhsSeries = Series.Empty;

            var ex = Assert.Throws<ArgumentException>( () => myProvider.ProvideValue( myContext.Object ) );
            Assert.That( ex.Message, Is.StringContaining( "not frozen" ).And.StringContaining( LhsSeriesName ) );
        }

        [Test]
        public void ProvideValue_RhsNotFrozen_Throws()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 1 ) );
            myLhsSeries.Freeze();
            myRhsSeries = new Series( CreateDatum( 2015, 1 ) );

            var ex = Assert.Throws<ArgumentException>( () => myProvider.ProvideValue( myContext.Object ) );
            Assert.That( ex.Message, Is.StringContaining( "not frozen" ).And.StringContaining( RhsSeriesName ) );
        }

        [Test]
        public void ProvideValue_RhsHasNoDataForPeriod_MissingDataForPeriodReturned()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 1 ) );
            myLhsSeries.Freeze();
            myRhsSeries = new Series( CreateDatum( 2014, 1 ) );
            myRhsSeries.Freeze();

            var result = myProvider.ProvideValue( myContext.Object );

            Assert.That( result, Is.InstanceOf<MissingDataForPeriod>() );
            Assert.That( ( ( MissingDataForPeriod )result ).Datum, Is.EqualTo( RhsSeriesName ) );
        }

        [Test]
        public void ProvideValue_WithValidInputData_RatioReturned()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 5 ), CreateDatum( 2014, 7 ) );
            myLhsSeries.Freeze();
            myRhsSeries = new Series( CreateDatum( 2015, 23 ), CreateDatum( 2014, 37 ) );
            myRhsSeries.Freeze();

            var result = ( IDatum )myProvider.ProvideValue( myContext.Object );

            Assert.That( result.Period, Is.EqualTo( new YearPeriod( 2015 ) ) );
            Assert.That( result.Value, Is.EqualTo( 28 ) );
        }

        [Test]
        public void ProvideValue_WithPreserveCurrency_CurrencyTakenOverForResult()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 5, Euro ), CreateDatum( 2014, 7, Euro ) );
            myLhsSeries.Freeze();
            myRhsSeries = new Series( CreateDatum( 2015, 23, Euro ), CreateDatum( 2014, 37, Euro ) );
            myRhsSeries.Freeze();

            var result = ( ICurrencyDatum )myProvider.ProvideValue( myContext.Object );

            Assert.That( result.Currency, Is.EqualTo( Euro ) );
        }

        [Test]
        public void ProvideValue_WhenCalled_InputsReferenced()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 5, Euro ), CreateDatum( 2014, 7, Euro ) );
            myLhsSeries.Freeze();
            myRhsSeries = new Series( CreateDatum( 2015, 23, Euro ), CreateDatum( 2014, 37, Euro ) );
            myRhsSeries.Freeze();

            var result = ( DerivedDatum )myProvider.ProvideValue( myContext.Object );

            Assert.That( result.Inputs, Is.EquivalentTo( new[] { myLhsSeries.First(), myRhsSeries.First() } ) );
        }


        [Test]
        public void ProvideValue_InconsistentCurrencies_Throws()
        {
            myLhsSeries = new Series( CreateDatum( 2015, 5, Euro ), CreateDatum( 2014, 7, Euro ) );
            myLhsSeries.Freeze();
            myRhsSeries = new Series( CreateDatum( 2015, 23, Dollar ), CreateDatum( 2014, 37, Dollar ) );
            myRhsSeries.Freeze();

            var ex = Assert.Throws<ArgumentException>( () => myProvider.ProvideValue( myContext.Object ) );
            Assert.That( ex.Message, Is.StringContaining( "Currency inconsistencies" ) );
        }

        private IDatum CreateDatum( int year, double value )
        {
            return new Datum
            {
                Period = new YearPeriod( year ),
                Value = value,
                Source = "Dummy",
                Timestamp = DateTime.UtcNow
            };
        }

        private IDatum CreateDatum( int year, double value, Currency currency )
        {
            return new CurrencyDatum
            {
                Period = new YearPeriod( year ),
                Value = value,
                Currency = currency,
                Source = "Dummy",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
