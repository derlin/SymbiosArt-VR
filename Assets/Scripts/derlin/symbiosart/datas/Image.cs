using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace derlin.symbiosart.datas
{
    public enum ImageState
    {
        UNKNOWN, LIKED, DISLIKED
    };


    public class Image
    {
        public Texture2D Texture { get; set; }

        public ImageMetas Metas;

        public ImageState State;

        public Image() { }

        public Image(ImageMetas metas, byte[] data)
        {
            Metas = metas;
            SetTexture(data);

        }

        public void SetTexture(byte[] rawData)
        {
            Texture = new Texture2D(0, 0);
            Texture.LoadImage(rawData);
        }
    }
}
