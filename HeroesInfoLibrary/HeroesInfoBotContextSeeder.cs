using HeroesInfoLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HeroesInfoLibrary
{
    public class HeroesInfoBotContextSeeder
    {
        private List<HeroData> _heroDataList = new List<HeroData>();

        public void SetUpHeroDataList(HeroDataDbContext context, string jsonLocation)
        {
            string jsonStringData;

            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            foreach (var file in Directory.EnumerateFiles(jsonLocation))
            {
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        jsonStringData = sr.ReadToEnd();
                    }
                }

                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStringData);

                Hero hero = JsonConvert.DeserializeObject<Hero>(Convert.ToString(jsonObj));
                var abilityList = new List<Ability>();
                var talentList = new List<Talent>();
                var tempAbility = new Ability();
                var tempTalent = new Talent();

                //for the heroes with multiple "profiles", such as abathur and abathur's hat, we need to loop over
                //all of them so we get the entire moveset
                foreach (var abilityProfile in jsonObj["abilities"])
                {
                    //this will only have one entry in it per profile. i think...
                    foreach (var profile in abilityProfile)
                    {
                        //loop over each profile and add it to the ability list
                        foreach (var ability in profile)
                        {
                            tempAbility = JsonConvert.DeserializeObject<Ability>(Convert.ToString(ability));
                            tempAbility.ShortName = GetShortName(tempAbility);
                            tempAbility.HeroName = hero.Name;//im using the full name instead of the shortname here because it will be displayed to the user
                            abilityList.Add(tempAbility);
                        }
                    }
                }

                //im using tiers (1,2,3,...) instead of level (1,4,7,...) because its more consistent when considering
                //heroes like chromie who get talents at different levels
                var talentTierNumber = 1;

                //talents are organized by tier, so loop over each tier to get the talents from each
                foreach (var talentTier in jsonObj["talents"])
                {
                    //this will only have one entry in it per tier. i think...
                    foreach (var talents in talentTier)
                    {
                        //loop over each talent in the tier and add it to the talent list
                        foreach (var talent in talents)
                        {
                            //add the talent tier to the json data
                            talent["talentTier"] = talentTier.Name;
                            tempTalent = JsonConvert.DeserializeObject<Talent>(Convert.ToString(talent));
                            tempTalent.ShortName = GetShortName(tempTalent);
                            tempTalent.HeroName = hero.Name;//im using the full name instead of the shortname here because it will be displayed to the user
                            talentList.Add(tempTalent);
                        }
                    }
                    ++talentTierNumber;
                }

                _heroDataList.Add(new HeroData(hero, abilityList, talentList));
            }

            string heroGuid;

            foreach (var hero in _heroDataList)
            {
                heroGuid = Guid.NewGuid().ToString();
                hero.Hero.Id = heroGuid;
                context.Hero.Add(hero.Hero);

                foreach (var ability in hero.Abilities)
                {
                    ability.Id = Guid.NewGuid().ToString();
                    ability.HeroId = heroGuid;
                    context.Ability.Add(ability);
                }

                foreach (var talent in hero.Talents)
                {
                    talent.Id = Guid.NewGuid().ToString();
                    talent.HeroId = heroGuid;
                    context.Talent.Add(talent);
                }
            }

            context.SaveChanges();
        }

        private string GetShortName(Ability ability)
        {
            var shortName = ability.Name.ToLower();
            var regex = new Regex("[^a-zA-Z ]");
            shortName = regex.Replace(shortName, "");
            return shortName;
        }

        private string GetShortName(Talent talent)
        {
            var shortName = talent.Name.ToLower();
            var regex = new Regex("[^a-zA-Z ]");
            shortName = regex.Replace(shortName, "");
            return shortName;
        }
    }
}
