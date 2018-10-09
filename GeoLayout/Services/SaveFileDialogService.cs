using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Waf.Applications.Services;
using System.Windows;
using GeoLayout.Domain.IO;
using Microsoft.Win32;

namespace GeoLayout.Services {
    public class SaveFileDialogService {

        private readonly Window _owner;

        public SaveFileDialogService(Window owner) {
            _owner = owner;
        }

        public SaveFileDialogResult SaveFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName) {
            var fileTypesList = fileTypes as IList<FileType> ?? fileTypes.ToList();
            var s = new SaveFileDialog {
                InitialDirectory = (RecentFolder != null && Directory.Exists(RecentFolder)) ? RecentFolder :
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = CreateFilter(fileTypesList)
            };

            string fileName = null;

            s.FileName = Path.Combine(s.InitialDirectory, defaultFileName);

            int filterIndex = fileTypesList.IndexOf(defaultFileType);
            if (filterIndex >= 0) { s.FilterIndex = filterIndex + 1; }

            if (s.ShowDialog(_owner) == true) {
                RecentFolder = Path.GetDirectoryName(s.FileName);
                fileName = s.FileName;
            }
            
            return new SaveFileDialogResult(fileTypesList[s.FilterIndex - 1], fileName);
        }

        private static string CreateFilter(IEnumerable<FileType> fileTypes) {
            string filter = "";
            foreach (FileType fileType in fileTypes) {
                if (!string.IsNullOrEmpty(filter)) { filter += @"|"; }
                filter += fileType.Description + @"|*" + fileType.FileExtension;
            }
            return filter;
        }

        public string RecentFolder { get; private set; }

    }
}
