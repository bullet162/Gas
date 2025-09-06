using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Responses;

namespace ForecastingGas.Algorithm;

public class SortPredictions : ISortPredictions
{
    public PredictionsResult SortForecast(PredictionsResult predictions)
    {
        var pred1 = predictions.MinPrediction;
        var pred2 = predictions.MaxPrediction;

        var num = Math.Min(pred1.Count, pred2.Count);

        List<decimal> minPredict = new();
        List<decimal> maxPredict = new();
        List<decimal> avePredict = new();

        for (int i = 0; i < num; i++)
        {
            var min = Math.Min(pred1[i] == pred1[i] ? pred1[i] : 0, pred2[i] == pred2[i] ? pred2[i] : 0);
            var max = Math.Max(pred1[i] == pred1[i] ? pred1[i] : 0, pred2[i] == pred2[i] ? pred2[i] : 0);
            var ave = (pred1[i] + pred2[i]) / 2;

            minPredict.Add(min);
            maxPredict.Add(max);
            avePredict.Add(ave);
        }

        return new PredictionsResult
        {
            MaxPrediction = maxPredict,
            MinPrediction = minPredict,
            AveragePrediction = avePredict
        };
    }
}