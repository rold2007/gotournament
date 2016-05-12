namespace GoTournament.Service
{
    using GoTournament.Interface;
    using Newtonsoft.Json;

    public class JsonService : IJsonService
    {
        public T DeserializeObject<T>(string value)
        {
            value = value.Replace("\\", "\\\\");
            return JsonConvert.DeserializeObject<T>(value);
        }

        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
