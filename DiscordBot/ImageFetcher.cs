﻿namespace Gideon
{
    class ImageFetcher
    {
        public string GetRandomPic() => Alani[Config.Utilities.GetRandomNumber(0, Alani.Length)];

        public string[] Alani = {
                "https://i.imgur.com/pRsqdMv.jpg",
                "https://i.imgur.com/1H0aGG4.jpg",
                "https://i.imgur.com/TbcHPJU.jpg",
                "https://i.imgur.com/cqyvwXL.jpg",
                "https://i.imgur.com/PhV3h8N.jpg",
                "https://i.imgur.com/MAp3ONz.jpg",
                "https://i.imgur.com/Q4fGT5cg.jpg",
                "https://i.imgur.com/fmjkcNc.jpg",
                "https://i.imgur.com/NSwMvXQ.jpg",
                "https://i.imgur.com/4lQ7LGG.jpg",
                "https://i.imgur.com/Nqwo43U.jpg"
                };
    }
}
