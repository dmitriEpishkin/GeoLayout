﻿using System;
using System.ComponentModel;
using GMap.NET;

namespace Nordwest.Wpf.Controls.MapProviders
{
    public class BaiduSateliteMapProvider : BaiduMapProviderBase
    {
        public static readonly BaiduSateliteMapProvider Instance;

        readonly Guid id = new Guid("89A10DFA-2557-431a-9656-20064E8D1342");
        public override Guid Id
        {
            get { return id; }
        }

        readonly string name = @"BaiduSateliteMap";
        public override string Name
        {
            get { return name; }
        }


        static BaiduSateliteMapProvider()
        {
            Instance = new BaiduSateliteMapProvider();
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            zoom = zoom - 1;
            var offsetX = Math.Pow(2, zoom);
            var offsetY = offsetX - 1;

            var numX = pos.X - offsetX;
            var numY = -pos.Y + offsetY;

            zoom = zoom + 1;
            var num = (pos.X + pos.Y) % 8 + 1;
            var x = numX.ToString().Replace(@"-", @"M");
            var y = numY.ToString().Replace(@"-", @"M");

            //return "http://q3.baidu.com/it/u=x=721;y=209;z=12;v=014;type=web&fm=44";
            //http://q3.baidu.com/it/u=x=721;y=209;z=12;v=014;type=web&fm=44
            string url = string.Format(UrlFormat, num, x, y, zoom, @"009", @"sate", @"46");

            return url;
        }

        static readonly string UrlFormat = @"http://q{0}.baidu.com/it/u=x={1};y={2};z={3};v={4};type={5}&fm={6}";
    }
}
