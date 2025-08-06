using System;
using System.Collections.Generic;
using System.Linq;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;

namespace ForecastingGas.Algorithm.Hwes
{
    public class AdditiveHwes : IHwes
    {
        private List<decimal> _level = null!;
        private List<decimal> _trend = null!;
        private List<decimal> _seasonal = null!;
        private int _seasonLength;

        public void InitializeComponents(HwesParams hwesParams)
        {
            _seasonLength = hwesParams.SeasonLength;
            var data = hwesParams.ActualValues;
            _level = hwesParams.Level;
            _trend = hwesParams.Trend;
            _seasonal = hwesParams.Seasonal;

            var forecast = hwesParams.ForecastValues;

            var initialLevel = data.Take(_seasonLength).Average();

            decimal sum = 0;
            for (int i = 0; i <= _seasonLength; i++)
            {
                sum += (data[_seasonLength + i] - data[i]) / _seasonLength;
                _level.Add(0.0M);
                _trend.Add(0.0M);
            }
            _level.Add(initialLevel);
            _trend.Add(sum / _seasonLength);

            for (int i = 0; i < _seasonLength; i++)
            {
                _seasonal.Add(data[i] - _level[0] - _trend[0] * (i + 1));
            }
            for (int i = 0; i <= _seasonLength + 1; i++)
            {
                forecast.Add(0.0m);
            }

        }

        public (List<decimal> trainedForecast, string model, int totalCount) TrainForecast(HwesParams hwesParams)
        {
            _level = hwesParams.Level;
            _trend = hwesParams.Trend;
            _seasonal = hwesParams.Seasonal;
            var diff = hwesParams.ActualValues.Count - hwesParams.SeasonLength;
            if (diff <= hwesParams.SeasonLength)
                throw new ArgumentOutOfRangeException
                ($"Season Length too large for the current data size. ({hwesParams.ActualValues.Count}). Please reduce the season length or provide more data.");

            const string modelName = "HWES";
            var data = hwesParams.ActualValues;
            var alpha = hwesParams.Alpha;
            var beta = hwesParams.Beta;
            var gamma = hwesParams.Gamma;
            var forecast = hwesParams.ForecastValues;
            _seasonLength = hwesParams.SeasonLength;

            for (int i = _seasonLength + 1; i < data.Count; i++)
            {
                int seasonIndex = i % _seasonLength;
                int prevSeasonIndex = (i - _seasonLength + 1) % _seasonLength;

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
                    forecast.Add(_level[i] + _trend[i] + _seasonal[seasonIndex]);
                }

            }
            return new(forecast, modelName, forecast.Count);
        }

        public List<decimal> GenerateForecasts(HwesParams hwesParams)
        {
            var horizon = hwesParams.ForecasHorizon;
            hwesParams.ForecastValues = new();
            var forecasts = hwesParams.ForecastValues;
            for (int i = 1; i <= horizon; i++)
            {
                int seasonIndex = (_seasonal.Count - _seasonLength + (i % _seasonLength)) % _seasonLength;
                decimal forecast = _level[^1] + i * _trend[^1] + _seasonal[seasonIndex];
                forecasts.Add(forecast);
            }

            return forecasts;
        }
    }
}