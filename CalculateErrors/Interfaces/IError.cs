using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Error_Metrics.Interfaces;

public interface IError
{
    decimal CalculateMse(ErrorParams errorParams);
}