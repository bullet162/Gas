using System;
using System.Collections.Generic;
using System.Linq;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Dto.Requests;
using ForecastingGas.Dto.Responses;
using ForecastingGas.Utils.Interfaces;

namespace ForecastingGas.Algorithm.Hwes
{
    public class AdditiveHwes : IHwes
    {
        private int _seasonLength;
        private IWatch _watch;
        public AdditiveHwes(IWatch watch)
        {
            _watch = watch;
        }

        public ALgoOutput TrainForecast(HwesParams hwesParams)
        {
            _watch.StartWatch();
            List<decimal> _level = hwesParams.LevelValues;
            List<decimal> _trend = hwesParams.TrendValues;
            List<decimal> _seasonal = hwesParams.SeasonalValues;

            _seasonLength = hwesParams.SeasonLength;
            var data = hwesParams.ActualValues;

            if (data.Count < 2 * _seasonLength)
            {
                throw new ArgumentOutOfRangeException(
                    $"Need at least {2 * _seasonLength} data points for seasonLength {_seasonLength}, " +
                    $"but only {data.Count} were provided.");
            }

            var initialLevel = data.Take(_seasonLength).Average();

            decimal sum = 0;
            for (int i = 0; i < _seasonLength; i++)
            {
                sum += (data[i + _seasonLength] - data[i]) / _seasonLength;
                _level.Add(0.0M);
                _trend.Add(0.0M);
            }
            _level.Add(initialLevel);
            _trend.Add(sum / _seasonLength);

            for (int i = 0; i < _seasonLength; i++)
            {
                _seasonal.Add(data[i] - initialLevel);
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
                    hwesParams.ForecastValues.Add(_level[i - 1] + _trend[i - 1] + _seasonal[seasonIndex]);
            }

            if (hwesParams.AddPrediction.Trim().ToLower() == "yes")
                hwesParams.PredictionValues = GenerateForecasts(hwesParams);
            var time = _watch.StopWatch();
            var results = new ALgoOutput
            {
                ForecastValues = hwesParams.ForecastValues,
                ActualValues = hwesParams.ActualValues,
                TotalCount = hwesParams.ActualValues.Count,
                AlgoType = modelName,
                LevelValues = _level,
                TrendValues = _trend,
                SeasonalValues = _seasonal,
                SeasonLength = _seasonLength,
                PredictionValues = hwesParams.PredictionValues,
                TimeComputed = time
            };

            return results;
        }

        public List<decimal> GenerateForecasts(HwesParams hwesParams)
        {
            List<decimal> _level = hwesParams.LevelValues;
            List<decimal> _trend = hwesParams.TrendValues;
            List<decimal> _seasonal = hwesParams.SeasonalValues;

            var horizon = hwesParams.ForecasHorizon;
            var forecasts = hwesParams.PredictionValues;
            for (int i = 1; i <= horizon; i++)
            {
                if (_seasonal.Count < _seasonLength || _level.Count == 0 || _trend.Count == 0)
                    throw new InvalidOperationException("Model must be trained before generating forecasts.");

                int seasonIndex = (_seasonal.Count - _seasonLength + (i % _seasonLength)) % _seasonLength;
                forecasts.Add(_level[^1] + i * _trend[^1] + _seasonal[seasonIndex]);
            }

            return forecasts;
        }
    }
}