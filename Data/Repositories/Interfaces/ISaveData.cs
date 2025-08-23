using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Data.Repositories.Interfaces;

public interface ISaveData
{
    Task<bool> SaveDatas(ALgoOutput output);
    Task<bool> SaveRawData(RawDataOutput dataOutput);
    Task<bool> SaveErrorData(ErrorOutput output);

}