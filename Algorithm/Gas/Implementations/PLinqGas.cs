using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm.Gas.Implementations;

public class Gas
{
    private ISes _ses;
    private IHwes _hwes;

    public Gas(ISes ses, IHwes hwes)
    {
        _ses = ses;
        _hwes = hwes;
    }

    public (List<decimal> forecastValues, string method, int totalCount) ApplyGas(GasParams gasParams)
    {
        var data = gasParams.ActualValues;
        var ForecastValues = gasParams.ForecastValues;
        var alphaSes = gasParams.AlphaSes;
        var AlphaHwes = gasParams.AlphaHwes;
        var beta = gasParams.Beta;
        var gamma = gasParams.Gamma;
        var SeasonLength = gasParams.SeasonLength;
        var WindowSize = gasParams.WindowSize;
        var ForecasHorizon = gasParams.ForecasHorizon;

        var HWESparams = new HwesParams
        {
            Alpha = AlphaHwes,
            ActualValues = data,
            Beta = beta,
            Gamma = gamma,
            SeasonLength = SeasonLength,
            ForecasHorizon = ForecasHorizon
        };

        var sesParams = new SesParams
        {
            Alpha = alphaSes,
            ActualValues = data
        };



        return ([], "da", 1);
    }
}