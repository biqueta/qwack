namespace Qwack.Transport.BasicTypes
{
    /// <summary>
    /// A list of methods for calculating a year fraction from a pair of dates
    /// </summary>
    public enum DayCountBasis
    {
        ACT360 = 360,
        Act360 = 360,
        Act_360 = 360,
        ACT365 = 365,
        Act365 = 365,
        Act_365 = 365,
        Act_Act = 365,

        Act_Act_ISDA,
        Act_Act_ICMA,

        Act_365F = 3650,
        Act365F = 3650,
        ACT365F = 3650,

        Act_364 = 364,
        Act364 = 364,
        ACT364 = 364,

        _30_360 = 30360,
        Thirty360 = 30360,

        _30E_360 = 303600,
        ThirtyE360 = 303600,

        Bus252 = 252,
        BUS252 = 252,
        Bus_252 = 252,

        Unity = 1
    }
}
