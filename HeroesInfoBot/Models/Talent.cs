using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroesInfoBot.Models
{
    public class Talent
    {
        public string Id { get; set; }

        public string HeroId { get; set; }

        [JsonProperty(PropertyName = "tooltipId")]
        public string TooltipId { get; set; }

        [JsonProperty(PropertyName = "talentTreeId")]
        public string TalentTreeId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "sort")]
        public int Sort { get; set; }

        [JsonProperty(PropertyName = "abilityId")]
        public string AbilityId { get; set; }

        [JsonProperty(PropertyName = "talentTier")]
        public string TalentTier { get; set; }
    }
}
