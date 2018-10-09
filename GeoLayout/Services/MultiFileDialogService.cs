using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using GeoLayout.Domain.IO;
using Microsoft.Win32;

namespace GeoLayout.Services {
    public class MultiFileDialogService {

        private readonly Window _owner;

        private readonly Lazy<OpenFileDialog> _fileDialog = new Lazy<OpenFileDialog>(() => new OpenFileDialog { Multiselect = true, CheckFileExists = true });

        public MultiFileDialogService(Window owner) {
            _owner = owner;
        }

        public MultiFileDialogResult ShowOpenFileDialog(IList<IFileTypeInfo> fileTypes) {
            if (fileTypes == null)
                throw new ArgumentNullException("fileTypes");
            if (!fileTypes.Any())
                throw new ArgumentException("Отсутствуют типы файлов");

            var res = ShowFileDialog(_owner, _fileDialog.Value, fileTypes, RecentFolder, true, true);

            if (res.FileNames.Length > 0)
                if (res.FileNames[0] != null)
                    RecentFolder = Path.GetDirectoryName(res.FileNames[0]);

            return res;
        }

        public MultiFileDialogResult ShowOpenFileDialog(IList<IFileTypeInfo> fileTypes, bool multiselect, string folder) {
            var prevFolder = RecentFolder;

            RecentFolder = folder;
            _fileDialog.Value.Multiselect = multiselect;

            var res = ShowOpenFileDialog(fileTypes);

            RecentFolder = prevFolder;
            _fileDialog.Value.Multiselect = true;

            return res;
        }

        private static MultiFileDialogResult ShowFileDialog(Window owner, FileDialog dialog, IList<IFileTypeInfo> fileTypes, string dirName, bool createAllSupportedFilter, bool createAllFilter) {
            if (!string.IsNullOrEmpty(dirName))
                dialog.InitialDirectory = dirName;

            createAllSupportedFilter = createAllSupportedFilter && fileTypes.Count > 1; 

            dialog.Filter = CreateFilter(fileTypes, createAllSupportedFilter, createAllFilter);

            if (!Directory.Exists(dialog.InitialDirectory))
                dialog.InitialDirectory = null;

            if (dialog.ShowDialog(owner) != true)
                return new MultiFileDialogResult(null, new string[0]);

            var filterIndex = dialog.FilterIndex - 1;
            if (createAllSupportedFilter)
                filterIndex--;

            IFileTypeInfo selectedFileType = null;
            if (filterIndex >= 0 && filterIndex < fileTypes.Count())
                selectedFileType = fileTypes.ElementAt(filterIndex);

            return new MultiFileDialogResult(selectedFileType, dialog.FileNames);
        }

        private static string CreateFilter(IList<IFileTypeInfo> fileTypes, bool createAllAcceptableMask, bool createAllMask) {
            string filter = "";

            // группы
            var groups = fileTypes.SelectMany(x => x.GetGroups()).Distinct();

            foreach (var g in groups) {
                var ff = fileTypes.Where(x => Array.Exists(x.GetGroups(), xx => xx == g));
                if (!string.IsNullOrEmpty(filter))
                    filter += @"|";
                filter += g + @"|*" + string.Join(@";*", ff.SelectMany(f => f.GetExtensions()));
            }

            if (createAllAcceptableMask) {
                if (!string.IsNullOrEmpty(filter))
                    filter += @"|";
                filter += "Все поддерживаемые форматы" + @"|*" + string.Join(@";*", fileTypes.SelectMany(f => f.GetExtensions()));
            }

            foreach (var fileType in fileTypes) {
                if (!string.IsNullOrEmpty(filter))
                    filter += @"|";
                filter += fileType.GetDescription(CultureInfo.CurrentUICulture) + @"|*" + string.Join(@";*", fileType.GetExtensions());
            }

            if (createAllMask)
                filter += "|Все файлы|*.*";

            return filter;
        }

        public string RecentFolder { get; private set; }
    }
}
