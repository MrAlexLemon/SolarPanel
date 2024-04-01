using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using System.Text.Json;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;

public class SolarPanelLogic : MonoBehaviour
{
    //Arduino
    SerialPort _serialPort;

    //InfluxDB
    InfluxDBClient client;
    WriteApi writeApi;

    //Solar Panel Material
    Renderer _renderer;

    //Init Text Color
    Color _defaultTextColor;

    //Init SolarPanel Color
    Color _defaultSolarPanelColor;

    //Colors for dynamic changes
    Color _warningColor;
    Color _errorColor;

    //Text for displaying Sensors Data
    public TMP_Text _environmentStats;
    public TMP_Text _solarPanelStats;
    public TMP_Text _solarPanelCoordinatesInfo;

    //Text Template for displaying Sensors Data
    string _environmentStatsTextTemplate;
    string _solarPanelStatsTextTemplate;
    string _solarPanelCoordinatesInfoTextTemplate;

    // Start is called before the first frame update
    void Start()
    {
        //Set communication with Arduino
        _serialPort = new SerialPort("COM5", 115200);
        _serialPort.Open();

        //Set InfluxDB connection
        client = new InfluxDBClient("url", "token");
        writeApi = client.GetWriteApi();

        //Save init settings
        _renderer = GetComponent<Renderer>();
        _defaultTextColor = _environmentStats.color;
        _defaultSolarPanelColor = _renderer.materials[1].color;
        _warningColor = Color.yellow;
        _errorColor = Color.red;

        //Set text templates
        _environmentStatsTextTemplate = "Env temperature: {0} C\r\nEnv humidity: {1} %\r\nLight level: {2}\r\nRaining: {3}\r\n";
        _solarPanelStatsTextTemplate = "Solar panel temperature: {0} C\r\nBus Voltage: {1} V\r\nShunt Voltage: {2} mV\r\nLoad Voltage: {3} V\r\nCurrent: {4} mA\r\nPower: {5} mW";
        _solarPanelCoordinatesInfoTextTemplate = "Coordinate X: {0}\r\nCoordinate Y: {1}\r\nCoordinate Z: {2}";
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            //Read sensors data
            string indata = _serialPort.ReadLine();
            //Debug.Log(indata);

            //Parse JSON
            ArduinoData sensorsData =
                    JsonSerializer.Deserialize<ArduinoData>(indata)!;

            //Update sensors values in text
            UpdateDisplayedSensorsText(sensorsData);

            //Check Sensors data values
            _renderer.materials[1].color = _warningColor;
            _environmentStats.color = _warningColor;

            _solarPanelStats.color = CheckSolarPanelSensorsValues(sensorsData);
            _environmentStats.color = CheckEnvironmentSensorsValues(sensorsData);

            if (_solarPanelStats.color == _errorColor || _environmentStats.color == _errorColor)
                _renderer.materials[1].color = _errorColor;
            else if (_solarPanelStats.color == _warningColor || _environmentStats.color == _warningColor)
                _renderer.materials[1].color = _warningColor;
            else
                _renderer.materials[1].color = _defaultTextColor;

            //Write Data To InfluxDB
            WriteDataToInfluxDB(sensorsData);


            //Rotate SolarPanel
            transform.rotation = new UnityEngine.Quaternion(-sensorsData.QuaternionY, -sensorsData.QuaternionZ, sensorsData.QuaternionX, sensorsData.QuaternionW);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //Update sensors values in text
    void UpdateDisplayedSensorsText(ArduinoData sensorsData)
    {
        _environmentStats.text = String.Format(_environmentStatsTextTemplate,
            sensorsData.EnvironmentTemperature, sensorsData.EnvironmentHumidity,
            sensorsData.LigtLevel, sensorsData.RainStatus);

        _solarPanelStats.text = String.Format(_solarPanelStatsTextTemplate,
            sensorsData.SolarPanelTemperature, sensorsData.BusVoltage,
            sensorsData.ShuntVoltage, sensorsData.LoadVoltage,
            sensorsData.Current, sensorsData.Power);

        _solarPanelCoordinatesInfo.text = String.Format(_solarPanelCoordinatesInfoTextTemplate,
            sensorsData.EulerX, sensorsData.EulerY, sensorsData.EulerZ);
    }

    //Write Data To InfluxDB
    void WriteDataToInfluxDB(ArduinoData sensorsData)
    {
        Task.Run(() =>
        {
            writeApi.WritePoints(new List<PointData> {
                        PointData.Measurement("Temperature")
                            .Tag("Object", "Environment")
                            .Field("Celcius", sensorsData.EnvironmentTemperature)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("Humidity")
                            .Tag("Object", "Environment")
                            .Field("Percentage", sensorsData.EnvironmentHumidity)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("Temperature")
                            .Tag("Object", "SolarPanel")
                            .Field("Celcius", sensorsData.SolarPanelTemperature)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("BusVoltage")
                            .Tag("Object", "SolarPanel")
                            .Field("V", sensorsData.BusVoltage)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("ShuntVoltage")
                            .Tag("Object", "SolarPanel")
                            .Field("mV", sensorsData.ShuntVoltage)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("LoadVoltage")
                            .Tag("Object", "SolarPanel")
                            .Field("V", sensorsData.LoadVoltage)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("Current")
                            .Tag("Object", "SolarPanel")
                            .Field("mA", sensorsData.Current)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("Power")
                            .Tag("Object", "SolarPanel")
                            .Field("mW", sensorsData.Power)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("LigtLevel")
                            .Tag("Object", "Environment")
                            .Field("Status", sensorsData.LigtLevel)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("RainStatus")
                            .Tag("Object", "Environment")
                            .Field("Status", sensorsData.RainStatus)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("QuaternionW")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.QuaternionW)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("QuaternionX")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.QuaternionX)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("QuaternionY")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.QuaternionY)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("QuaternionZ")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.QuaternionZ)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("EulerX")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.EulerX)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("EulerY")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.EulerY)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns),
                        PointData.Measurement("EulerZ")
                            .Tag("Object", "SolarPanel")
                            .Field("Coordinate", sensorsData.EulerZ)
                            .Timestamp(sensorsData.CurrentDateTime, WritePrecision.Ns)
                }, "bucket", "organization");
        }
            );
    }

    //Check Solar Panel Sensors data values
    Color CheckSolarPanelSensorsValues(ArduinoData sensorsData)
    {
        if (sensorsData.LoadVoltage > 5.0 || sensorsData.Current > 1000 ||
            sensorsData.Power > 5000 || sensorsData.SolarPanelTemperature > 35.0)
        {
            return _errorColor;
        }
        else if ((sensorsData.LoadVoltage > 4.5 && sensorsData.LoadVoltage <= 5.0)
                || (sensorsData.Current > 900 && sensorsData.Current <= 1000)
                || (sensorsData.Power > 4500 && sensorsData.Power <= 5000)
                || (sensorsData.SolarPanelTemperature > 30.0 && sensorsData.SolarPanelTemperature <= 35.0))
        {
            return _warningColor;
        }

        return _defaultTextColor;
    }

    //Check Environment Sensors data values
    Color CheckEnvironmentSensorsValues(ArduinoData sensorsData)
    {
        if (sensorsData.LigtLevel == "FEW" || sensorsData.LigtLevel == "DARK" || sensorsData.RainStatus == "Raining"
            || (sensorsData.EnvironmentTemperature > 40.0 && sensorsData.EnvironmentTemperature <= 50.0)
            || (sensorsData.EnvironmentHumidity > 55.0 && sensorsData.EnvironmentHumidity < 75.0))
        {
            return _warningColor;
        }
        else if (sensorsData.EnvironmentTemperature > 50.0 || sensorsData.EnvironmentHumidity > 75.0)
        {
            return _errorColor;
        }

        return _defaultTextColor;
    }
}
