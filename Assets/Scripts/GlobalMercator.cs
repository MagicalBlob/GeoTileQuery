using System;
using System.Text;
using UnityEngine;

/*
###############################################################################
# $Id$
#
# Project:  GDAL2Tiles, Google Summer of Code 2007 & 2008
#           Global Map Tiles Classes
# Purpose:  Convert a raster into TMS tiles, create KML SuperOverlay EPSG:4326,
#           generate a simple HTML viewers based on Google Maps and OpenLayers
# Author:   Klokan Petr Pridal, klokan at klokan dot cz
# Web:      http://www.klokan.cz/projects/gdal2tiles/
#
###############################################################################
# Copyright (c) 2008 Klokan Petr Pridal. All rights reserved.
#
# Permission is hereby granted, free of charge, to any person obtaining a
# copy of this software and associated documentation files (the "Software"),
# to deal in the Software without restriction, including without limitation
# the rights to use, copy, modify, merge, publish, distribute, sublicense,
# and/or sell copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
# OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
# THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
# DEALINGS IN THE SOFTWARE.
###############################################################################

globalmaptiles.py

Global Map Tiles as defined in Tile Map Service (TMS) Profiles
==============================================================

Functions necessary for generation of global tiles used on the web.
It contains classes implementing coordinate conversions for:

  - GlobalMercator (based on EPSG:900913 = EPSG:3785)
       for Google Maps, Yahoo Maps, Microsoft Maps compatible tiles
  - GlobalGeodetic (based on EPSG:4326)
       for OpenLayers Base Map and Google Earth compatible tiles

More info at:

http://wiki.osgeo.org/wiki/Tile_Map_Service_Specification
http://wiki.osgeo.org/wiki/WMS_Tiling_Client_Recommendation
http://msdn.microsoft.com/en-us/library/bb259689.aspx
http://code.google.com/apis/maps/documentation/overlays.html#Google_Maps_Coordinates

Created by Klokan Petr Pridal on 2008-07-03.
Google Summer of Code 2008, project GDAL2Tiles for OSGEO.

In case you use this class in your product, translate it to another language
or find it usefull for your project please let me know.
My email: klokan at klokan dot cz.
I would like to know where it was used.

Class is available under the open-source GDAL license (www.gdal.org).
*/

/// <summary>
/// C# Port of Global Map Tiles ( https://gist.github.com/maptiler/fddb5ce33ba995d5523de9afdf8ef118 )
/// </summary>
public class GlobalMercator
{
    /*
    TMS Global Mercator Profile
    ---------------------------

    Functions necessary for generation of tiles in Spherical Mercator projection,
    EPSG:900913 (EPSG:gOOglE, Google Maps Global Mercator), EPSG:3785, OSGEO:41001.

    Such tiles are compatible with Google Maps, Microsoft Virtual Earth, Yahoo Maps,
    UK Ordnance Survey OpenSpace API, ...
    and you can overlay them on top of base maps of those web mapping applications.
    
    Pixel and tile coordinates are in TMS notation (origin [0,0] in bottom-left).

    What coordinate conversions do we need for TMS Global Mercator tiles::

         LatLon      <->       Meters      <->     Pixels    <->       Tile     

     WGS84 coordinates   Spherical Mercator  Pixels in pyramid  Tiles in pyramid
         lat/lon            XY in metres     XY pixels Z zoom      XYZ from TMS 
        EPSG:4326           EPSG:900913                                         
         .----.              ---------               --                TMS      
        /      \     <->     |       |     <->     /----/    <->      Google    
        \      /             |       |           /--------/          QuadTree   
         -----               ---------         /------------/                   
       KML, public         WebMapService         Web Clients      TileMapService

    What is the coordinate extent of Earth in EPSG:900913?

      [-20037508.342789244, -20037508.342789244, 20037508.342789244, 20037508.342789244]
      Constant 20037508.342789244 comes from the circumference of the Earth in meters,
      which is 40 thousand kilometers, the coordinate origin is in the middle of extent.
      In fact you can calculate the constant as: 2 * math.pi * 6378137 / 2.0
      $ echo 180 85 | gdaltransform -s_srs EPSG:4326 -t_srs EPSG:900913
      Polar areas with abs(latitude) bigger then 85.05112878 are clipped off.

    What are zoom level constants (pixels/meter) for pyramid with EPSG:900913?

      whole region is on top of pyramid (zoom=0) covered by 256x256 pixels tile,
      every lower zoom level resolution is always divided by two
      initialResolution = 20037508.342789244 * 2 / 256 = 156543.03392804062

    What is the difference between TMS and Google Maps/QuadTree tile name convention?

      The tile raster itself is the same (equal extent, projection, pixel size),
      there is just different identification of the same raster tile.
      Tiles in TMS are counted from [0,0] in the bottom-left corner, id is XYZ.
      Google placed the origin [0,0] to the top-left corner, reference is XYZ.
      Microsoft is referencing tiles by a QuadTree name, defined on the website:
      http://msdn2.microsoft.com/en-us/library/bb259689.aspx

    The lat/lon coordinates are using WGS84 datum, yeh?

      Yes, all lat/lon we are mentioning should use WGS84 Geodetic Datum.
      Well, the web clients like Google Maps are projecting those coordinates by
      Spherical Mercator, so in fact lat/lon coordinates on sphere are treated as if
      the were on the WGS84 ellipsoid.
     
      From MSDN documentation:
      To simplify the calculations, we use the spherical form of projection, not
      the ellipsoidal form. Since the projection is used only for map display,
      and not for displaying numeric coordinates, we don't need the extra precision
      of an ellipsoidal projection. The spherical projection causes approximately
      0.33 percent scale distortion in the Y direction, which is not visually noticable.

    How do I create a raster in EPSG:900913 and convert coordinates with PROJ.4?

      You can use standard GIS tools like gdalwarp, cs2cs or gdaltransform.
      All of the tools supports -t_srs 'epsg:900913'.

      For other GIS programs check the exact definition of the projection:
      More info at http://spatialreference.org/ref/user/google-projection/
      The same projection is degined as EPSG:3785. WKT definition is in the official
      EPSG database.

      Proj4 Text:
        +proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0
        +k=1.0 +units=m +nadgrids=@null +no_defs

      Human readable WKT format of EPGS:900913:
         PROJCS["Google Maps Global Mercator",
             GEOGCS["WGS 84",
                 DATUM["WGS_1984",
                     SPHEROID["WGS 84",6378137,298.2572235630016,
                         AUTHORITY["EPSG","7030"]],
                     AUTHORITY["EPSG","6326"]],
                 PRIMEM["Greenwich",0],
                 UNIT["degree",0.0174532925199433],
                 AUTHORITY["EPSG","4326"]],
             PROJECTION["Mercator_1SP"],
             PARAMETER["central_meridian",0],
             PARAMETER["scale_factor",1],
             PARAMETER["false_easting",0],
             PARAMETER["false_northing",0],
             UNIT["metre",1,
                 AUTHORITY["EPSG","9001"]]]
    */

    //Initialize the TMS Global Mercator pyramid
    public const int TileSize = 256;
    private const int EarthRadius = 6378137;
    private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize; // 156543.03392804062 for tileSize 256 pixels
    private const double OriginShift = 2 * Math.PI * EarthRadius / 2.0; // 20037508.342789244

    /// <summary>
    /// Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
    /// </summary>
    /// <param name="latitude">Input latitude</param>
    /// <param name="longitude">Input longitude</param>
    /// <returns>Converted XY (meters) vector</returns>
    public static Vector2D LatLonToMeters(double latitude, double longitude)
    {
        double metersX = longitude * OriginShift / 180.0;
        double metersY = Math.Log(Math.Tan((90 + latitude) * Math.PI / 360.0)) / (Math.PI / 180.0);

        metersY = metersY * OriginShift / 180.0;
        return new Vector2D(metersX, metersY);
    }

    /// <summary>
    /// Converts XY point from Spherical Mercator EPSG:900913 to lon/lat in WGS84 Datum
    /// </summary>
    /// <param name="metersX">Input X (meters)</param>
    /// <param name="metersY">Input Y (meters)</param>
    /// <returns>Converted longitude/latitude vector</returns>
    public static Vector2D MetersToLonLat(double metersX, double metersY)
    {
        double longitude = (metersX / OriginShift) * 180.0;
        double latitude = (metersY / OriginShift) * 180.0;

        latitude = 180 / Math.PI * (2 * Math.Atan(Math.Exp(latitude * Math.PI / 180.0)) - Math.PI / 2.0);
        return new Vector2D(longitude, latitude);
    }

    /// <summary>
    /// Converts pixel coordinates in given zoom level of pyramid to EPSG:900913
    /// </summary>
    /// <param name="pixelsX">Input X (pixels)</param>
    /// <param name="pixelsY">Input Y (pixels)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted XY (meters) vector</returns>
    public static Vector2D PixelsToMeters(double pixelsX, double pixelsY, int zoom)
    {
        double resolution = Resolution(zoom);
        double metersX = pixelsX * resolution - OriginShift;
        double metersY = pixelsY * resolution - OriginShift;
        return new Vector2D(metersX, metersY);
    }

    /// <summary>
    /// Converts EPSG:900913 to pyramid pixel coordinates in given zoom level
    /// </summary>
    /// <param name="metersX">Input X (meters)</param>
    /// <param name="metersY">Input Y (meters)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted XY (pixels) vector</returns>
    public static Vector2D MetersToPixels(double metersX, double metersY, int zoom)
    {
        double resolution = Resolution(zoom);
        double pixelsX = (metersX + OriginShift) / resolution;
        double pixelsY = (metersY + OriginShift) / resolution;
        return new Vector2D(pixelsX, pixelsY);
    }

    /// <summary>
    /// Returns a tile covering region in given pixel coordinates
    /// </summary>
    /// <param name="pixelsX">Input X (pixels)</param>
    /// <param name="pixelsY">Input Y (pixels)</param>
    /// <returns>Converted XY (tile) vector</returns>
    public static Vector2Int PixelsToTile(double pixelsX, double pixelsY)
    {
        int tileX = (int)(Math.Ceiling(pixelsX / TileSize) - 1);
        int tileY = (int)(Math.Ceiling(pixelsY / TileSize) - 1);
        return new Vector2Int(tileX, tileY);
    }

    /// <summary>
    /// Move the origin of pixel coordinates to top-left corner
    /// </summary>
    /// <param name="pixelsX">Input X (pixels)</param>
    /// <param name="pixelsY">Input Y (pixels)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Moved pixel coordinates vector</returns>
    public static Vector2D PixelsToRaster(double pixelsX, double pixelsY, int zoom)
    {
        int mapSize = TileSize << zoom;
        return new Vector2D(pixelsX, mapSize - pixelsY);
    }

    /// <summary>
    /// Returns TMS tile for given mercator coordinates
    /// </summary>
    /// <param name="metersX">Input X (meters)</param>
    /// <param name="metersY">Input Y (meters)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted XY (TMS tile) vector</returns>
    public static Vector2Int MetersToTMSTile(double metersX, double metersY, int zoom)
    {
        Vector2D pixelsXPixelsY = MetersToPixels(metersX, metersY, zoom);
        return PixelsToTile(pixelsXPixelsY.X, pixelsXPixelsY.Y);
    }

    /// <summary>
    /// Returns bounds of the given TMS tile in EPSG:900913 coordinates
    /// </summary>
    /// <param name="tileX">Input X (TMS tile)</param>
    /// <param name="tileY">Input Y (TMS tile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Bounds of the given TMS tile in EPSG:900913 coordinates</returns>
    public static Bounds TMSTileBounds(int tileX, int tileY, int zoom)
    {
        return new Bounds(PixelsToMeters(tileX * TileSize, tileY * TileSize, zoom), PixelsToMeters((tileX + 1) * TileSize, (tileY + 1) * TileSize, zoom));
    }

    /// <summary>
    /// Returns bounds of the given Google tile in EPSG:900913 coordinates
    /// </summary>
    /// <param name="tileX">Input X (Google tile)</param>
    /// <param name="tileY">Input Y (Googletile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Bounds of the given Google tile in EPSG:900913 coordinates</returns>
    public static Bounds GoogleTileBounds(int tileX, int tileY, int zoom)
    {
        Vector2Int tmsCoords = GoogleTileToTMS(tileX, tileY, zoom);
        return TMSTileBounds(tmsCoords.x, tmsCoords.y, zoom);
    }

    /// <summary>
    /// Returns bounds of the given TMS tile in latitude/longitude using WGS84 datum
    /// </summary>
    /// <param name="tileX">Input X (TMS tile)</param>
    /// <param name="tileY">Input Y (TMS tile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Bounds of the given TMS tile in latitude/longitude using WGS84 datum</returns>
    public static Bounds TMSTileLatLonBounds(int tileX, int tileY, int zoom)
    {
        Bounds bounds = TMSTileBounds(tileX, tileY, zoom);
        return new Bounds(MetersToLonLat(bounds.Min.X, bounds.Min.Y), MetersToLonLat(bounds.Max.X, bounds.Max.Y));
    }

    /// <summary>
    /// Returns bounds of the given Google tile in latitude/longitude using WGS84 datum
    /// </summary>
    /// <param name="tileX">Input X (Google tile)</param>
    /// <param name="tileY">Input Y (Google tile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Bounds of the given Google tile in latitude/longitude using WGS84 datum</returns>
    public static Bounds GoogleTileLatLonBounds(int tileX, int tileY, int zoom)
    {
        Bounds bounds = GoogleTileBounds(tileX, tileY, zoom);
        return new Bounds(MetersToLonLat(bounds.Min.X, bounds.Min.Y), MetersToLonLat(bounds.Max.X, bounds.Max.Y));
    }

    /// <summary>
    /// Resolution (meters/pixel) for given zoom level (measured at Equator)
    /// </summary>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Resolution (meters/pixel) for given zoom level (measured at Equator)</returns>
    public static double Resolution(int zoom)
    {
        return InitialResolution / Math.Pow(2, zoom); // return (2 * math.pi * 6378137) / (self.tileSize * 2**zoom)
    }

    /// <summary>
    /// Maximal scaledown zoom of the pyramid closest to the pixelSize
    /// </summary>
    /// <param name="pixelSize">Pixel size</param>
    /// <returns>Maximal scaledown zoom of the pyramid closest to the pixelSize</returns>
    public static int ZoomForPixelSize(double pixelSize)
    {
        for (int i = 0; i < 30; i++)
        {
            if (pixelSize > Resolution(i))
            {
                return i != 0 ? i - 1 : 0; // We don't want to scale up
            }
        }

        throw new InvalidOperationException();
    }

    /// <summary>
    /// Converts TMS tile coordinates to Google Tile coordinates
    /// </summary>
    /// <param name="tileX">Input X (TMS tile)</param>
    /// <param name="tileY">Input Y (TMS tile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted Google Tile coordinates</returns>
    public static Vector2Int GoogleTile(int tileX, int tileY, int zoom)
    {
        return new Vector2Int(tileX, (int)(Math.Pow(2, zoom) - 1) - tileY); // coordinate origin is moved from bottom-left to top-left corner of the extent
    }

    /// <summary>
    /// Converts Google Tile coordinates to TMS tile coordinates
    /// </summary>
    /// <param name="tileX">Input X (Google tile)</param>
    /// <param name="tileY">Input Y (Google tile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted TMS tile coordinates</returns>
    public static Vector2Int GoogleTileToTMS(int tileX, int tileY, int zoom)
    {
        return new Vector2Int(tileX, (int)(Math.Pow(2, zoom) - 1) - tileY); // coordinate origin is moved from top-left to bottom-left corner of the extent
    }

    /// <summary>
    /// Returns tile for given mercator coordinates
    /// </summary>
    /// <param name="metersX">Input X (meters)</param>
    /// <param name="metersY">Input Y (meters)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted XY (Google tile) vector</returns>
    public static Vector2Int MetersToGoogleTile(double metersX, double metersY, int zoom)//(int tileX, int tileY, int zoom)
    {
        Vector2Int tileCoords = GlobalMercator.MetersToTMSTile(metersX, metersY, zoom);
        tileCoords.y = (int)(Math.Pow(2, zoom) - 1) - tileCoords.y; // coordinate origin is moved from bottom-left to top-left corner of the extent
        return tileCoords;
    }

    /// <summary>
    /// Converts TMS tile coordinates to Microsoft QuadTree
    /// </summary>
    /// <param name="tileX">Input X (TMS tile)</param>
    /// <param name="tileY">Input Y (TMS tile)</param>
    /// <param name="zoom">Zoom level</param>
    /// <returns>Converted Microsoft QuadTree key</returns>
    public static string QuadTree(int tileX, int tileY, int zoom)
    {
        StringBuilder quadKey = new StringBuilder();
        tileY = ((int)Math.Pow(2, zoom) - 1) - tileY;
        for (int i = zoom; i > 0; i--)
        {
            int digit = 0;
            int mask = 1 << (i - 1);
            if ((tileX & mask) != 0)
            {
                digit += 1;
            }
            if ((tileY & mask) != 0)
            {
                digit += 2;
            }
            quadKey.Append(digit);
        }

        return quadKey.ToString();
    }
}