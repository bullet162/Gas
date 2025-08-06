using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm.Gas.Implementations;

/* in 1st loop 
seasonLength = seasonLength

in 2nd loop to n loop
seasonLength = 0;

*/
public class MTGas
{
    private ISes _ses;
    private IHwes _hwes;

    public MTGas(ISes ses, IHwes hwes)
    {
        _ses = ses;
        _hwes = hwes;
    }

    private (SesParams ses1, HwesParams hwes1) GetParameter(GasParams gasParams)
    {
        var data = gasParams.ActualValues;
        var ForecastValues = gasParams.ForecastValues;
        var alphaSes = gasParams.AlphaSes;
        var AlphaHwes = gasParams.AlphaHwes;
        var beta = gasParams.Beta;
        var gamma = gasParams.Gamma;
        var SeasonLength = gasParams.SeasonLength;
        var ForecasHorizon = gasParams.ForecasHorizon;

        return new(new SesParams
        {
            ActualValues = data,
            Alpha = alphaSes
        },
        new HwesParams
        {
            ActualValues = data,
            ForecastValues = ForecastValues,
            Alpha = AlphaHwes,
            Beta = beta,
            Gamma = gamma,
            SeasonLength = SeasonLength,
            ForecasHorizon = ForecasHorizon
        });
    }

    private (List<decimal> ses, List<decimal> hwes) Calculate(GasParams gasParams)
    {
        var components = GetParameter(gasParams);
        var hwesParams = components.hwes1;
        var sesParams = components.ses1;

        (List<decimal>, string, int) hwesResult = new();
        (List<decimal>, string, int) sesResult = new();

        Thread hwes = new Thread(() =>
         {
             hwesResult = _hwes.TrainForecast(hwesParams);
         });

        Thread ses = new Thread(() =>
        {
            sesResult = _ses.SesForecast(sesParams);
        });

        hwes.Start();
        ses.Start();

        hwes.Join();
        ses.Join();

        return new(sesResult.Item1!, hwesResult.Item1!);
    }

    public (List<decimal> forecastvalues, string model, int TotalCount) ApplyGasMt(GasParams gasParams)
    {
        var WindowSize = gasParams.WindowSize;
        var data = gasParams.ActualValues;

        for (int i = 0; i < data.Count; i += WindowSize)
        {
            var windowData = data.Take(WindowSize).ToList();

            var adjustedParams = new GasParams
            {
                ActualValues = windowData,
                ForecastValues = gasParams.ForecastValues,
                AlphaSes = gasParams.AlphaSes,
                AlphaHwes = gasParams.AlphaHwes,
                Beta = gasParams.Beta,
                Gamma = gasParams.Gamma,
                SeasonLength = gasParams.SeasonLength,
                ForecasHorizon = gasParams.ForecasHorizon,
                WindowSize = gasParams.WindowSize
            };
            _hwes.InitializeComponents(GetParameter(adjustedParams).hwes1);

            var results = Calculate(adjustedParams);

            var hwesResult = results.hwes;
            var sesResult = results.ses;

            Console.WriteLine($"hwes forecast: {hwesResult.Count}");
            Console.WriteLine($"ses forecast: {sesResult.Count}");

            if (hwesResult == null || sesResult == null)
                throw new Exception("Forecast results are null.");

            var evaluateHwes = new ErrorParams
            {
                ActualValues = windowData,
                ForecastValues = hwesResult,
                SeasonLength = adjustedParams.SeasonLength
            };

            var evaluateSes = new ErrorParams
            {
                ActualValues = windowData,
                ForecastValues = sesResult,
                SeasonLength = adjustedParams.SeasonLength

            };


        }


        return ([], "", 0);
    }
}

