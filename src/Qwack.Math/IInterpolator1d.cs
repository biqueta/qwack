using Qwack.Transport.BasicTypes;

namespace Qwack.Math
{
    public interface IInterpolator1D
    {
        Interpolator1DType Type { get; }
        IInterpolator1D Bump(int pillar, double delta, bool updateInPlace = false);
        IInterpolator1D UpdateY(int pillar, double newValue, bool updateInPlace = false);
        double Interpolate(double t);
        double FirstDerivative(double x);
        double SecondDerivative(double x);
        double[] Sensitivity(double x);
    }
}
