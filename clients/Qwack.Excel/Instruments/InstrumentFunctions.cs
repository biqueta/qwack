using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcelDna.Integration;
using Qwack.Core.Curves;
using Qwack.Core.Basic;
using Qwack.Excel.Services;
using Qwack.Excel.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Qwack.Dates;
using Qwack.Core.Instruments.Asset;
using Qwack.Core.Instruments.Funding;
using Qwack.Core.Instruments;
using Qwack.Core.Models;
using Qwack.Models.Models;
using Qwack.Core.Cubes;

namespace Qwack.Excel.Curves
{
    public class InstrumentFunctions
    {
        private static readonly ILogger _logger = ContainerStores.GlobalContainer.GetService<ILoggerFactory>()?.CreateLogger<InstrumentFunctions>();

        [ExcelFunction(Description = "Creates an asian swap", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(CreateAsianSwap))]
        public static object CreateAsianSwap(
             [ExcelArgument(Description = "Object name")] string ObjectName,
             [ExcelArgument(Description = "Period code or dates")] object PeriodCodeOrDates,
             [ExcelArgument(Description = "Asset Id")] string AssetId,
             [ExcelArgument(Description = "Currency")] string Currency,
             [ExcelArgument(Description = "Strike")] double Strike,
             [ExcelArgument(Description = "Notional")] double Notional,
             [ExcelArgument(Description = "Fixing calendar")] object FixingCalendar,
             [ExcelArgument(Description = "Payment calendar")] object PaymentCalendar,
             [ExcelArgument(Description = "Payment offset")] object PaymentOffset,
             [ExcelArgument(Description = "Spot lag")] object SpotLag,
             [ExcelArgument(Description = "Fixing date generation type")] object DateGenerationType,
             [ExcelArgument(Description = "Discount curve")] string DiscountCurve)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var fixingCal = FixingCalendar.OptionalExcel("WeekendsOnly");
                var paymentCal = PaymentCalendar.OptionalExcel("WeekendsOnly");
                var paymentOffset = PaymentOffset.OptionalExcel("0b");
                var spotLag = PaymentOffset.OptionalExcel("0b");
                var dGenType = DateGenerationType.OptionalExcel("BusinessDays");

                if (!ContainerStores.SessionContainer.GetService<ICalendarProvider>().Collection.TryGetCalendar(fixingCal, out var fCal))
                {
                    _logger?.LogInformation("Calendar {calendar} not found in cache", fixingCal);
                    return $"Calendar {fixingCal} not found in cache";
                }
                if (!ContainerStores.SessionContainer.GetService<ICalendarProvider>().Collection.TryGetCalendar(paymentCal, out var pCal))
                {
                    _logger?.LogInformation("Calendar {calendar} not found in cache", paymentCal);
                    return $"Calendar {paymentCal} not found in cache";
                }
                var pOffset = new Frequency(paymentOffset);
                var sLag = new Frequency(spotLag);
                if (!Enum.TryParse(dGenType, out DateGenerationType dType))
                {
                    return $"Could not parse date generation type - {dGenType}";
                }
                var currency = new Currency(Currency, DayCountBasis.Act365F, pCal);

                AsianSwapStrip product;
                if(PeriodCodeOrDates is object[,])
                {
                    var dates = ((object[,])PeriodCodeOrDates).ObjectRangeToVector<double>().ToDateTimeArray();
                    product = AssetProductFactory.CreateMonthlyAsianSwap(dates[0],dates[1], Strike, AssetId, fCal, pCal, pOffset, currency, TradeDirection.Long, sLag, Notional, dType);
                }
                else if (PeriodCodeOrDates is double)
                {
                    PeriodCodeOrDates = DateTime.FromOADate((double)PeriodCodeOrDates).ToString("MMM-yy");
                    product = AssetProductFactory.CreateMonthlyAsianSwap(PeriodCodeOrDates as string, Strike, AssetId, fCal, pCal, pOffset, currency, TradeDirection.Long, sLag, Notional, dType);
                }
                else
                    product = AssetProductFactory.CreateMonthlyAsianSwap(PeriodCodeOrDates as string, Strike, AssetId, fCal, pCal, pOffset, currency, TradeDirection.Long, sLag, Notional, dType);

                product.TradeId = ObjectName;
                foreach(var s in product.Swaplets)
                {
                    s.DiscountCurve = DiscountCurve;
                }

                var cache = ContainerStores.GetObjectCache<AsianSwapStrip>();
                cache.PutObject(ObjectName, new SessionItem<AsianSwapStrip> { Name = ObjectName, Value = product });
                return ObjectName + '¬' + cache.GetObject(ObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Creates an asian option", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(CreateAsianOption))]
        public static object CreateAsianOption(
             [ExcelArgument(Description = "Object name")] string ObjectName,
             [ExcelArgument(Description = "Period code")] object PeriodCodeOrDates,
             [ExcelArgument(Description = "Asset Id")] string AssetId,
             [ExcelArgument(Description = "Currency")] string Currency,
             [ExcelArgument(Description = "Strike")] double Strike,
             [ExcelArgument(Description = "Put/Call")] string PutOrCall,
             [ExcelArgument(Description = "Notional")] double Notional,
             [ExcelArgument(Description = "Fixing calendar")] object FixingCalendar,
             [ExcelArgument(Description = "Payment calendar")] object PaymentCalendar,
             [ExcelArgument(Description = "Payment offset")] object PaymentOffset,
             [ExcelArgument(Description = "Spot lag")] object SpotLag,
             [ExcelArgument(Description = "Fixing date generation type")] object DateGenerationType,
             [ExcelArgument(Description = "Discount curve")] string DiscountCurve)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var fixingCal = FixingCalendar.OptionalExcel("WeekendsOnly");
                var paymentCal = PaymentCalendar.OptionalExcel("WeekendsOnly");
                var paymentOffset = PaymentOffset.OptionalExcel("0b");
                var spotLag = PaymentOffset.OptionalExcel("0b");
                var dGenType = DateGenerationType.OptionalExcel("BusinessDays");

                if (!Enum.TryParse(PutOrCall, out OptionType oType))
                {
                    return $"Could not parse put/call flag - {PutOrCall}";
                }

                if (!ContainerStores.SessionContainer.GetService<ICalendarProvider>().Collection.TryGetCalendar(fixingCal, out var fCal))
                {
                    _logger?.LogInformation("Calendar {calendar} not found in cache", fixingCal);
                    return $"Calendar {fixingCal} not found in cache";
                }
                if (!ContainerStores.SessionContainer.GetService<ICalendarProvider>().Collection.TryGetCalendar(paymentCal, out var pCal))
                {
                    _logger?.LogInformation("Calendar {calendar} not found in cache", paymentCal);
                    return $"Calendar {paymentCal} not found in cache";
                }
                var pOffset = new Frequency(paymentOffset);
                var sLag = new Frequency(spotLag);
                if (!Enum.TryParse(dGenType, out DateGenerationType dType))
                {
                    return $"Could not parse date generation type - {dGenType}";
                }
                var currency = new Currency(Currency, DayCountBasis.Act365F, pCal);

                AsianOption product;
                if (PeriodCodeOrDates is object[,])
                {
                    var dates = ((object[,])PeriodCodeOrDates).ObjectRangeToVector<double>().ToDateTimeArray();
                    product = AssetProductFactory.CreatAsianOption(dates[0], dates[1], Strike, AssetId, oType, fCal, pCal, pOffset, currency, TradeDirection.Long, sLag, Notional, dType);
                }
                else if (PeriodCodeOrDates is double)
                {
                    PeriodCodeOrDates = DateTime.FromOADate((double)PeriodCodeOrDates).ToString("MMM-yy");
                    product = AssetProductFactory.CreatAsianOption(PeriodCodeOrDates as string, Strike, AssetId, oType, fCal, pCal, pOffset, currency, TradeDirection.Long, sLag, Notional, dType);
                }
                else
                    product = AssetProductFactory.CreatAsianOption(PeriodCodeOrDates as string, Strike, AssetId, oType, fCal, pCal, pOffset, currency, TradeDirection.Long, sLag, Notional, dType);

                product.TradeId = ObjectName;
                product.DiscountCurve = DiscountCurve;
                
                var cache = ContainerStores.GetObjectCache<AsianOption>();
                cache.PutObject(ObjectName, new SessionItem<AsianOption> { Name = ObjectName, Value = product });
                return ObjectName + '¬' + cache.GetObject(ObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Returns PV of a portfolio given an AssetFx model", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(AssetPortfolioPV))]
        public static object AssetPortfolioPV(
           [ExcelArgument(Description = "Result object name")] string ResultObjectName,
           [ExcelArgument(Description = "Portolio object name")] string PortfolioName,
           [ExcelArgument(Description = "Asset-FX model name")] string ModelName)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var pfolio = ContainerStores.GetObjectCache<Portfolio>().GetObject(PortfolioName);
                var model = ContainerStores.GetObjectCache<IAssetFxModel>().GetObject(ModelName);

                var result = pfolio.Value.PV(model.Value);
                var resultCache = ContainerStores.GetObjectCache<ICube>();
                resultCache.PutObject(ResultObjectName, new SessionItem<ICube> { Name = ResultObjectName, Value = result });
                return ResultObjectName + '¬' + resultCache.GetObject(ResultObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Returns asset delta of a portfolio given an AssetFx model", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(AssetPortfolioDelta))]
        public static object AssetPortfolioDelta(
            [ExcelArgument(Description = "Result object name")] string ResultObjectName,
            [ExcelArgument(Description = "Portolio object name")] string PortfolioName,
            [ExcelArgument(Description = "Asset-FX model name")] string ModelName)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var pfolio = ContainerStores.GetObjectCache<Portfolio>().GetObject(PortfolioName);
                var model = ContainerStores.GetObjectCache<IAssetFxModel>().GetObject(ModelName);

                var result = pfolio.Value.AssetDelta(model.Value);
                var resultCache = ContainerStores.GetObjectCache<ICube>();
                resultCache.PutObject(ResultObjectName, new SessionItem<ICube> { Name = ResultObjectName, Value = result });
                return ResultObjectName + '¬' + resultCache.GetObject(ResultObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Returns asset delta and gamma of a portfolio given an AssetFx model", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(AssetPortfolioDeltaGamma))]
        public static object AssetPortfolioDeltaGamma(
            [ExcelArgument(Description = "Result object name")] string ResultObjectName,
            [ExcelArgument(Description = "Portolio object name")] string PortfolioName,
            [ExcelArgument(Description = "Asset-FX model name")] string ModelName)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var pfolio = ContainerStores.GetObjectCache<Portfolio>().GetObject(PortfolioName);
                var model = ContainerStores.GetObjectCache<IAssetFxModel>().GetObject(ModelName);

                var result = pfolio.Value.AssetDeltaGamma(model.Value);
                var resultCache = ContainerStores.GetObjectCache<ICube>();
                resultCache.PutObject(ResultObjectName, new SessionItem<ICube> { Name = ResultObjectName, Value = result });
                return ResultObjectName + '¬' + resultCache.GetObject(ResultObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Returns asset vega of a portfolio given an AssetFx model", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(AssetPortfolioVega))]
        public static object AssetPortfolioVega(
            [ExcelArgument(Description = "Result object name")] string ResultObjectName,
            [ExcelArgument(Description = "Portolio object name")] string PortfolioName,
            [ExcelArgument(Description = "Asset-FX model name")] string ModelName)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var pfolio = ContainerStores.GetObjectCache<Portfolio>().GetObject(PortfolioName);
                var model = ContainerStores.GetObjectCache<IAssetFxModel>().GetObject(ModelName);

                var result = pfolio.Value.AssetVega(model.Value);
                var resultCache = ContainerStores.GetObjectCache<ICube>();
                resultCache.PutObject(ResultObjectName, new SessionItem<ICube> { Name = ResultObjectName, Value = result });
                return ResultObjectName + '¬' + resultCache.GetObject(ResultObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Returns fx delta of a portfolio given an AssetFx model", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(AssetPortfolioFxDelta))]
        public static object AssetPortfolioFxDelta(
            [ExcelArgument(Description = "Result object name")] string ResultObjectName,
            [ExcelArgument(Description = "Portolio object name")] string PortfolioName,
            [ExcelArgument(Description = "Asset-FX model name")] string ModelName)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var pfolio = ContainerStores.GetObjectCache<Portfolio>().GetObject(PortfolioName);
                var model = ContainerStores.GetObjectCache<IAssetFxModel>().GetObject(ModelName);

                var result = pfolio.Value.FxDelta(model.Value);
                var resultCache = ContainerStores.GetObjectCache<ICube>();
                resultCache.PutObject(ResultObjectName, new SessionItem<ICube> { Name = ResultObjectName, Value = result });
                return ResultObjectName + '¬' + resultCache.GetObject(ResultObjectName).Version;
            });
        }

        [ExcelFunction(Description = "Creates a portfolio of instruments", Category = CategoryNames.Instruments, Name = CategoryNames.Instruments + "_" + nameof(CreatePortfolio))]
        public static object CreatePortfolio(
            [ExcelArgument(Description = "Object name")] string ObjectName,
            [ExcelArgument(Description = "Instruments")] object[] Instruments)
        {
            return ExcelHelper.Execute(_logger, () =>
            {
                var swapCache = ContainerStores.GetObjectCache<IrSwap>();
                var swaps = Instruments.GetAnyFromCache<IrSwap>(); 
                var fraCache = ContainerStores.GetObjectCache<ForwardRateAgreement>();
                var fras = Instruments.GetAnyFromCache<ForwardRateAgreement>(); 
                var stirCache = ContainerStores.GetObjectCache<STIRFuture>();
                var futures = Instruments.GetAnyFromCache<STIRFuture>(); 
                var fxFwds = Instruments.GetAnyFromCache<FxForward>(); 
                var xccySwaps = Instruments.GetAnyFromCache<XccyBasisSwap>();      
                var basisSwaps = Instruments.GetAnyFromCache<IrBasisSwap>();

                var asianOptions = Instruments.GetAnyFromCache<AsianOption>();
                var asianStrips = Instruments.GetAnyFromCache<AsianSwapStrip>();
                var asianSwaps = Instruments.GetAnyFromCache<AsianSwap>();
                var forwards = Instruments.GetAnyFromCache<Forward>();
                var europeanOptions = Instruments.GetAnyFromCache<EuropeanOption>();

                //allows merging of FICs into portfolios
                var ficInstruments = Instruments.GetAnyFromCache<FundingInstrumentCollection>()
                    .SelectMany(s => s);

                //allows merging of portfolios into portfolios
                var pfInstruments = Instruments.GetAnyFromCache<Portfolio>()
                    .SelectMany(s => s.Instruments);

                var pf = new Portfolio
                {
                    Instruments = new List<IInstrument>()
                };

                pf.Instruments.AddRange(swaps);
                pf.Instruments.AddRange(fras);
                pf.Instruments.AddRange(futures);
                pf.Instruments.AddRange(fxFwds);
                pf.Instruments.AddRange(xccySwaps);
                pf.Instruments.AddRange(basisSwaps);
                pf.Instruments.AddRange(ficInstruments);

                pf.Instruments.AddRange(pfInstruments);
                pf.Instruments.AddRange(asianOptions);
                pf.Instruments.AddRange(asianStrips);
                pf.Instruments.AddRange(asianSwaps);
                pf.Instruments.AddRange(forwards);
                pf.Instruments.AddRange(europeanOptions);

                var pFolioCache = ContainerStores.GetObjectCache<Portfolio>();

                pFolioCache.PutObject(ObjectName, new SessionItem<Portfolio> { Name = ObjectName, Value = pf });
                return ObjectName + '¬' + pFolioCache.GetObject(ObjectName).Version;
            });
        }
    }
}
