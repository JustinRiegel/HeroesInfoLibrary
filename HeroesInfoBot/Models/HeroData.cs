﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroesInfoBot.Models
{
    public class HeroData
    {
        public HeroData(Hero hero, List<Ability> abilities, List<Talent> talents)
        {
            Hero = hero;
            Abilities = abilities;
            Talents = talents;
        }

        public Hero Hero { get; }
        public List<Ability> Abilities { get; }
        public List<Talent> Talents { get; }
    }
}
