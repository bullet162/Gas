using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface ISaveData
{
    Task<ForecastDescription> SaveDatas(ALgoOutput output);
    Task<bool> SaveRawData(RawDataOutput dataOutput);
    Task<bool> SaveErrorData(ErrorOutput output);

}