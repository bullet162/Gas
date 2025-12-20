using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface ISaveData
{
    Task<bool> SaveRawData(RawDataOutput dataOutput);
}