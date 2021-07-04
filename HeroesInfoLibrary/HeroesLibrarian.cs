using HeroesInfoLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeroesInfoLibrary
{
    public class HeroesLibrarian
    {
        #region Member variables 
        private List<string> AbilityKeys = new List<string>() { "q", "w", "e", "r", "d", "1", "2", "3", "4", "5", "6" };
        private List<string> Level10TalentTierExceptions = new List<string>() { "deathwing", "tracer", "varian" };

        private HeroDataDbContext _dbContext;
        private List<HeroData> _heroDataList = new List<HeroData>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for the HeroesLibrarian, able to fetch all the Heroes data you need from the library.
        /// </summary>
        /// <param name="jsonLocation">File path to the folder with the Hero JSON data. Relative paths should work fine.</param>
        /// <param name="dbLocation">File path to where the database file will be created/reference. Relative paths should work fine.</param>
        public HeroesLibrarian(string jsonLocation, string dbLocation)
        {
            _dbContext = new HeroDataDbContext(dbLocation);
            SetUpHeroDataList(jsonLocation);
        }

        #endregion

        #region Database creation

        private void SetUpHeroDataList(string jsonLocation)
        {
            string jsonStringData;

            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();

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
                _dbContext.Hero.Add(hero.Hero);

                foreach (var ability in hero.Abilities)
                {
                    ability.Id = Guid.NewGuid().ToString();
                    ability.HeroId = heroGuid;
                    _dbContext.Ability.Add(ability);
                }

                foreach (var talent in hero.Talents)
                {
                    talent.Id = Guid.NewGuid().ToString();
                    talent.HeroId = heroGuid;
                    _dbContext.Talent.Add(talent);
                }
            }

            _dbContext.SaveChanges();
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

        #endregion

        #region Input processing 

        /// <summary>
        /// The method to call to get Ability and Talent data. Supported forms of input are (case insensitive):
        /// [[Ability or Talent name]], i.e. [[Once Again the First Time]]
        /// [[Hero name/Talent name]], i.e. [[Chromie/Andorhal Anomaly]]
        /// [[Hero name/Ability name]], i.e. [[Chromie/Sand Blast]]
        /// [[Hero name/Ability hotkey]], i.e [[Chromie/Q]], works for Q,W,E,R,D,Trait,1,2,3,4,5,6
        /// </summary>
        /// <param name="input">The input string used to fetch the appropriate data. Despite the examples in the method summary, the [[ ]] should be stripped out before passing it in.</param>
        /// <returns>Returns a List of strings, each entry of which is already formatted for display to the user.</returns>
        public List<string> GetAbilityAndTalentDataByString(string input)
        {
            //var jsonLocation = ConfigurationManager.AppSettings.Get("jsonDataLocation");
            //var dbLocation = ConfigurationManager.AppSettings.Get("databaseLocation");


            //CASE 1 (DONE) - talents and abilities by name [[Pyroblast]], [[Possession]]
            //CASE 2 (DONE) - all talents by hero's tier [[Lunara/4]], [[Butcher/10]]. because all talent tier levels are the same, extra logic will be needed to account for chromie
            //CASE 3 (DONE) - abilities by hero and slot [[Varian/W]], [[Murky/Trait]]
            //CASE 4 (DONE) - talents and abilities by hero and name [[Dehaka/Drag]], [[Chromie/Time Trap]]

            //possible TODOs:
            //parital names, such as [[kael/flames]], [[morales]], [[hammer]]
            //      this is a maybe, but probably not - prioritize better matches, i.e. [[Block]] should show Block before Ice Block
            //      (probably through wildcard only at the end, Block%, then at both ends, %Block%)
            //      im thinking this is a no, because there are better ways to implement partial match results
            //      exclude preceeding "the"s, punctuation, numbers
            //      NOTE: the shortname for Butcher is "thebutcher" but the shortname for TLV is "lostvikings". one has the "the", one does not, so maybe a query with each?
            //a query to get all talents that affect a specific hotkey/ability?

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

                //clean up input a bit to account for some name weirdness in the database.
                //TODO: this is honestly just a sto-gap, because i'd really like to have a column in the hero table that is a semicolon-delimited list of "similar names".
                //then i wouldn't need to do these one-off checks, just check the list of not-quite-names to get the actual ShortName of the Hero.
                //it could even be parsed in from a file so that anyone using this library can easily add other matches
                if (inputPartOne == "butcher")
                {
                    //the user put in "butcher" but its called The Butcher in the database
                    inputPartOne = "thebutcher";
                }
                else if ((inputPartOne.Contains("lost") && inputPartOne.Contains("vikings")) || inputPartOne.Contains("tlv"))
                {
                    inputPartOne = "lostvikings";
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

        #endregion

        #region Database queries

        private List<CleanResultData> GetByAbilityOrTalentName(string name)
        {
            var returnList = new List<CleanResultData>();
            var abilityList = (from a in _dbContext.Ability
                               where a.ShortName.ToLower().Contains(name)
                               select a
                         ).ToList();
            var talentList = (from t in _dbContext.Talent
                              where t.ShortName.ToLower().Contains(name)
                              select t
                         ).ToList();
            abilityList.ForEach((a) => { returnList.Add(GetCleanResultData(a)); });
            //rule out the duplicate talents we already got from abilities, most commonly heroic (R) abilities, based on AbilityId
            talentList = talentList.Where(tl => !abilityList.Exists(a => a.AbilityId == tl.AbilityId)).ToList();
            talentList.ForEach((t) => { returnList.Add(GetCleanResultData(t)); });

            return returnList;
        }

        private List<CleanResultData> GetByHeroNameAndTalentTier(string heroName, string talentTier)
        {
            //need to check for heroic talent tier and if it is, return the heroic abilities instead.
            //exceptions: varian gets his at 4, tracer and deathwing both have theirs at the start and should return talents like normal
            //chromie/8 and chromie/10 should both return the R-hotkey abilities
            //chromie/10 is covered by the third check, but chromie/8 should be valid too, thus special case
            if ((heroName == "varian" && talentTier == "4") ||
                (heroName == "chromie" && talentTier == "8") ||
                (!Level10TalentTierExceptions.Contains(heroName) && talentTier == "10"))
            {
                //because varian gets his R ability from his level 4 talent, it can be assumed the user is looking for the ability information, not the talent tier information
                return GetByHeroNameAndAbilityHotkey(heroName, "r");
            }
            var returnList = new List<CleanResultData>();
            //in case someone uses chromie's actually talent tier levels, they need to be adjusted to match everyone else's, because that's what's in the database
            if (heroName == "chromie")
            {
                switch (talentTier)
                {
                    case "2":
                        talentTier = "4";
                        break;
                    case "5":
                        talentTier = "7";
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
            var talentList = (from t in _dbContext.Talent
                              join h in _dbContext.Hero
                              on t.HeroId equals h.Id
                              where h.ShortName.ToLower().Contains(heroName) && t.TalentTier.ToLower() == talentTier
                              select t
                         ).ToList();
            talentList.ForEach((t) => { returnList.Add(GetCleanResultData(t)); });

            return returnList;
        }

        private List<CleanResultData> GetByHeroNameAndAbilityHotkey(string heroName, string hotkey)
        {
            var returnList = new List<CleanResultData>();
            if (hotkey == "trait")
            {
                hotkey = "d";
            }
            var abId = $"|{hotkey}";
            var abilityList = (from a in _dbContext.Ability
                               join h in _dbContext.Hero
                               on a.HeroId equals h.Id
                               where h.ShortName.ToLower().Contains(heroName) && a.AbilityId.ToLower().Contains($"|{hotkey}")//cannot use Hotkey, need to use AbilityId, because passive traits don't have a hotkey
                               select a
                         ).ToList();
            abilityList.ForEach((a) => { returnList.Add(GetCleanResultData(a)); });
            return returnList;
        }

        private List<CleanResultData> GetByHeroNameAndAbilityName(string heroName, string abilityName)
        {
            var returnList = new List<CleanResultData>();
            var abilityList = (from a in _dbContext.Ability
                               join h in _dbContext.Hero
                               on a.HeroId equals h.Id
                               where h.ShortName.ToLower().Contains(heroName) && a.ShortName.ToLower().Contains(abilityName)
                               select a
                         ).ToList();
            abilityList.ForEach((a) => { returnList.Add(GetCleanResultData(a)); });
            return returnList;
        }

        #endregion

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

    /// <summary>
    /// CleanResultData is a semi-abstracted class capable of holding pertinent data from both Abilities and Talents to be displayed to the user through the ToString() override.
    /// It is meant to be able to hold data from both Abilities and Talents, but each of those has fields the other doesn't that need to be displayed.
    /// </summary>
    public class CleanResultData
    {
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
            //chromie out here being special again
            if (heroName != "Chromie" || string.IsNullOrEmpty(tier))
            {
                TalentTier = tier;
            }
            else
            {
                //tier should always be a number by this point
                var chromieIsASpecialCase = Int32.Parse(tier) - 2;
                //set the tier to 1 if it's below that after subtracting 2 above (accounting for level 1 talents)
                TalentTier = chromieIsASpecialCase > 1 ? chromieIsASpecialCase.ToString() : "1";
            }
            Description = description;
            Cooldown = cooldown;
            ManaCost = mana;
        }

        public override string ToString()
        {
            //TODO add specific statement denoting Talent or Ability?
            if (string.IsNullOrEmpty(TalentTier))//talent tier is empty, so its an ability
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
