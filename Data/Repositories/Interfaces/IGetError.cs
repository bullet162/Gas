using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface IGetError
{
    Task<ErrorOutput> GetErrorOutputsById(int Id);
}