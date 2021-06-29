using Newtonsoft.Json;

namespace HeroesInfoBot.Models
{
    public class Talent
    {
        public string Id { get; set; }

        public string HeroId { get; set; }//TODO do i still need heroId here, now that ive added HeroName

        [JsonProperty(PropertyName = "heroName")]
        public string HeroName { get; set; }

        [JsonProperty(PropertyName = "tooltipId")]
        public string TooltipId { get; set; }

        [JsonProperty(PropertyName = "talentTreeId")]
        public string TalentTreeId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "shortName")]
        public string ShortName { get; set; }

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
