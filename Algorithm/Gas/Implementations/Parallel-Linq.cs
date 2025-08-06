using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm.Gas.Implementations;

public class PGas
{
    private ISes _ses;
    private IHwes _hwes;

    public PGas(ISes ses, IHwes hwes)
    {
        _ses = ses;
        _hwes = hwes;
    }

    public (List<decimal> forecastValues, string method, int totalCount) ApplyPGas()
    {


        return ([], "da", 1);
    }
}