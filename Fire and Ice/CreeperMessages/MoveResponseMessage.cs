﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creeper;

namespace CreeperMessages
{
    public class MoveResponseMessage
    {
        public Move Move { get; set; }
        public PlayerType PlayerType { get; set; }

        public MoveResponseMessage(Move move, PlayerType playerType)
        {
            Move = move;
            PlayerType = playerType;
        }
    }
}
