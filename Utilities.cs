using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkapi
{
    /// <summary>
    /// Обьект картинки для загрузки
    /// </summary>
    public class ImgFile
    {
        /// <summary>
        /// Название файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Полный путь до файла
        /// </summary>
        public string FileFullName { get; set; }

        /// <summary>
        /// Размер файла в байтах
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Длинна изображения
        /// </summary>
        public int ImgWidth { get; set; }

        /// <summary>
        /// Высота изображения
        /// </summary>
        public int ImgHeight { get; set; }

        public ImgFile(string fileFullName)
        {
            this.FileFullName = fileFullName;
            GetFileInfo();
        }

        private void GetFileInfo()
        {
            FileInfo file = new FileInfo(this.FileFullName);
            this.FileSize = file.Length / 1024;
            this.FileName = file.Name;
            Debug.WriteLine(this.FileSize.ToString() + " " + this.FileName);
        }

    }
}
