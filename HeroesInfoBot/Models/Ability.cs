using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroesInfoBot.Models
{
    public class Ability
    {
        public string Id { get; set; }

        public string HeroId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

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
