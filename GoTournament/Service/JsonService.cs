using GoTournament.Interface;
using Newtonsoft.Json;

namespace GoTournament.Service
{
    public class JsonService : IJsonService
    {
        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
