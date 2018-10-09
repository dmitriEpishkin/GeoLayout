using System;
using GeoLayout.Domain.Data;

namespace GeoLayout.Domain {
    public static class WgsUtmConverter {
        //<!-- Copyright 1997-1998 by Charles L.Taylor -->

        /* Ellipsoid model constants (actual values here are for WGS84) */
        const double sm_a = 6378137.0;
        const double sm_b = 6356752.314;
        const double sm_EccSquared = 6.69437999013e-03;
        const double UTMScaleFactor = 0.9996;

    /*
    * LatLonToUTMXY
    *
    * Converts a latitude/longitude pair to x and y coordinates in the
    * Universal Transverse Mercator projection.
    *
    * Inputs:
    *   lat - Latitude of the point, in degrees.
    *   lon - Longitude of the point, in degrees.
    *   zone - UTM zone to be used for calculating values for x and y.
    *          If zone is less than 1 or greater than 60, the routine
    *          will determine the appropriate zone from the value of lon.
    *
    * Outputs:
    *   xy - A 2-element array where the UTM x and y values will be stored.
    *
    */

    public static GeoLocationXY LatLonToUTMXY(GeoLocation latLng, int zone) {

        // Compute the UTM zone.
        if (zone < 1 || zone > 60)
            zone = (int)Math.Floor((latLng.Longitude + 180.0) / 6) + 1;

        var lat = DegToRad(latLng.Latitude);
        var lon = DegToRad(latLng.Longitude);

        var xy = MapLatLonToXy(lat, lon, UTMCentralMeridian(zone));

        /* Adjust easting and northing for UTM system. */
        var xNew = xy.Item1 * UTMScaleFactor + 500000.0;
        var yNew = xy.Item2 * UTMScaleFactor;
        if (xy.Item2 < 0.0)
            yNew = yNew + 10000000.0;

        return new GeoLocationXY(zone, xNew, yNew, latLng.Elevation);
    }

    /*
    * UTMXYToLatLon
    *
    * Converts x and y coordinates in the Universal Transverse Mercator
    * projection to a latitude/longitude pair.
    *
    * Inputs:
    *	x - The easting of the point, in meters.
    *	y - The northing of the point, in meters.
    *	zone - The UTM zone in which the point lies.
    *	southhemi - True if the point is in the southern hemisphere;
    *               false otherwise.
    *
    * Outputs:
    *	latlon - A 2-element array containing the latitude and
    *            longitude of the point, in degrees.
    */

    public static GeoLocation UTMXYToLatLon(GeoLocationXY xy, bool southhemi) {

        if (xy.Zone < 1 || xy.Zone > 60)
            throw new ArgumentOutOfRangeException(nameof(xy.Zone));

        var x = xy.X - 500000.0;
        x /= UTMScaleFactor;

        var y = xy.Y;

        /* If in southern hemisphere, adjust y accordingly. */
        if (southhemi)
            y -= 10000000;

        y /= UTMScaleFactor;

        var cmeridian = UTMCentralMeridian(xy.Zone);
        var rad = MapXyToLatLon(x, y, cmeridian);

        return new GeoLocation(RadToDeg(rad.Item1), RadToDeg(rad.Item2), xy.Elevation);
    }

    private static double DegToRad(double deg) {
        return deg / 180.0 * Math.PI;
    }
    private static double RadToDeg(double rad) {
        return rad / Math.PI * 180.0;
    }

    /*
    * Computes the ellipsoidal distance from the equator to a point at a
    * given latitude.
    *
    * Reference: Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
    * GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
    *
    * Inputs:
    *     phi - Latitude of the point, in radians.
    *
    * Globals:
    *     sm_a - Ellipsoid model major axis.
    *     sm_b - Ellipsoid model minor axis.
    *
    * Returns:
    *     The ellipsoidal distance of the point from the equator, in meters.
    *
    */

    private static double ArcLengthOfMeridian(double phi) {

        var n = (sm_a - sm_b) / (sm_a + sm_b);
        var alpha = (sm_a + sm_b) / 2.0 * (1.0 + n * n / 4.0 + n * n * n * n / 64.0);
        var beta = -3.0 * n / 2.0 + 9.0 * n * n * n / 16.0 - 3.0 * n * n * n * n * n / 32.0;
        var gamma = 15.0 * n * n / 16.0 - 15.0 * n * n * n * n / 32.0;
        var delta = -35.0 * n * n * n / 48.0 + 105.0 * n * n * n * n * n / 256.0;
        var epsilon = 315.0 * n * n * n * n / 512.0;

        return alpha * (phi + beta * Math.Sin(2.0 * phi)
                      + gamma * Math.Sin(4.0 * phi)
                      + delta * Math.Sin(6.0 * phi)
                      + epsilon * Math.Sin(8.0 * phi));
    }

    /*
    * Determines the central meridian for the given UTM zone.
    *
    * Inputs:
    *     zone - An integer value designating the UTM zone, range [1,60].
    *
    * Returns:
    *   The central meridian for the given UTM zone, in radians, or zero
    *   if the UTM zone parameter is outside the range [1,60].
    *   Range of the central meridian is the radian equivalent of [-177,+177].
    *
    */

    private static double UTMCentralMeridian(int zone) {
        return DegToRad(-183.0 + (zone * 6.0));
    }

    /*
    * Computes the footpoint latitude for use in converting transverse
    * Mercator coordinates to ellipsoidal coordinates.
    *
    * Reference: Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
    *   GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
    *
    * Inputs:
    *   y - The UTM northing coordinate, in meters.
    *
    * Returns:
    *   The footpoint latitude, in radians.
    *
    */

    private static double FootpointLatitude(double y) {
        /* Precalculate n (Eq. 10.18) */
        var n = (sm_a - sm_b) / (sm_a + sm_b);
        /* Precalculate alpha_ (Eq. 10.22) */
        /* (Same as alpha in Eq. 10.17) */
        var alpha = ((sm_a + sm_b) / 2.0)
                    * (1 + (Math.Pow(n, 2.0) / 4) + (Math.Pow(n, 4.0) / 64));
        /* Precalculate y_ (Eq. 10.23) */
        var y_ = y / alpha;
        /* Precalculate beta_ (Eq. 10.22) */
        var beta = (3.0 * n / 2.0) + (-27.0 * Math.Pow(n, 3.0) / 32.0)
                   + (269.0 * Math.Pow(n, 5.0) / 512.0);
        /* Precalculate gamma_ (Eq. 10.22) */
        var gamma = (21.0 * Math.Pow(n, 2.0) / 16.0)
                    + (-55.0 * Math.Pow(n, 4.0) / 32.0);
        /* Precalculate delta_ (Eq. 10.22) */
        var delta = (151.0 * Math.Pow(n, 3.0) / 96.0)
                    + (-417.0 * Math.Pow(n, 5.0) / 128.0);
        /* Precalculate epsilon_ (Eq. 10.22) */
        var epsilon = (1097.0 * Math.Pow(n, 4.0) / 512.0);
        /* Now calculate the sum of the series (Eq. 10.21) */
        var result = y_ + (beta * Math.Sin(2.0 * y_))
                     + (gamma * Math.Sin(4.0 * y_))
                     + (delta * Math.Sin(6.0 * y_))
                     + (epsilon * Math.Sin(8.0 * y_));

        return result;
    }

    /*
    * MapLatLonToXY
    *
    * Converts a latitude/longitude pair to x and y coordinates in the
    * Transverse Mercator projection.  Note that Transverse Mercator is not
    * the same as UTM; a scale factor is required to convert between them.
    *
    * Reference: Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
    * GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
    *
    * Inputs:
    *    phi - Latitude of the point, in radians.
    *    lambda - Longitude of the point, in radians.
    *    lambda0 - Longitude of the central meridian to be used, in radians.
    *
    * Outputs:
    *    xy - A 2-element array containing the x and y coordinates
    *         of the computed point.
    *
    * Returns:
    *    The function does not return a value.
    *
    */

    private static Tuple<double, double> MapLatLonToXy(double phi, double lambda, double lambda0) {

        var ep2 = (Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0)) / Math.Pow(sm_b, 2.0);
        var nu2 = ep2 * Math.Pow(Math.Cos(phi), 2.0);
        var n = Math.Pow(sm_a, 2.0) / (sm_b * Math.Sqrt(1 + nu2));
        var t = Math.Tan(phi);
        var t2 = t * t;
        var l = lambda - lambda0;

        /* Precalculate coefficients for l**n in the equations below
           so a normal human being can read the expressions for easting
           and northing
           -- l**1 and l**2 have coefficients of 1.0 */
        var l3Coef = 1.0 - t2 + nu2;

        var l4Coef = 5.0 - t2 + 9 * nu2 + 4.0 * (nu2 * nu2);

        var l5Coef = 5.0 - 18.0 * t2 + (t2 * t2) + 14.0 * nu2
                     - 58.0 * t2 * nu2;

        var l6Coef = 61.0 - 58.0 * t2 + (t2 * t2) + 270.0 * nu2
                     - 330.0 * t2 * nu2;

        var l7Coef = 61.0 - 479.0 * t2 + 179.0 * (t2 * t2) - (t2 * t2 * t2);

        var l8Coef = 1385.0 - 3111.0 * t2 + 543.0 * (t2 * t2) - (t2 * t2 * t2);

        /* Calculate easting (x) */
        var x = n * Math.Cos(phi) * l
                + (n / 6.0 * Math.Pow(Math.Cos(phi), 3.0) * l3Coef * Math.Pow(l, 3.0))
                + (n / 120.0 * Math.Pow(Math.Cos(phi), 5.0) * l5Coef * Math.Pow(l, 5.0))
                + (n / 5040.0 * Math.Pow(Math.Cos(phi), 7.0) * l7Coef * Math.Pow(l, 7.0));

        /* Calculate northing (y) */
        var y = ArcLengthOfMeridian(phi)
                + (t / 2.0 * n * Math.Pow(Math.Cos(phi), 2.0) * Math.Pow(l, 2.0))
                + (t / 24.0 * n * Math.Pow(Math.Cos(phi), 4.0) * l4Coef * Math.Pow(l, 4.0))
                + (t / 720.0 * n * Math.Pow(Math.Cos(phi), 6.0) * l6Coef * Math.Pow(l, 6.0))
                + (t / 40320.0 * n * Math.Pow(Math.Cos(phi), 8.0) * l8Coef * Math.Pow(l, 8.0));

        return new Tuple<double, double>(x, y);
    }

    /*
    * MapXYToLatLon
    *
    * Converts x and y coordinates in the Transverse Mercator projection to
    * a latitude/longitude pair.  Note that Transverse Mercator is not
    * the same as UTM; a scale factor is required to convert between them.
    *
    * Reference: Hoffmann-Wellenhof, B., Lichtenegger, H., and Collins, J.,
    *   GPS: Theory and Practice, 3rd ed.  New York: Springer-Verlag Wien, 1994.
    *
    * Inputs:
    *   x - The easting of the point, in meters.
    *   y - The northing of the point, in meters.
    *   lambda0 - Longitude of the central meridian to be used, in radians.
    *
    * Outputs:
    *   philambda - A 2-element containing the latitude and longitude
    *               in radians.
    *
    * Returns:
    *   The function does not return a value.
    *
    * Remarks:
    *   The local variables Nf, nuf2, tf, and tf2 serve the same purpose as
    *   N, nu2, t, and t2 in MapLatLonToXY, but they are computed with respect
    *   to the footpoint latitude phif.
    *
    *   x1frac, x2frac, x2poly, x3poly, etc. are to enhance readability and
    *   to optimize computations.
    *
    */

    private static Tuple<double, double> MapXyToLatLon(double x, double y, double lambda0) {

        /* Get the value of phif, the footpoint latitude. */
        var phif = FootpointLatitude(y);

        /* Precalculate ep2 */
        var ep2 = (Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0))
                  / Math.Pow(sm_b, 2.0);

        /* Precalculate cos (phif) */
        var cf = Math.Cos(phif);

        /* Precalculate nuf2 */
        var nuf2 = ep2 * Math.Pow(cf, 2.0);

        /* Precalculate Nf and initialize Nfpow */
        var nf = Math.Pow(sm_a, 2.0) / (sm_b * Math.Sqrt(1 + nuf2));
        var nfpow = nf;

        /* Precalculate tf */
        var tf = Math.Tan(phif);
        var tf2 = tf * tf;
        var tf4 = tf2 * tf2;

        /* Precalculate fractional coefficients for x**n in the equations
           below to simplify the expressions for latitude and longitude. */
        var x1Frac = 1.0 / (nfpow * cf);

        nfpow *= nf; /* now equals Nf**2) */
        var x2Frac = tf / (2.0 * nfpow);

        nfpow *= nf; /* now equals Nf**3) */
        var x3Frac = 1.0 / (6.0 * nfpow * cf);

        nfpow *= nf; /* now equals Nf**4) */
        var x4Frac = tf / (24.0 * nfpow);

        nfpow *= nf; /* now equals Nf**5) */
        var x5Frac = 1.0 / (120.0 * nfpow * cf);

        nfpow *= nf; /* now equals Nf**6) */
        var x6Frac = tf / (720.0 * nfpow);

        nfpow *= nf; /* now equals Nf**7) */
        var x7Frac = 1.0 / (5040.0 * nfpow * cf);

        nfpow *= nf; /* now equals Nf**8) */
        var x8Frac = tf / (40320.0 * nfpow);

        /* Precalculate polynomial coefficients for x**n.
           -- x**1 does not have a polynomial coefficient. */
        var x2Poly = -1.0 - nuf2;

        var x3Poly = -1.0 - 2 * tf2 - nuf2;

        var x4Poly = 5.0 + 3.0 * tf2 + 6.0 * nuf2 - 6.0 * tf2 * nuf2
                     - 3.0 * (nuf2 * nuf2) - 9.0 * tf2 * (nuf2 * nuf2);

        var x5Poly = 5.0 + 28.0 * tf2 + 24.0 * tf4 + 6.0 * nuf2 + 8.0 * tf2 * nuf2;

        var x6Poly = -61.0 - 90.0 * tf2 - 45.0 * tf4 - 107.0 * nuf2
                     + 162.0 * tf2 * nuf2;

        var x7Poly = -61.0 - 662.0 * tf2 - 1320.0 * tf4 - 720.0 * (tf4 * tf2);

        var x8Poly = 1385.0 + 3633.0 * tf2 + 4095.0 * tf4 + 1575 * (tf4 * tf2);

        /* Calculate latitude */
        var lat = phif + x2Frac * x2Poly * (x * x)
                  + x4Frac * x4Poly * Math.Pow(x, 4.0)
                  + x6Frac * x6Poly * Math.Pow(x, 6.0)
                  + x8Frac * x8Poly * Math.Pow(x, 8.0);

        /* Calculate longitude */
        var lon = lambda0 + x1Frac * x
                  + x3Frac * x3Poly * Math.Pow(x, 3.0)
                  + x5Frac * x5Poly * Math.Pow(x, 5.0)
                  + x7Frac * x7Poly * Math.Pow(x, 7.0);

        return new Tuple<double, double>(lat, lon);
    }

}
}
