using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExampleBlazor.Data;

public class ChartData
{
    public readonly string Name;
    public readonly string Sup;
    public double Min;
    public double Max;
    public double Step;
    public double MinorStep;
    private readonly Action _stateHasChanged;
    private double _value = 0;

    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            _stateHasChanged.Invoke();
        }
    }

    public ChartData(string name, string sup, double min, double max, Action stateHasChanged)
    {
        Name = name;
        Sup = sup;
        Min = min;
        Max = max;
        Step = (max - min)/4;
        MinorStep = Step/10;
        _stateHasChanged = stateHasChanged;
    }


    public async Task FetchDataMean(string? selectedBucket, string? selectedDevice)
    {
        if (selectedBucket != null && !string.IsNullOrEmpty(selectedDevice))
        {
            var table = await InfluxModel.FetchDataMean(selectedBucket, selectedDevice, "7d", "environment", Name);
            if (table != null)
            {
                var value = table.Records.FirstOrDefault()!.Values.First(rec => rec.Key == "_value").Value;
                Value = Math.Round(Convert.ToDouble(value), 2);
            }
            else
            {
                Value = 0;
            }
        }
        else Value = 0;
    }
}