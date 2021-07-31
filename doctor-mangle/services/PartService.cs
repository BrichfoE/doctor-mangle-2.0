using doctor_mangle.constants;
using doctor_mangle.interfaces;
using doctor_mangle.models.parts;
using Microsoft.Extensions.Logging;

namespace doctor_mangle.services
{
    public class PartService : IPartService
    {
        private readonly ILogger<IPartService> _logger;
        public PartService(ILogger<IPartService> logger)
        {
            this._logger = logger;
        }
        public string GetPartDetails(BodyPart part)
        {
            if (part == null)
            {
                _logger.LogWarning("Cannot print details when null argument is provided");
                return string.Empty;
            }
            _logger.LogInformation($"Print details for {part.PartName}");
            var result = $"{part.PartName}" +
                $"\r\nDurability { part.PartDurability}" +
                $"\r\nAlacrity:  {part.PartStats[Stat.Alacrity]}" +
                $"\r\nStrenght:  {part.PartStats[Stat.Strength]}" +
                $"\r\nEndurance: {part.PartStats[Stat.Endurance]}" +
                $"\r\nTechnique: {part.PartStats[Stat.Technique]}";
            return result;
        }
    }
}
