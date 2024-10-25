namespace MqttLibrary
{
    public class MqttConfig
    {
        public string URL { get; set; }
        public int PORT { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_url"></param>
        /// <param name="_port"></param>
        public MqttConfig(string _url = "broker.emqx.io", int _port = 1883)
        {
            this.URL = _url;
            this.PORT = _port;
        }
    }
}

