using symbiosart.datas;
using System;
using System.Collections.Generic;
using System.IO;


namespace symbiosart.utils
{
    public class UserManager
    {
        public User User { get; set; }

        public UserManager(User user)
        {
            // TODO
            if (user == null) user = new User();
            User = user;
        }

        // ================================== instance methods

        public void Cache(Image image)
        {
            FileUtils.SaveTextToFile(image.metas.ToJson(), image.metas.MetaFile(User.CachePath));
            FileUtils.SaveTextureToFile(image.Texture, image.metas.ImageFile(User.CachePath));
        }

        public void Uncache(Image image) { Uncache(image.metas); }
        public void Uncache(ImageMetas metas)
        {
            File.Delete(metas.MetaFile(User.CachePath));
            File.Delete(metas.ImageFile(User.CachePath));
        }


        public Image GetCached(ImageMetas metas)
        {
            Image image = new Image();
            image.metas = metas;
            image.Texture = FileUtils.loadTextureFromDisc(metas.ImageFile(User.CachePath));
            return image;
        }


        public void Like(Image image) { Like(image.metas); }
        public void Like(ImageMetas metas)
        {
            User.MarkAsLiked(metas);
            moveTo(metas, User.LikedPath);
        }


        public void Dislike(Image image) { Dislike(image.metas); }
        public void Dislike(ImageMetas metas)
        {
            User.MarkAsDisliked(metas);
            moveTo(metas, User.DislikedPath);
        }

        private void moveTo(ImageMetas metas, string to)
        {
            File.Move(metas.ImageFile(User.CachePath), metas.ImageFile(to));
            File.Move(metas.MetaFile(User.CachePath), metas.MetaFile(to));
        }


    }
}
