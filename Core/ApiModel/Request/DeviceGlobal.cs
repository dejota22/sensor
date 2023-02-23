﻿using Newtonsoft.Json;

namespace Core.ApiModel.Request
{
    public class DeviceGlobal
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("seq")]
        public int Seq { get; set; }

        [JsonProperty("leituras_efetuadas")]
        public int LeiturasEfetuadas { get; set; }

        [JsonProperty("q_cards_enviados")]
        public int QCardsEnviados { get; set; }

        [JsonProperty("q_totais_enviados")]
        public int QTotaisEnviados { get; set; }

        [JsonProperty("alarme")]
        public int Alarme { get; set; }

        [JsonProperty("rms_acc_x")]
        public double RmsAccX { get; set; }

        [JsonProperty("rms_acc_y")]
        public double RmsAccY { get; set; }

        [JsonProperty("rms_acc_z")]
        public double RmsAccZ { get; set; }

        [JsonProperty("rms_spd_x")]
        public double RmsSpdX { get; set; }

        [JsonProperty("rms_spd_y")]
        public double RmsSpdY { get; set; }

        [JsonProperty("rms_spd_z")]
        public double RmsSpdZ { get; set; }

        [JsonProperty("ftr_crista_x")]
        public double FtrCristaX { get; set; }

        [JsonProperty("ftr_crista_y")]
        public double FtrCristaY { get; set; }

        [JsonProperty("ftr_crista_z")]
        public double FtrCristaZ { get; set; }
    }
}