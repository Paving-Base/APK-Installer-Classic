using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using Windows.Storage;
using System.IO;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;

namespace APKInstaller.Helpers
{
    /// <summary>
    /// Class providing functionality to support generating and copying protocol activation URIs.
    /// </summary>
    public static class ClipboardHelper
    {
        /// <summary>
        /// 复制到剪切板
        /// </summary>
        /// <param name="text">文字</param> 
        public static void CopyText(string text)
        {
            Clipboard.SetText(text);
        }

        /// <summary>
        /// 复制到剪切板
        /// </summary>
        /// <param name="image">图片</param> 
        public static void CopyBitmap(BitmapSource image)
        {
            Clipboard.SetImage(image);
        }

        /// <summary>
        /// 复制或剪切文件到剪切板
        /// </summary>
        /// <param name="filePath">文件路径数组</param>
        /// <remarks>清空剪切板</remarks>
        public static void SetFileDrop(string filePath)
        {
            if (filePath == null) return;
            SetFileDropList(new[] { filePath });
        }
        /// <summary>
        /// 复制或剪切文件到剪切板
        /// </summary>
        /// <param name="files">文件路径数组</param>
        /// <remarks>清空剪切板</remarks>
        public static void SetFileDropList(string[] files)
        {
            Clipboard.Clear();//清空剪切板 
            StringCollection strcoll = new StringCollection();
            foreach (var file in files)
            {
                strcoll.Add(file);
            }
            Clipboard.SetFileDropList(strcoll);
        }

        /// <summary>
        /// 复制或剪切文件到剪切板
        /// </summary>
        /// <param name="filePath">文件路径数组</param>
        /// <param name="cut">true:剪切；false:复制</param>
        public static void CopyFile(string filePath, bool cut = false)
        {
            if (filePath == null) return;
            CopyFileList(new[] { filePath }, cut);
        }
        /// <summary>
        /// 复制或剪切文件到剪切板
        /// </summary>
        /// <param name="files">文件路径数组</param>
        /// <param name="cut">true:剪切；false:复制</param>
        public static void CopyFileList(string[] files, bool cut = false)
        {
            if (files == null) return;
            IDataObject data = new DataObject(DataFormats.FileDrop, files);
            MemoryStream memo = new MemoryStream(4);
            byte[] bytes = new byte[] { (byte)(cut ? 2 : 5), 0, 0, 0 };
            memo.Write(bytes, 0, bytes.Length);
            data.SetData("PreferredDropEffect", memo);
            Clipboard.SetDataObject(data, false);
        }

        /// <summary>
        /// 获取剪贴板中的文件列表（方法）
        /// </summary>
        /// <returns>System.Collections.List<string>返回剪切板中文件路径集合</returns>
        public static List<string> GetClipboardList()
        {
            List<string> clipboardList = new List<string>();
            StringCollection sc = Clipboard.GetFileDropList();
            foreach (var listFileName in sc)
            {
                clipboardList.Add(listFileName);
            }
            return clipboardList;
        }
    }
}
