using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Error_Metrics.Interfaces;

namespace ForecastingGas.Error_Metrics.Service;

public class Error : IError
{
    public decimal CalculateMse(ErrorParams errorParams)
    {
        List<decimal> squaredErrors = new();

        var count = Math.Min(errorParams.ForecastValues.Count, errorParams.ActualValues.Count);

        for (int i = 0; i < count; i++)
        {
            decimal error = errorParams.ActualValues[i] - errorParams.ForecastValues[i];
            squaredErrors.Add(error * error);
        }

        var mse = squaredErrors.Take(squaredErrors.Count).Average();
        return mse;
    }

    public ErrorOutput EvaluateAlgoErrors(ErrorParams errorParams)
    {
        List<decimal> errors = new();
        List<decimal> absError = new();
        List<decimal> squaredErrors = new();
        List<decimal> PabsError = new();

        var count = Math.Min(errorParams.ForecastValues.Count, errorParams.ActualValues.Count);

        for (int i = errorParams.SeasonLength + 2; i < count; i++)
        {
            decimal error = errorParams.ActualValues[i] - errorParams.ForecastValues[i];
            decimal abs = Math.Abs(error);
            decimal percAbs = (errorParams.ActualValues[i] == 0) ? 0 : (abs / errorParams.ActualValues[i]) * 100;

            errors.Add(error);
            absError.Add(abs);
            squaredErrors.Add(error * error);
            PabsError.Add(percAbs);
        }


        var mae = absError.Take(absError.Count).Average();
        var mse = squaredErrors.Take(squaredErrors.Count).Average();
        var rmse = Math.Sqrt((double)mse);
        var mape = PabsError.Take(PabsError.Count).Average();

        return new ErrorOutput
        {
            MAE = mae,
            MSE = mse,
            RMSE = rmse,
            MAPE = mape
        };
    }
}