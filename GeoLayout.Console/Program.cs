using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoLayout.Domain;
using GeoLayout.Domain.IO;

namespace GeoLayout.Console {
    class Program {
        static void Main(string[] args) {

            var inputFile = @"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\keyPoints3.gpx";
            var outputFile = @"C:\Users\Епишкин Дмитрий\Desktop\ВЛУ\000.gpx";

            var importer = new GpxImporter();
            var exporter = new GpxExporter();

            var keyPoints = importer.ImportWaypoints(inputFile);

            var profile = ProfileFactory.CreateWithFixedStep(keyPoints, 200, i => "000-" + (i+1).ToString("D3"));

            //var profile = GridFactory.CreateRectangle(keyPoints[0], 500, 100, 10000, 5000, Math.PI / 4);

            exporter.ExportWaypoints(outputFile, profile);

            System.Console.WriteLine(@"Готово");
            System.Console.ReadKey();

        }
    }
}
