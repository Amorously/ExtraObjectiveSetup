using AmorLib.Dependencies;
using AmorLib.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.JSON
{
    public static class EOSJson
    {
        private static readonly JsonSerializerOptions _setting = JsonSerializerUtil.CreateDefaultSettings(true, PData_Wrapper.IsLoaded, InjectLib_Wrapper.IsLoaded);

        static EOSJson()
        {
            _setting.Converters.Add(new JsonStringEnumConverter());
            _setting.Converters.Add(new MyVector3Converter());

            //if (MTFOPartialDataUtil.IsLoaded && InjectLibUtil.IsLoaded)
            //{
            //    _setting.Converters.Add(InjectLibUtil.InjectLibConnector);
            //    _setting.Converters.Add(new LocalizedTextConverter());
            //    EOSLogger.Log("InjectLib (AWO) && PartialData support found!");
            //}
            //else
            //{
            //    if (MTFOPartialDataUtil.IsLoaded)
            //    {
            //        _setting.Converters.Add(MTFOPartialDataUtil.PersistentIDConverter);
            //        _setting.Converters.Add(WritableLocalizedTextConverter.Converter);
            //        //_setting.Converters.Add(MTFOPartialDataUtil.LocalizedTextConverter);
            //        EOSLogger.Log("PartialData support found!");
            //    }
            //    else
            //    {
            //        if (InjectLibUtil.IsLoaded)
            //        {
            //            _setting.Converters.Add(InjectLibUtil.InjectLibConnector);
            //            EOSLogger.Log("InjectLib (AWO) support found!");
            //        }
            //        _setting.Converters.Add(new LocalizedTextConverter());
            //    }
            //}
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _setting)!;
        }

        public static object Deserialize(Type type, string json)
        {
            return JsonSerializer.Deserialize(json, type, _setting)!;
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _setting);
        }
    }
}
