using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Backgammon.Client.Models
{
    public sealed class AvatarOption
    {

        public int Id { get; }

        public ImageSource Image { get; }

        public AvatarOption(int id, byte[] imageData)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            Id = id;
            Image = CreateImage(imageData);
        }

        private BitmapImage CreateImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException(
                    "Avatar image data cannot be empty.",
                    nameof(imageData));
            }

            using (MemoryStream imageStream = new MemoryStream(imageData))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = imageStream;
                image.EndInit();
                image.Freeze();

                return image;
            }
        }

    }
}
