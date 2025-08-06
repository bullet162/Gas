using System;
using System.Collections.Generic;
using System.Linq;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm.Hwes
{
    public class AdditiveHwes : IHwes
    {
        private List<decimal> _level = new();
        private List<decimal> _trend = new();
        private List<decimal> _seasonal = new();
        private int _seasonLength;

        public (List<decimal> trainedForecast, string model, int totalCount) TrainForecast(HwesParams hwesParams)
        {

            _seasonLength = hwesParams.SeasonLength;
            var data = hwesParams.ActualValues;

            var initialLevel = data.Take(_seasonLength).Average();

            decimal sum = 0;
            for (int i = 0; i < _seasonLength; i++)
            {
                sum += (data[_seasonLength + i] - data[i]) / _seasonLength;
                _level.Add(0.0M);
                _trend.Add(0.0M);
            }
            _level.Add(initialLevel);
            _trend.Add(sum / _seasonLength);

            for (int i = 0; i < _seasonLength; i++)
            {
                _seasonal.Add(data[i] - _level[0] - _trend[0] * i);
            }
            for (int i = 0; i < _seasonLength + 1; i++)
            {
                hwesParams.ForecastValues.Add(0.0m);
            }

            var diff = hwesParams.ActualValues.Count - hwesParams.SeasonLength;

            if (diff <= hwesParams.SeasonLength)
                throw new ArgumentOutOfRangeException
                ($"Season Length too large for the current data size. ({hwesParams.ActualValues.Count}). Please reduce the season length or provide more data.");

            const string modelName = "HWES";
            var alpha = hwesParams.Alpha;
            var beta = hwesParams.Beta;
            var gamma = hwesParams.Gamma;

            for (int i = _seasonLength + 1; i < data.Count; i++)
            {
                int seasonIndex = i % _seasonLength;
                int prevSeasonIndex = ((i - _seasonLength + 1) % _seasonLength + _seasonLength) % _seasonLength;

                decimal newLevel = alpha * (data[i] - _seasonal[prevSeasonIndex])
                                + (1 - alpha) * (_level[i - 1] + _trend[i - 1]);
                _level.Add(newLevel);

                decimal newTrend = beta * (_level[i] - _level[i - 1])
                                + (1 - beta) * _trend[i - 1];
                _trend.Add(newTrend);

                decimal newSeasonal = gamma * (data[i] - _level[i] - _trend[i])
                                   + (1 - gamma) * _seasonal[prevSeasonIndex];
                _seasonal.Add(newSeasonal);

                if (i >= _seasonLength + 2)
                {
                    hwesParams.ForecastValues.Add(_level[i] + _trend[i] + _seasonal[seasonIndex]);
                }

            }
            return new(hwesParams.ForecastValues, modelName, hwesParams.ForecastValues.Count);
        }

        public List<decimal> GenerateForecasts(HwesParams hwesParams)
        {
            var horizon = hwesParams.ForecasHorizon;
            hwesParams.ForecastValues = new();
            var forecasts = hwesParams.ForecastValues;
            for (int i = 1; i <= horizon; i++)
            {
                if (_seasonal.Count < _seasonLength || _level.Count == 0 || _trend.Count == 0)
                    throw new InvalidOperationException("Model must be trained before generating forecasts.");

                int seasonIndex = (_seasonal.Count - _seasonLength + (i % _seasonLength)) % _seasonLength;
                hwesParams.ForecastValues.Add(_level[^1] + i * _trend[^1] + _seasonal[seasonIndex]);
            }

            return forecasts;
        }
    }
}