using System.Text.Json.Serialization;

namespace PuntosPopularWeb.Gateway.DTOs
{
    public class BpOutReqDTO
    {
        [JsonPropertyName("codigoError")]
        public string CodigoError { get; set; } = string.Empty;

        [JsonPropertyName("mensajeError")]
        public string MensajeError { get; set; } = string.Empty;

        [JsonPropertyName("fechaHora")]
        public DateTime FechaHora { get; set; }
    }
}