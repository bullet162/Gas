using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Error_Metrics.Interfaces;

public interface IError
{
    (double Rmse, decimal Mae, decimal Mse, decimal Mape) CalculateErrors(ErrorParams errorParams);
}