using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Error_Metrics.Interfaces;

public interface IError
{
    decimal CalculateMse(ErrorParams errorParams);
    ErrorOutput EvaluateAlgoErrors(ErrorParams errorParams);
    ErrorOutput EvaluateAlgoErrors(ErrorEvaluate errorParams);
}