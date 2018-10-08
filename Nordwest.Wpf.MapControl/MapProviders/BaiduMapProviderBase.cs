using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

namespace Nordwest.Wpf.Controls.MapProviders
{
    public abstract class BaiduMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider
    {
        private string ClientKey = @"1308e84a0e8a1fc2115263a4b3cf87f1";
        public BaiduMapProviderBase()
        {
            MaxZoom = null;
            RefererUrl = @"http://map.baidu.com";
            Copyright = string.Format(@"©{0} Baidu Corporation, ©{0} NAVTEQ, ©{0} Image courtesy of NASA", DateTime.Today.Year);    
        }

        public override PureProjection Projection
        {
            get { return MercatorProjection.Instance; }
        }

        GMapProvider[] overlays;
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { this };
                }
                return overlays;
            }
        }

        public MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int Zoom)
        {
            throw new NotImplementedException();
        }

        
        string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
        {
            throw new NotImplementedException();
        }

        public MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int Zoom)
        {
            throw new NotImplementedException();
        }

        public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
        {
            return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), out pointList);
        }

        public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
        {
            List<PointLatLng> pointList;
            status = GetPoints(keywords, out pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        public GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
        {
            return GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out pointList);
        }

        public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
        {
            List<PointLatLng> pointList;
            status = GetLatLngFromGeocoderUrl(MakeGeocoderDetailedUrl(placemark), out pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
        {
            GeoCoderStatusCode status;
            pointList = null;

            try
            {
                string geo = /*GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(urlEnd, CacheType.GeocoderCache) :*/ string.Empty;

                if (string.IsNullOrEmpty(geo))
                {
                    geo = GetContentUsingHttp(url);

                    /*if (!string.IsNullOrEmpty(geo))
                    {
                        cache = true;
                    }*/
                }

                status = GeoCoderStatusCode.Unknow;
                if (!string.IsNullOrEmpty(geo))
                {
                    if (geo.StartsWith(@"<?xml") && geo.Contains(@"<GeocoderSearchResponse"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(geo);
                        XmlNode xn = doc[@"GeocoderSearchResponse"];
                        string statuscode = xn[@"status"].InnerText;
                        switch (statuscode)
                        {
                            case @"OK":
                                {
                                    pointList = new List<PointLatLng>();
                                    xn = xn[@"result"];
                                    XmlNodeList xnl = xn.ChildNodes;
                                    foreach (XmlNode xno in xnl)
                                    {
                                        XmlNode latitude = xno[@"lat"];
                                        XmlNode longitude = xno[@"lng"];
                                        if (latitude != null && longitude != null)
                                        {
                                            pointList.Add(new PointLatLng(Double.Parse(latitude.InnerText, CultureInfo.InvariantCulture),
                                                                            Double.Parse(longitude.InnerText, CultureInfo.InvariantCulture)));
                                        }

                                    }

                                    if (pointList.Count > 0)
                                    {
                                        status = GeoCoderStatusCode.G_GEO_SUCCESS;
                                        /* if (cache && GMaps.Instance.UseGeocoderCache)
                                            {
                                                Cache.Instance.SaveContent(urlEnd, CacheType.GeocoderCache, geo);
                                            }*/
                                        break;
                                    }

                                    status = GeoCoderStatusCode.G_GEO_UNKNOWN_ADDRESS;
                                    break;
                                }

                            case @"400":
                                status = GeoCoderStatusCode.G_GEO_BAD_REQUEST;
                                break; // bad request, The request contained an error.
                            case @"401":
                                status = GeoCoderStatusCode.G_GEO_BAD_KEY;
                                break; // Unauthorized, Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.
                            case @"403":
                                status = GeoCoderStatusCode.G_GEO_BAD_REQUEST;
                                break; // Forbidden, The request is for something forbidden. Authorization will not help.
                            case @"404":
                                status = GeoCoderStatusCode.G_GEO_UNKNOWN_ADDRESS;
                                break; // Not Found, The requested resource was not found. 
                            case @"500":
                                status = GeoCoderStatusCode.G_GEO_SERVER_ERROR;
                                break; // Internal Server Error, Your request could not be completed because there was a problem with the service.
                            case @"501":
                                status = GeoCoderStatusCode.Unknow;
                                break; // Service Unavailable, There's a problem with the service right now. Please try again later.
                            default:
                                status = GeoCoderStatusCode.Unknow;
                                break; // unknown, for possible future error codes
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status = GeoCoderStatusCode.ExceptionInCode;
                Debug.WriteLine(@"GetLatLngFromGeocoderUrl: " + ex);
            }

            return status;
        }

        string MakeGeocoderDetailedUrl(Placemark placemark)
        {
            string parameters = string.Empty;

            if (!AddFieldIfNotEmpty(ref parameters, "countryRegion", placemark.CountryNameCode))
                AddFieldIfNotEmpty(ref parameters, "countryRegion", placemark.CountryName);

            AddFieldIfNotEmpty(ref parameters, "adminDistrict", placemark.DistrictName);
            AddFieldIfNotEmpty(ref parameters, "locality", placemark.LocalityName);
            AddFieldIfNotEmpty(ref parameters, "postalCode", placemark.PostalCodeNumber);

            if (!string.IsNullOrEmpty(placemark.HouseNo))
                AddFieldIfNotEmpty(ref parameters, "addressLine", placemark.ThoroughfareName + " " + placemark.HouseNo);
            else
                AddFieldIfNotEmpty(ref parameters, "addressLine", placemark.ThoroughfareName);

            return MakeGeocoderUrl(parameters);
        }

        string MakeGeocoderUrl(string keywords)
        {
            return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, keywords, ClientKey);
        }

        bool AddFieldIfNotEmpty(ref string Input, string FieldName, string Value)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                if (string.IsNullOrEmpty(Input))
                    Input = string.Empty;
                else
                    Input = Input + @"&";

                Input = Input + FieldName + @"=" + Value;

                return true;
            }
            return false;
        }

        public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
        {
            throw new NotImplementedException();
        }

        public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
        {
            throw new NotImplementedException();
        }

        //http://api.map.baidu.com/geocoder?address=%E4%B8%8A%E5%9C%B0%E5%8D%81%E8%A1%9710%E5%8F%B7&output=json&key=37492c0ee6f924cb5e934fa08c6b1676
        static readonly string GeocoderUrlFormat = @"http://api.map.baidu.com/geocoder?address={0}&output=xml&key={1}";
    }
}