using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qwack.Core.Basic;
using Qwack.Core.Instruments.Asset;
using Qwack.Dates;
using Qwack.Transport.BasicTypes;
using Qwack.Transport.TransportObjects.Instruments;
using Qwack.Transport.TransportObjects.Instruments.Asset;

namespace Qwack.Core.Instruments
{
    public static class InstrumentFactory
    {
        public static IInstrument GetInstrument(this TO_Instrument transportObject, ICurrencyProvider currencyProvider, ICalendarProvider calendarProvider)
        {
            if (transportObject.AssetInstrumentType != AssetInstrumentType.None)
            {
                switch (transportObject.AssetInstrumentType)
                {
                    case AssetInstrumentType.AsianSwap:
                        return transportObject.AsianSwap.GetAsianSwap(currencyProvider, calendarProvider);
                    case AssetInstrumentType.AsianSwapStrip:
                        return new AsianSwapStrip
                        {
                            TradeId = transportObject.AsianSwapStrip.TradeId,
                            Counterparty = transportObject.AsianSwapStrip.Counterparty,
                            PortfolioName = transportObject.AsianSwapStrip.PortfolioName,
                            Swaplets = transportObject.AsianSwapStrip.Swaplets.Select(x => x.GetAsianSwap(currencyProvider,calendarProvider)).ToArray(),
                            HedgingSet = transportObject.AsianSwapStrip.HedgingSet,
                        };
                    case AssetInstrumentType.AsianOption:
                        var ao = (AsianOption)GetAsianSwap(transportObject.AsianOption, currencyProvider, calendarProvider);
                        ao.CallPut = transportObject.AsianOption.CallPut;
                        return ao;
                    case AssetInstrumentType.Forward:
                        return transportObject.Forward.GetForward(currencyProvider, calendarProvider);
                }
            }
            else
            {
                switch (transportObject.FundingInstrumentType)
                {
                }
            }

            throw new Exception("Unable to re-constitute object");
        }

        public static Portfolio GetPortfolio(this TO_Portfolio transportObject, ICurrencyProvider currencyProvider, ICalendarProvider calendarProvider) => new Portfolio
        {
            PortfolioName = transportObject.PortfolioName,
            Instruments = transportObject.Instruments.Select(x => x.GetInstrument(currencyProvider, calendarProvider)).ToList()
        };

        private static AsianSwap GetAsianSwap(this TO_AsianSwap transportObject, ICurrencyProvider currencyProvider, ICalendarProvider calendarProvider) => new AsianSwap
        {
            TradeId = transportObject.TradeId,
            Notional = transportObject.Notional,
            Direction = transportObject.Direction,
            AverageStartDate = transportObject.AverageStartDate,
            AverageEndDate = transportObject.AverageEndDate,
            FixingDates = transportObject.FixingDates,
            FixingCalendar = calendarProvider.GetCalendarSafe(transportObject.FixingCalendar),
            PaymentCalendar = calendarProvider.GetCalendarSafe(transportObject.PaymentCalendar),
            SpotLag = new Frequency(transportObject.SpotLag),
            SpotLagRollType = transportObject.SpotLagRollType,
            PaymentLag = new Frequency(transportObject.PaymentLag),
            PaymentLagRollType = transportObject.PaymentLagRollType,
            PaymentDate = transportObject.PaymentDate,
            PaymentCurrency = currencyProvider.GetCurrencySafe(transportObject.PaymentCurrency),
            AssetFixingId = transportObject.AssetFixingId,
            AssetId = transportObject.AssetId,
            DiscountCurve = transportObject.DiscountCurve,
            FxConversionType = transportObject.FxConversionType,
            FxFixingDates = transportObject.FxFixingDates,
            FxFixingId = transportObject.FxFixingId,
            Strike = transportObject.Strike,
            Counterparty = transportObject.Counterparty,
            HedgingSet = transportObject.HedgingSet,
            PortfolioName = transportObject.PortfolioName,
        };

        private static Forward GetForward(this TO_Forward transportObject, ICurrencyProvider currencyProvider, ICalendarProvider calendarProvider) => new Forward
        {
            TradeId = transportObject.TradeId,
            Notional = transportObject.Notional,
            Direction = transportObject.Direction,
            ExpiryDate = transportObject.ExpiryDate,
            FixingCalendar = calendarProvider.GetCalendarSafe(transportObject.FixingCalendar),
            PaymentCalendar = calendarProvider.GetCalendarSafe(transportObject.PaymentCalendar),
            SpotLag = new Frequency(transportObject.SpotLag),
            PaymentLag = new Frequency(transportObject.PaymentLag),
            Strike = transportObject.Strike,
            AssetId = transportObject.AssetId,
            PaymentCurrency = currencyProvider.GetCurrencySafe(transportObject.PaymentCurrency),
            FxFixingId = transportObject.FxFixingId,
            DiscountCurve = transportObject.DiscountCurve,
            PaymentDate = transportObject.PaymentDate,
            Counterparty = transportObject.Counterparty,
            FxConversionType = transportObject.FxConversionType,
            HedgingSet = transportObject.HedgingSet,
            PortfolioName = transportObject.PortfolioName,
        };
    }
}
