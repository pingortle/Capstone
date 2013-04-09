﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Caliburn.Micro;
using CreeperMessages;
using System.Media;

namespace CreeperSound
{
    
    public class SoundEngine : IHandle<SoundPlayMessage>
    {
        public SoundEngine(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }


        public void Handle(SoundPlayMessage message)
        {
            String path = Path.GetFullPath("..\\..\\..\\CreeperSound\\SoundAssets");
            String soundFile = "\\";
            SoundPlayer player;

            switch (message.Type)
            {
                case SoundPlayType.Default:
                    soundFile += "default.wav";
                    break;
                case SoundPlayType.MenuSlideOut:
                    soundFile += "MenuSlideOut.wav";
                    break;
                case SoundPlayType.MenuButtonMouseOver:
                    soundFile += "MenuButtonMouseOver.wav";
                    break;
                case SoundPlayType.MenuButtonClick:
                    soundFile += "MenuButtonClick.wav";
                    break;
            }

            player = new SoundPlayer(path + soundFile);
            player.Play();
        }
    }
}