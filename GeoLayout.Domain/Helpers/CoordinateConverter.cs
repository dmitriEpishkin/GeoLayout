using System;
using System.Diagnostics;
using System.Globalization;

namespace Northwest.MTToolsCore
{
    public class CoordinateConverter
    {
        private const double Deg2Rad = Math.PI / 180;

        //public static Coordinate Wgs84ToPulkovo42(Coordinate c, int zoneNumber)
        //{
        //    if (c.System != @"WGS-84")
        //        throw new InvalidOperationException("Coordinate must be in WGS-84 system!");

        //    double lat = c.Lat * Deg2Rad;
        //    double lon = c.Long * Deg2Rad;
        //    double hei = c.Height;

        //    WGS84ToPulkovo42(ref lat, ref lon, ref hei);

        //    Coordinate work = new Coordinate("Pulkovo-42");

        //    work.Lat = c.Lat;// lat / magicNumber;
        //    work.Long = c.Long;// lon / magicNumber;
        //    work.Height = hei;

        //    S_TransMercator Par_gauss = new S_TransMercator();
        //    Par_gauss.CentralMeridian = 0;
        //    Par_gauss.OriginLatitude = 0;
        //    Par_gauss.ScaleFactor = 1;
        //    Par_gauss.North = 0;
        //    Par_gauss.East = 500000;
        //    Par_gauss.Auto = true;

        //    S_TransMercator proj_par = Par_gauss;
        //    proj_par.CentralMeridian = (zoneNumber * 6.0 - 3.0) * Deg2Rad;
        //    proj_par.East = proj_par.East + zoneNumber * 1000000.0;
        //    Project_TMR(ref proj_par, ref lat, ref lon);

        //    work.X = lon;
        //    work.Y = lat;
        //    work.Alt = hei;

        //    return work;
        //}
        
        //public static Coordinate Wgs84ToUTM(Coordinate coordinate)
        //{
        //    if (coordinate.System != @"WGS-84")
        //        throw new InvalidOperationException("Coordinate must be in WGS-84 system!");

        //    double utmNorthing;
        //    double utmEasting;
        //    string utmZone;
        //    LatLongToUTM(23, coordinate.Lat, coordinate.Long, out utmNorthing, out  utmEasting, out utmZone);

        //    return new Coordinate(@"UTM") { Alt = coordinate.Height, Height = coordinate.Height, Lat = coordinate.Lat, Long = coordinate.Long, X = utmEasting, Y = utmNorthing };
        //}

        #region Private

        private static void WGS84ToPulkovo42(ref double lat, ref double lon, ref double hei)
        {
            Sellips el_WGS84 = new Sellips(), da_out = new Sellips(), el_out = new Sellips();
            el_WGS84.A = 6378137;
            el_WGS84.B = 6356752.3142;
            el_WGS84.C = 1.0 / 298.257223563;
            el_out.A = 6378245;
            el_out.B = 6356863.019;
            el_out.C = 1.0 / 298.3;
            da_out.A = 28;
            da_out.B = -130;
            da_out.C = -95;

            ShiftFromWGS84(ref el_WGS84, ref da_out, ref el_out, ref lon, ref lat, ref hei);
        }

        private static void ShiftFromWGS84(ref Sellips el_WGS84, ref Sellips da_out, ref Sellips el_out, ref double Lon, ref double Lat, ref double H)
        {
            double da = el_out.A - el_WGS84.A;
            double df = el_out.C - el_WGS84.C;
            double out_Lat = 0.0, out_Lon = 0.0, out_Hgt = 0.0;
            MolodenskyShift(el_out.A, da, el_out.C, df, -da_out.A, -da_out.B, -da_out.C, Lat, Lon, H, ref out_Lat, ref out_Lon, ref out_Hgt);
            Lon = out_Lon;
            Lat = out_Lat;
            H = out_Hgt;
        }

        private static void MolodenskyShift(double a, double da, double f, double df, double dx, double dy, double dz, double Lat_in, double Lon_in, double Hgt_in, ref double Lat_out, ref double Lon_out, ref double Hgt_out)
        {
            double tLon_in;   /* temp longitude                                     */
            double e2;        /* Intermediate calculations for dp, dl               */
            double ep2;       /* Intermediate calculations for dp, dl               */
            double sin_Lat;   /* sin(Latitude_1)                                    */
            double sin2_Lat;  /* (sin(Latitude_1))^2                                */
            double sin_Lon;   /* sin(Longitude_1)                                   */
            double cos_Lat;   /* cos(Latitude_1)                                    */
            double cos_Lon;   /* cos(Longitude_1)                                   */
            double w2;        /* Intermediate calculations for dp, dl               */
            double w;         /* Intermediate calculations for dp, dl               */
            double w3;        /* Intermediate calculations for dp, dl               */
            double m;         /* Intermediate calculations for dp, dl               */
            double n;         /* Intermediate calculations for dp, dl               */
            double dp;        /* Delta phi                                          */
            double dp1;       /* Delta phi calculations                             */
            double dp2;       /* Delta phi calculations                             */
            double dp3;       /* Delta phi calculations                             */
            double dl;        /* Delta lambda                                       */
            double dh;        /* Delta height                                       */
            double dh1;       /* Delta height calculations                          */
            double dh2;       /* Delta height calculations                          */

            if (Lon_in > Math.PI) tLon_in = Lon_in - (2 * Math.PI);
            else tLon_in = Lon_in;
            e2 = 2 * f - f * f;
            ep2 = e2 / (1 - e2);
            sin_Lat = Math.Sin(Lat_in);
            cos_Lat = Math.Cos(Lat_in);
            sin_Lon = Math.Sin(tLon_in);
            cos_Lon = Math.Cos(tLon_in);
            sin2_Lat = sin_Lat * sin_Lat;
            w2 = 1.0 - e2 * sin2_Lat;
            w = Math.Sqrt(w2);
            w3 = w * w2;
            m = (a * (1.0 - e2)) / w3;
            n = a / w;
            dp1 = cos_Lat * dz - sin_Lat * cos_Lon * dx - sin_Lat * sin_Lon * dy;
            dp2 = ((e2 * sin_Lat * cos_Lat) / w) * da;
            dp3 = sin_Lat * cos_Lat * (2.0 * n + ep2 * m * sin2_Lat) * (1.0 - f) * df;
            dp = (dp1 + dp2 + dp3) / (m + Hgt_in);
            dl = (-sin_Lon * dx + cos_Lon * dy) / ((n + Hgt_in) * cos_Lat);
            dh1 = (cos_Lat * cos_Lon * dx) + (cos_Lat * sin_Lon * dy) + (sin_Lat * dz);
            dh2 = -(w * da) + ((a * (1 - f)) / w) * sin2_Lat * df;
            dh = dh1 + dh2;
            Lat_out = Lat_in + dp;
            Lon_out = Lon_in + dl;
            Hgt_out = Hgt_in + dh;
            if (Lon_out > (Math.PI * 2)) Lon_out -= 2 * Math.PI;
            if (Lon_out < (-Math.PI)) Lon_out += 2 * Math.PI;
        }

        private static void Project_TMR(ref S_TransMercator proj_par, ref double Lat, ref double Lon)
        {
            double tn;        /* True Meridianal distance constant  */
            double tn2;
            double tn3;
            double tn4;
            double tn5;

            double TranMerc_b = 6356863.019; // Semi-minor axis of ellipsoid, in meters
            double TranMerc_a = 6378245;
            double TranMerc_f = 1.0 / 298.3;
            double TranMerc_Origin_Lat = proj_par.OriginLatitude;
            double TranMerc_Origin_Long = proj_par.CentralMeridian;
            double TranMerc_False_Northing = proj_par.North;
            double TranMerc_False_Easting = proj_par.East;
            double TranMerc_Scale_Factor = proj_par.ScaleFactor;
            double TranMerc_es = 2.0 * TranMerc_f - TranMerc_f * TranMerc_f;
            double TranMerc_ebs = (1.0 / (1.0 - TranMerc_es)) - 1.0;

            // True meridianal constants
            tn = (TranMerc_a - TranMerc_b) / (TranMerc_a + TranMerc_b);
            tn2 = tn * tn;
            tn3 = tn2 * tn;
            tn4 = tn3 * tn;
            tn5 = tn4 * tn;

            double TranMerc_ap = TranMerc_a * (1.0 - tn + 5.0 * (tn2 - tn3) / 4.0 + 81.0 * (tn4 - tn5) / 64.0);
            double TranMerc_bp = 3.0e0 * TranMerc_a * (tn - tn2 + 7.0 * (tn3 - tn4) / 8.0 + 55.0 * tn5 / 64.0) / 2.0;
            double TranMerc_cp = 15.0 * TranMerc_a * (tn2 - tn3 + 3.0 * (tn4 - tn5) / 4.0) / 16.0;
            double TranMerc_dp = 35.0 * TranMerc_a * (tn3 - tn4 + 11.0 * tn5 / 16.0) / 48.0;
            double TranMerc_ep = 315.0 * TranMerc_a * (tn4 - tn5) / 512.0;
            // Real Convert Geodetic To Transverse Mercator
            double c;       // Cosine of latitude
            double c2, c3, c5, c7;
            double dlam;    // Delta longitude - Difference in Longitude
            double eta;     // constant - TranMerc_ebs *c *c
            double eta2, eta3, eta4;
            double s;       // Sine of latitude
            double sn;      // Radius of curvature in the prime vertical
            double t;       // Tangent of latitude
            double tan2, tan3, tan4, tan5, tan6;
            double t1;      // Term in coordinate conversion formula - GP to Y
            double t2;      // Term in coordinate conversion formula - GP to Y
            double t3;      // Term in coordinate conversion formula - GP to Y
            double t4;      // Term in coordinate conversion formula - GP to Y
            double t5;      // Term in coordinate conversion formula - GP to Y
            double t6;      // Term in coordinate conversion formula - GP to Y
            double t7;      // Term in coordinate conversion formula - GP to Y
            double t8;      // Term in coordinate conversion formula - GP to Y
            double t9;      // Term in coordinate conversion formula - GP to Y
            double tmd;     // True Meridional distance
            double tmdo;    // True Meridional distance for latitude of origin
            double x_East, y_North;

            if (Lon > Math.PI) Lon -= (2.0 * Math.PI);
            dlam = Lon - TranMerc_Origin_Long;
            if (dlam > Math.PI) dlam -= (2.0 * Math.PI);
            if (dlam < -Math.PI) dlam += (2.0 * Math.PI);
            if (Math.Abs(dlam) < 2.0e-10) dlam = 0.0;

            s = Math.Sin(Lat); c = Math.Cos(Lat);
            c2 = c * c;
            c3 = c2 * c;
            c5 = c3 * c2;
            c7 = c5 * c2;
            t = Math.Tan(Lat);
            tan2 = t * t;
            tan3 = tan2 * t;
            tan4 = tan3 * t;
            tan5 = tan4 * t;
            tan6 = tan5 * t;
            eta = TranMerc_ebs * c2;
            eta2 = eta * eta;
            eta3 = eta2 * eta;
            eta4 = eta3 * eta;

            // radius of curvature in prime vertical
            sn = TranMerc_a / Math.Sqrt(1.0e0 - TranMerc_es * s * s);

            // True Meridianal Distances
            tmd = TranMerc_ap * Lat -
                  TranMerc_bp * Math.Sin(2.0 * Lat) +
                  TranMerc_cp * Math.Sin(4.0 * Lat) -
                  TranMerc_dp * Math.Sin(6.0 * Lat) +
                  TranMerc_ep * Math.Sin(8.0 * Lat);

            //  Origin
            tmdo = TranMerc_ap * TranMerc_Origin_Lat -
                   TranMerc_bp * Math.Sin(2.0 * TranMerc_Origin_Lat) +
                   TranMerc_cp * Math.Sin(4.0 * TranMerc_Origin_Lat) -
                   TranMerc_dp * Math.Sin(6.0 * TranMerc_Origin_Lat) +
                   TranMerc_ep * Math.Sin(8.0 * TranMerc_Origin_Lat);

            // y_North
            t1 = (tmd - tmdo) * TranMerc_Scale_Factor;
            t2 = sn * s * c * TranMerc_Scale_Factor / 2.0;
            t3 = sn * s * c3 * TranMerc_Scale_Factor * (5.0 - tan2 + 9.0 * eta
                                                     + 4.0 * eta2) / 24.0;

            t4 = sn * s * c5 * TranMerc_Scale_Factor * (61.0 - 58.0 * tan2
                                                     + tan4 + 270.0 * eta - 330.0 * tan2 * eta + 445.0 * eta2
                                                     + 324.0 * eta3 - 680.0 * tan2 * eta2 + 88.0 * eta4
                                                     - 600.0 * tan2 * eta3 - 192.0 * tan2 * eta4) / 720.0;

            t5 = sn * s * c7 * TranMerc_Scale_Factor * (1385.0 - 3111.0 *
                                                     tan2 + 543.0 * tan4 - tan6) / 40320.0;

            y_North = TranMerc_False_Northing + t1 + Math.Pow(dlam, 2.0) * t2
                    + Math.Pow(dlam, 4.0) * t3 + Math.Pow(dlam, 6.0) * t4
                    + Math.Pow(dlam, 8.0) * t5;

            // x_East
            t6 = sn * c * TranMerc_Scale_Factor;
            t7 = sn * c3 * TranMerc_Scale_Factor * (1.0 - tan2 + eta) / 6.0;
            t8 = sn * c5 * TranMerc_Scale_Factor * (5.0 - 18.0 * tan2 + tan4
                                                 + 14.0 * eta - 58.0 * tan2 * eta + 13.0 * eta2 + 4.0 * eta3
                                                 - 64.0 * tan2 * eta2 - 24.0 * tan2 * eta3) / 120.0;
            t9 = sn * c7 * TranMerc_Scale_Factor * (61.0 - 479.0 * tan2
                                                 + 179.0 * tan4 - tan6) / 5040.0;

            x_East = TranMerc_False_Easting + dlam * t6 + Math.Pow(dlam, 3.0) * t7
                   + Math.Pow(dlam, 5.0) * t8 + Math.Pow(dlam, 7.0) * t9;

            Lat = y_North;
            Lon = x_East;
        }


        #endregion

        //честно стырено с http://www.gpsy.com/gpsinfo/geotoutm/
        private static readonly Ellipsoid[] ellipsoid = new Ellipsoid[]
        {
            //  id, Ellipsoid name, Equatorial Radius, square of eccentricity	
            new Ellipsoid(-1, "Placeholder", 0, 0), //placeholder only, To allow array indices to match id numbers
            new Ellipsoid(1, "Airy", 6377563, 0.00667054),
            new Ellipsoid(2, "Australian National", 6378160, 0.006694542),
            new Ellipsoid(3, "Bessel 1841", 6377397, 0.006674372),
            new Ellipsoid(4, "Bessel 1841 (Nambia) ", 6377484, 0.006674372),
            new Ellipsoid(5, "Clarke 1866", 6378206, 0.006768658),
            new Ellipsoid(6, "Clarke 1880", 6378249, 0.006803511),
            new Ellipsoid(7, "Everest", 6377276, 0.006637847),
            new Ellipsoid(8, "Fischer 1960 (Mercury) ", 6378166, 0.006693422),
            new Ellipsoid(9, "Fischer 1968", 6378150, 0.006693422),
            new Ellipsoid(10, "GRS 1967", 6378160, 0.006694605),
            new Ellipsoid(11, "GRS 1980", 6378137, 0.00669438),
            new Ellipsoid(12, "Helmert 1906", 6378200, 0.006693422),
            new Ellipsoid(13, "Hough", 6378270, 0.00672267),
            new Ellipsoid(14, "International", 6378388, 0.00672267),
            new Ellipsoid(15, "Krassovsky", 6378245, 0.006693422),
            new Ellipsoid(16, "Modified Airy", 6377340, 0.00667054),
            new Ellipsoid(17, "Modified Everest", 6377304, 0.006637847),
            new Ellipsoid(18, "Modified Fischer 1960", 6378155, 0.006693422),
            new Ellipsoid(19, "South American 1969", 6378160, 0.006694542),
            new Ellipsoid(20, "WGS 60", 6378165, 0.006693422),
            new Ellipsoid(21, "WGS 66", 6378145, 0.006694542),
            new Ellipsoid(22, "WGS-72", 6378135, 0.006694318),
            new Ellipsoid(23, @"WGS-84", 6378137, 0.00669438)
        };

        private static void LatLongToUTM(int referenceEllipsoid, double latitude, double longitude, out double utmNorthing, out double utmEasting,
            out string utmZone)
        {
            //converts lat/long to UTM coords.  Equations from USGS Bulletin 1532 
            //East Longitudes are positive, West longitudes are negative. 
            //North latitudes are positive, South latitudes are negative
            //Lat and longitude are in decimal degrees
            //Written by Chuck Gantz- chuck.gantz@globalstar.com

            var a = ellipsoid[referenceEllipsoid].EquatorialRadius;
            var eccSquared = ellipsoid[referenceEllipsoid].EccentricitySquared;
            const double k0 = 0.9996;
            const double deg2Rad = Math.PI/180;

            //Make sure the longitude is between -180.00 .. 179.9
            var longTemp = (longitude + 180) - (int) ((longitude + 180)/360)*360 - 180; // -180.00 .. 179.9;

            var latRad = latitude*deg2Rad;
            var longRad = longTemp*deg2Rad;

            var zoneNumber = (int) ((longTemp + 180)/6) + 1;

            if (latitude >= 56.0 && latitude < 64.0 && longTemp >= 3.0 && longTemp < 12.0)
                zoneNumber = 32;

            // Special zones for Svalbard
            if (latitude >= 72.0 && latitude < 84.0)
            {
                if (longTemp >= 0.0 && longTemp < 9.0) zoneNumber = 31;
                else if (longTemp >= 9.0 && longTemp < 21.0) zoneNumber = 33;
                else if (longTemp >= 21.0 && longTemp < 33.0) zoneNumber = 35;
                else if (longTemp >= 33.0 && longTemp < 42.0) zoneNumber = 37;
            }
            var longOrigin = (double)(zoneNumber - 1) * 6 - 180 + 3;
            var longOriginRad = longOrigin*deg2Rad;

            //compute the UTM Zone from the latitude and longitude
            //sprintf(UTMZone, "%d%c", ZoneNumber, UTMLetterDesignator(Lat));
            utmZone = zoneNumber.ToString(CultureInfo.InvariantCulture) + UTMLetterDesignator(latitude);

            var eccPrimeSquared = (eccSquared)/(1 - eccSquared);

            var N = a/Math.Sqrt(1 - eccSquared*Math.Sin(latRad)*Math.Sin(latRad));
            var T = Math.Tan(latRad)*Math.Tan(latRad);
            var C = eccPrimeSquared*Math.Cos(latRad)*Math.Cos(latRad);
            var A = Math.Cos(latRad)*(longRad - longOriginRad);

            var M = a*((1 - eccSquared/4 - 3*eccSquared*eccSquared/64 - 5*eccSquared*eccSquared*eccSquared/256)*latRad
                       - (3*eccSquared/8 + 3*eccSquared*eccSquared/32 + 45*eccSquared*eccSquared*eccSquared/1024)*Math.Sin(2*latRad)
                       + (15*eccSquared*eccSquared/256 + 45*eccSquared*eccSquared*eccSquared/1024)*Math.Sin(4*latRad)
                       - (35*eccSquared*eccSquared*eccSquared/3072)*Math.Sin(6*latRad));

            utmEasting = k0*N*(A + (1 - T + C)*A*A*A/6 + (5 - 18*T + T*T + 72*C - 58*eccPrimeSquared)*A*A*A*A*A/120) + 500000.0;
            utmNorthing = k0*(M + N*Math.Tan(latRad)*(A*A/2 + (5 - T + 9*C + 4*C*C)*A*A*A*A/24+(61 - 58*T + T*T + 600*C - 330*eccPrimeSquared)*A*A*A*A*A*A/720));
            
            if (latitude < 0)
                utmNorthing += 10000000.0; //10000000 meter offset for southern hemisphere
        }

        private static char UTMLetterDesignator(double lat)
        {
            //This routine determines the correct UTM letter designator for the given latitude
            //returns 'Z' if latitude is outside the UTM limits of 84N to 80S
            //Written by Chuck Gantz- chuck.gantz@globalstar.com
            if ((84 >= lat) && (lat >= 72))     return 'X';
            if ((72 > lat) && (lat >= 64))      return 'W';
            if ((64 > lat) && (lat >= 56))      return 'V';
            if ((56 > lat) && (lat >= 48))      return 'U';
            if ((48 > lat) && (lat >= 40))      return 'T';
            if ((40 > lat) && (lat >= 32))      return 'S';
            if ((32 > lat) && (lat >= 24))      return 'R';
            if ((24 > lat) && (lat >= 16))      return 'Q';
            if ((16 > lat) && (lat >= 8))       return 'P';
            if ((8 > lat) && (lat >= 0))        return 'N';
            if ((0 > lat) && (lat >= -8))       return 'M';
            if ((-8 > lat) && (lat >= -16))     return 'L';
            if ((-16 > lat) && (lat >= -24))    return 'K';
            if ((-24 > lat) && (lat >= -32))    return 'J';
            if ((-32 > lat) && (lat >= -40))    return 'H';
            if ((-40 > lat) && (lat >= -48))    return 'G';
            if ((-48 > lat) && (lat >= -56))    return 'F';
            if ((-56 > lat) && (lat >= -64))    return 'E';
            if ((-64 > lat) && (lat >= -72))    return 'D';
            if ((-72 > lat) && (lat >= -80))    return 'C';
            return 'Z'; //This is here as an error flag to show that the Latitude is outside the UTM limits
        }

        private class Ellipsoid
        {
            public Ellipsoid(int id, string name, double radius, double ecc)
            {
                Id = id;
                EllipsoidName = name;
                EquatorialRadius = radius;
                EccentricitySquared = ecc;
            }

            public readonly int Id;
            public readonly string EllipsoidName;
            public readonly double EquatorialRadius;
            public readonly double EccentricitySquared;
        }

        public struct Sellips {
            public string name;
            public string index;
            public double A, B, C;
        }

        public struct S_TransMercator {
            public double CentralMeridian;
            public double OriginLatitude;
            public double ScaleFactor;
            public double North;
            public double East;
            public bool Auto;
        }
    }
}
