﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperCore
{
    public enum PlayerType { AI, Human, Network }

    public class Player
    {
        public CreeperColor Color { get; private set; }
        public PlayerType PlayerType { get; private set; }

        public Player(PlayerType playerType, CreeperColor creeperColor)
        {
            PlayerType = playerType;
            Color = creeperColor;
        }
    }
}