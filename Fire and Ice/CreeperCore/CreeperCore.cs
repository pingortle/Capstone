﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAControlGame;
using Creeper;

namespace CreeperCore
{
    public enum OpponentType { Human, AI, Network }

    public class CreeperCore
    {
        //Only the core should be able to change these properties
        //  They should always stay as private set
        public CreeperBoard Board { get; private set; }
        public GameState GameState { get; private set; }
        public GameState PreviousGameState { get; private set; }
        public OpponentType OpponentType { get; private set; }

        protected Game1 _xnaGame;

        public CreeperCore(Game1 xnaGame)
        {
            Board = new CreeperBoard();
            GameState = GameState.SplashScreen;

            _xnaGame = xnaGame;
            _xnaGame.Board = this.Board;
        }

        public void Run()
        {

        }

        public void StartGame()
        {
            PreviousGameState = GameState;
            GameState = GameState.InGame;

            _xnaGame.StateChange(PreviousGameState, GameState);
        }


    }
}