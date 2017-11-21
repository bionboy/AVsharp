using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

using CSCore;
using CSCore.SoundIn;
using CSCore.Codecs.WAV;
using CSCore.Streams;

namespace openTK_tut {
    class Program {
        static void Main(string[] args) {
            GameWindow win = new GameWindow(500, 500);
            Game game = new Game(win);

            //SoundCapture sc = new SoundCapture();
            game.Start();
            //sc.StopAlt();
        }
    }
}
