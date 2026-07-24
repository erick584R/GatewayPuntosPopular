using System.Text.Json.Serialization;

namespace PuntosPopularWeb.Gateway.DTOs
{
    public class BpInReqDTO
    {
        [JsonPropertyName("canal")]
        public int Canal { get; set; }

        [JsonPropertyName("dispositivoFisico")]
        public string DispositivoFisico { get; set; } = string.Empty;

        [JsonPropertyName("ipDispositivo")]
        public string IpDispositivo { get; set; } = string.Empty;

        [JsonPropertyName("ctnro")]
        public string Ctnro { get; set; } = string.Empty;

        [JsonPropertyName("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}