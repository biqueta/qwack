using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qwack.Options;
using Xunit;
using Qwack.Core.Basic;

namespace Qwack.Math.Tests.Options.VolSurfaces
{
    public class RiskyFlyVolSurfaceFacts
    {
        [Fact]
        public void RiskyFlySimple()
        {
            //flat surface
            var origin = new DateTime(2017, 02, 07);
            var atms = new double[] { 0.3, 0.32, 0.34 };
            var fwds = new double[] { 100, 102, 110 };
            var maturities = new DateTime[] { new DateTime(2017, 04, 06), new DateTime(2017, 06, 07), new DateTime(2017, 08, 07) };
            var wingDeltas = new[] { 0.1, 0.25 };
            var riskies = new[] {  new[] { 0.02, 0.025, 0.03 }, new[] { 0.01, 0.015, 0.02 } };
            var flies = new[] {  new[] { 0.009, 0.012, 0.015 }, new[] { 0.005, 0.006, 0.007 } };
            var surface = new Qwack.Options.VolSurfaces.RiskyFlySurface(
                origin, atms, maturities, wingDeltas, riskies, flies, fwds, WingQuoteType.Simple,
                AtmVolType.ZeroDeltaStraddle, Math.Interpolation.Interpolator1DType.Linear,
                Math.Interpolation.Interpolator1DType.LinearInVariance);

            Assert.Equal(atms[1], surface.GetVolForDeltaStrike(0.5, maturities[1], fwds[1]));
        }

        [Fact]
        public void WorksWIthBackwardsStrikes()
        {
            //flat surface
            var origin = new DateTime(2017, 02, 07);
            var atms = new double[] { 0.3, 0.32, 0.34 };
            var fwds = new double[] { 100, 102, 110 };
            var maturities = new DateTime[] { new DateTime(2017, 04, 06), new DateTime(2017, 06, 07), new DateTime(2017, 08, 07) };
            var wingDeltas = new[] { 0.1, 0.25 };
            var riskies = new[] { new[] { 0.02, 0.025, 0.03 }, new[] { 0.01, 0.015, 0.02 } };
            var flies = new[] { new[] { 0.009, 0.012, 0.015 }, new[] { 0.005, 0.006, 0.007 } };
            var surface = new Qwack.Options.VolSurfaces.RiskyFlySurface(
                origin, atms, maturities, wingDeltas, riskies, flies, fwds, WingQuoteType.Simple,
                AtmVolType.ZeroDeltaStraddle, Math.Interpolation.Interpolator1DType.Linear,
                Math.Interpolation.Interpolator1DType.LinearInVariance);

            var wingDeltas2 = new[] { 0.25, 0.1 };
            var riskies2 = new[] { new[]  { 0.01, 0.015, 0.02 }, new[] { 0.02, 0.025, 0.03 } };
            var flies2 = new[] { new[] { 0.005, 0.006, 0.007 } , new[] { 0.009, 0.012, 0.015 }  };
            var surface2 = new Qwack.Options.VolSurfaces.RiskyFlySurface(
                origin, atms, maturities, wingDeltas2, riskies2, flies2, fwds, WingQuoteType.Simple,
                AtmVolType.ZeroDeltaStraddle, Math.Interpolation.Interpolator1DType.Linear,
                Math.Interpolation.Interpolator1DType.LinearInVariance);

            Assert.Equal(surface.GetVolForDeltaStrike(0.5, maturities[1], fwds[1]), surface2.GetVolForDeltaStrike(0.5, maturities[1], fwds[1]));
            Assert.Equal(surface.GetVolForDeltaStrike(0.2, maturities[0], fwds[0]), surface2.GetVolForDeltaStrike(0.2, maturities[0], fwds[0]));
            Assert.Equal(surface.GetVolForDeltaStrike(0.1, maturities[2], fwds[2]), surface2.GetVolForDeltaStrike(0.1, maturities[2], fwds[2]));
            
        }
    }
}
