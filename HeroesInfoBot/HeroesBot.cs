using HeroesInfoBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeroesInfoBot
{
    public class HeroesBot
    {
        private const string GetAbilityByNameQueryString = "SELECT * FROM Ability WHERE ShortName LIKE '%{0}%'";
        private const string GetTalentByNameQueryString = "SELECT * FROM Talent WHERE ShortName LIKE '%{0}%'";
        private const string GetTalentTierByHeroNameQueryString =
            @"SELECT t.* FROM Talent t
                INNER JOIN Hero h
                ON h.Id = t.HeroId
                WHERE h.ShortName LIKE '%{0}%' AND t.TalentTier = {1}";
        private const string GetAbilityByHeroNameAndHotkeyQueryString =
            @"SELECT a.* FROM Ability a
                INNER JOIN Hero h
                ON h.Id = a.HeroId
                WHERE h.ShortName LIKE '%{0}%' AND a.AbilityId LIKE '%|{1}%'";//cannot use hotkey, need to use AbilityId, because passive traits don't have a hotkey
        private const string GetAbilityByHeroNameAndAbilityNameQueryString =
            @"SELECT a.* FROM Ability a
                INNER JOIN Hero h
                ON h.Id = a.HeroId
                WHERE h.ShortName LIKE '%{0}%' AND a.ShortName LIKE '%{1}%'";
        private List<string> AbilityKeys = new List<string>() { "q", "w", "e", "r", "d", "1", "2", "3", "4", "5", "6" };
        private List<string> Level10TalentTierExceptions = new List<string>() { "deathwing", "tracer", "varian" };

        private HeroDataDbContext _dbContext;
        private HeroesInfoBotContextSeeder _contextSeeder;

        /// <summary>
        /// Constructor for the now-poorly-named HeroesInfoBot.
        /// </summary>
        /// <param name="jsonLocation">File path to the folder with the Hero JSON data. Relative paths should work fine.</param>
        /// <param name="dbLocation">File path to where the database file will be created/reference. Relative paths should work fine.</param>
        public HeroesBot(string jsonLocation, string dbLocation)
        {
            _dbContext = new HeroDataDbContext(dbLocation);
            _contextSeeder = new HeroesInfoBotContextSeeder();
            _contextSeeder.SetUpHeroDataList(_dbContext, jsonLocation);
        }

        /// <summary>
        /// The method to call to get Ability and Talent data. Supported forms of input are (case insensitive):
        /// [[Ability or Talent name]], i.e. [[Once Again the First Time]]
        /// [[Hero name/Talent name]], i.e. [[Chromie/Andorhal Anomaly]]
        /// [[Hero name/Ability name]], i.e. [[Chromie/Sand Blast]]
        /// [[Hero name/Ability hotkey]], i.e [[Chromie/Q]], works for Q,W,E,R,D,Trait,1,2,3,4,5,6
        /// </summary>
        /// <param name="input">The input string used to fetch the appropriate data. Despite the examples I gave in the method summary, the [[ ]] should be stripped out before passing it in.</param>
        /// <returns>Returns a List of strings, each entry of which is already formatted for display to the user.</returns>
        public List<string> GetAbilityAndTalentDataByString(string input)
        {
            //var jsonLocation = ConfigurationManager.AppSettings.Get("jsonDataLocation");
            //var dbLocation = ConfigurationManager.AppSettings.Get("databaseLocation");

            
            //CASE 1 (DONE) - talents and abilities by name [[Pyroblast]], [[Possession]]
            //CASE 2 (DONE) - all talents by hero's tier [[Lunara/4]], [[Butcher/10]]. because all talent tier levels are the same, extra logic will be needed to account for chromie
            //CASE 3 (DONE) - abilities by hero and slot [[Varian/W]], [[Murky/Trait]]
            //CASE 4 (DONE) - talents and abilities by hero and name [[Dehaka/Drag]], [[Chromie/Time Trap]]
            //parital names, such as [[kael/flames]], [[morales]], [[hammer]]
            //this is a maybe, but probably not - prioritize better matches, i.e. [[Block]] should show Block before Ice Block
            //      (probably through wildcard only at the end, Block%, then at both ends, %Block%)
            //      im thinking this is a no, because there are better ways to implement partial match results
            //exclude preceeding "the"s, punctuation, numbers
            //NOTE: the shortname for Butcher is "thebutcher" but the shortname for TLV is "lostvikings". one has the "the", one does not, so maybe a query with each?
            
            var results = ProcessUserInput(input.ToLower());
            var returnList = new List<string>();
            results.ForEach(r => returnList.Add(r.ToString()));
            return returnList;
        }

        private List<CleanResultData> ProcessUserInput(string input)
        {
            var resultList = new List<CleanResultData>();
            //does the input contain the separator character
            if (input.Contains('/'))
            {
                var inputPartOne = input.Split('/')[0];
                var inputPartTwo = input.Split('/')[1];

                if (inputPartOne == "butcher")
                {
                    //the user put in "butcher" but its called The Butcher in the database
                    inputPartOne = "thebutcher";
                }

                //if the second part is numeric or is an ability key, we want to use both those queries, as there is overlap between number buttons and talent tiers
                if (Int32.TryParse(inputPartTwo, out int talentTier) || AbilityKeys.Contains(inputPartTwo))
                {
                    if (inputPartTwo == "trait")//if the input was "trait", convert it to the key so it fits the rest of the code
                    {
                        inputPartTwo = "d";
                    }
                    
                    resultList = GetByHeroNameAndTalentTier(inputPartOne, inputPartTwo);
                    resultList.AddRange(GetByHeroNameAndAbilityHotkey(inputPartOne, inputPartTwo));
                }
                else
                {
                    //second part was not a talent tier or an ability key, so search by ability name
                    //TODO maybe also make it search by talent name in case someone wants to be really clever? or do i say that's covered by the first case?
                    resultList = GetByHeroNameAndAbilityName(inputPartOne, inputPartTwo);

                }
            }
            else //doesn't contain the separator, so just looking for an ability or talent name
            {
                resultList = GetByAbilityOrTalentName(input);
            }

            return resultList;
        }

        private List<CleanResultData> GetByAbilityOrTalentName(string name)
        {
            var returnList = new List<CleanResultData>();
            var abilityList = _dbContext.Ability.FromSql(string.Format(GetAbilityByNameQueryString, name)).ToList();
            var talentList = _dbContext.Talent.FromSql(string.Format(GetTalentByNameQueryString, name)).ToList();
            abilityList.ForEach((a) => { returnList.Add(GetCleanResultData(a)); });
            talentList = talentList.Where(tl => !abilityList.Exists(a => a.AbilityId == tl.AbilityId)).ToList();
            talentList.ForEach((t) => { returnList.Add(GetCleanResultData(t)); });

            return returnList;
        }

        private List<CleanResultData> GetByHeroNameAndTalentTier(string heroName, string talentTier)
        {
            //need to check for heroic talent tier and if it is, return the heroic abilities instead.
            //exceptions: varian gets his at 4, tracer and deathwing both have theirs at the start and should return talents like normal
            if((heroName == "varian" && talentTier == "4") || (!Level10TalentTierExceptions.Contains(heroName) && talentTier == "10"))
            {
                //because varian gets his R ability from his level 4 talent, it can be assumed the user is looking for the ability information, not the talent tier information
                return GetByHeroNameAndAbilityHotkey(heroName, "r");
            }
            var returnList = new List<CleanResultData>();
            //in case someone uses chromie's actually talent tier levels, they need to be adjusted to match everyone else's, because that's what's in the database
            if(heroName == "chromie")
            {
                switch(talentTier)
                {
                    case "2": talentTier = "4";
                        break;
                    case "5":
                        talentTier = "7";
                        break;
                    case "8":
                        talentTier = "10";
                        break;
                    case "11":
                        talentTier = "13";
                        break;
                    case "14":
                        talentTier = "16";
                        break;
                    case "18":
                        talentTier = "20";
                        break;
                }
            }
            var talentList = _dbContext.Talent.FromSql(string.Format(GetTalentTierByHeroNameQueryString, heroName, talentTier)).ToList();
            talentList.ForEach((t) => { returnList.Add(GetCleanResultData(t)); });

            return returnList;
        }

        private List<CleanResultData> GetByHeroNameAndAbilityHotkey(string heroName, string hotkey)
        {
            var returnList = new List<CleanResultData>();
            if(hotkey == "trait")
            {
                hotkey = "d";
            }
            var abilityList = _dbContext.Ability.FromSql(string.Format(GetAbilityByHeroNameAndHotkeyQueryString, heroName, hotkey)).ToList();
            abilityList.ForEach((a) => { returnList.Add(GetCleanResultData(a)); });
            return returnList;
        }

        private List<CleanResultData> GetByHeroNameAndAbilityName(string heroName, string abilityName)
        {
            var returnList = new List<CleanResultData>();
            var abilityList = _dbContext.Ability.FromSql(string.Format(GetAbilityByHeroNameAndAbilityNameQueryString, heroName, abilityName)).ToList();
            abilityList.ForEach((a) => { returnList.Add(GetCleanResultData(a)); });
            return returnList;
        }

        private CleanResultData GetCleanResultData(Ability ability)
        {
            return new CleanResultData(
                ability.HeroName,
                ability.Name,
                ability.AbilityId,
                string.Empty,//empty string for talent tier, as abilities don't have those
                ability.Description,
                ability.Cooldown,
                ability.ManaCost
                );
        }

        private CleanResultData GetCleanResultData(Talent talent)
        {
            return new CleanResultData(
                talent.HeroName,
                talent.Name,
                talent.AbilityId,
                talent.TalentTier,
                talent.Description,
                string.Empty,//empty string for cooldown
                string.Empty//empty string for mana cost
                );
        }
    }

    class CleanResultData
    {
        //name
        //hotkey (including active or passive or null for talents)
        //what talent tier it is, if its a talent
        //description
        //cooldown
        //mana cost
        public string HeroName { get; private set; }
        public string Name { get; private set; }
        public string Hotkey { get; private set; }
        public string TalentTier { get; private set; }
        public string Description { get; private set; }
        public string Cooldown { get; private set; }
        public string ManaCost { get; private set; }

        public CleanResultData(string heroName, string name, string abilityId, string tier, string description, string cooldown, string mana)
        {
            //the abilityId takes 1 of 2 forms: Chromie|Q1 [hotkey, then sort order], or Chromie|Passive
            //in the first case, the first character after the pipe is the actual hotkey
            //in the second, it is either "Active" or "Passive"
            //because of thise, we can do a length check to parse out the data we're actually looking for
            //this holds true for both Talents and Abilities
            var hotkey = string.Empty;
            if (!string.IsNullOrEmpty(abilityId))
            {
                hotkey = abilityId.Split('|')[1];
                if (hotkey.Length == 2)
                {
                    hotkey = hotkey.Substring(0, 1);
                }
            }
            HeroName = heroName;
            Name = name;//ability or talent name
            Hotkey = hotkey;
            TalentTier = tier;
            Description = description;
            Cooldown = cooldown;
            ManaCost = mana;
        }

        public override string ToString()
        {
            if(string.IsNullOrEmpty(TalentTier))//talent tier is empty, so its an ability
            {
                return $"{Name} ({HeroName}) - " + (string.IsNullOrEmpty(Hotkey) ? string.Empty : $"[{Hotkey}]{Environment.NewLine}") +
                    (string.IsNullOrEmpty(ManaCost) ? string.Empty : $"Ability cost: {ManaCost}{Environment.NewLine}") +
                    (string.IsNullOrEmpty(Cooldown) ? string.Empty : $"Cooldown: {Cooldown}{Environment.NewLine}") +
                    $"Description: {Description}{Environment.NewLine}";

            }
            else
            {
                return $"{Name} ({HeroName}) - " + (Hotkey == "Active" ? "[Active] " : string.Empty) + $"Level {TalentTier}{Environment.NewLine}" +
                    $"Description: {Description}{Environment.NewLine}";
            }
        }
    }
}
