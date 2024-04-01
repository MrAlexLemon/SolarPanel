using System;
using System.Text.Json.Serialization;

public class ArduinoData
{
    [JsonPropertyName("ET")]
    public float EnvironmentTemperature { get; set; }
    [JsonPropertyName("EH")]
    public float EnvironmentHumidity { get; set; }
    [JsonPropertyName("SPT")]
    public float SolarPanelTemperature { get; set; }
    [JsonPropertyName("BV")]
    public float BusVoltage { get; set; }
    [JsonPropertyName("SV")]
    public float ShuntVoltage { get; set; }
    [JsonPropertyName("LV")]
    public float LoadVoltage { get; set; }
    [JsonPropertyName("C")]
    public float Current { get; set; }
    [JsonPropertyName("P")]
    public float Power { get; set; }
    [JsonPropertyName("L")]
    public string LigtLevel { get; set; }
    [JsonPropertyName("R")]
    public string RainStatus { get; set; }
    [JsonPropertyName("QW")]
    public float QuaternionW { get; set; }
    [JsonPropertyName("QX")]
    public float QuaternionX { get; set; }
    [JsonPropertyName("QY")]
    public float QuaternionY { get; set; }
    [JsonPropertyName("QZ")]
    public float QuaternionZ { get; set; }
    [JsonPropertyName("EX")]
    public float EulerX { get; set; }
    [JsonPropertyName("EY")]
    public float EulerY { get; set; }
    [JsonPropertyName("EZ")]
    public float EulerZ { get; set; }
    public DateTime CurrentDateTime { get; set; } = DateTime.Now;
}