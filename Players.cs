using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace buildBot
{
    internal class Players
    {
        public long userId;
        public string charName;
        public string playerName;
        public int photoCount;
        public Byte[] photo1;
        public Byte[] photo2;
        public Byte[] photo3;
        public Byte[] photo4;
        public Byte[] photo5;
        public Byte[] photo6;
        public Byte[] photo7;
        public bool canSendPhotos;

        public Players(long _userid,int _photoCount,bool canSend)
        {
            userId = _userid;
            photoCount = _photoCount;
            canSendPhotos = canSend;
        }
    }
}
