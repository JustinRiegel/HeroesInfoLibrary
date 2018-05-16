using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroesInfoBot.Models
{
    public class Hero
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int HeroId { get; set; }

        [JsonProperty(PropertyName = "shortName")]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "attributeId")]
        public string AttributeId { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }
}
