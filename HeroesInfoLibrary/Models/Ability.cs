using Newtonsoft.Json;

namespace HeroesInfoLibrary.Models
{
    public class Ability
    {
        public string Id { get; set; }

        public string HeroId { get; set; }//TODO do i still need heroId here, now that ive added HeroName

        [JsonProperty(PropertyName = "heroName")]
        public string HeroName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "shortName")]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "hotkey")]
        public string Hotkey { get; set; }

        [JsonProperty(PropertyName = "abilityId")]
        public string AbilityId { get; set; }

        [JsonProperty(PropertyName = "cooldown")]
        public string Cooldown { get; set; }

        [JsonProperty(PropertyName = "manaCost")]
        public string ManaCost { get; set; }

        [JsonProperty(PropertyName = "trait")]
        public bool Trait { get; set; }
    }
}
