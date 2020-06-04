﻿using Darwin.Helpers;
using Darwin.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Darwin.Database
{
    public static class CatalogSupport
    {
		public const string FinzDatabaseFilename = "database.db";

		public const string SurveyAreasFolderName = "surveyAreas";
		public const string CatalogFolderName = "catalog";
		public const string TracedFinsFolderName = "tracedFins";
		public const string MatchQueuesFolderName = "matchQueues";
		public const string MatchQResultsFolderName = "matchQResults";
		public const string SightingsFolderName = "sightings";

        public static DarwinDatabase OpenDatabase(string databaseFilename, Options o, bool create, string area = "default")
        {
			CatalogScheme cat = new CatalogScheme();
			DarwinDatabase db = null;

			if (create)
			{
				RebuildFolders(o.CurrentDataPath, area);
				//// should ONLY end up here with IFF we are NOT converting an old database
				//int id = o.CurrentDefaultCatalogScheme;
				//cat.SchemeName = o.DefinedCatalogSchemeName[id];
				//cat.CategoryNames = o.DefinedCatalogCategoryName[id]; // this is a vector
			}

			db = new SQLiteDatabase(databaseFilename, o, o.CatalogSchemes[o.DefaultCatalogScheme], create);

			return db;
		}

		public static void UpdateFinFieldsFromImage(string basePath, DatabaseFin fin)
        {
			if (fin.Version < 2.0m)
			{
				List<ImageMod> imageMods;
				bool thumbOnly;
				string originalFilename;
				float normScale;

				string fullFilename = Path.Combine(basePath, fin.ImageFilename);

				PngHelper.ParsePngText(fullFilename, out normScale, out imageMods, out thumbOnly, out originalFilename);

				fin.ImageMods = imageMods;
				fin.Scale = normScale;

				// This is a little hacky, but we're going to get the bottom directory name, and append that to
				// the filename below.
				var bottomDirectoryName = Path.GetFileName(Path.GetDirectoryName(fullFilename));

				originalFilename = Path.Combine(bottomDirectoryName, originalFilename);

				// TODO Original isn't right -- need to replay imagemods, maybe?
				fin.OriginalImageFilename = originalFilename;
				fin.ImageFilename = originalFilename;
				fin.Version = 2.0m;
				// TODO: Save these changes back to the database
			}
		}

		public static DatabaseFin OpenFinz(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException(nameof(filename));

			string uniqueDirectoryName = Path.GetFileName(filename).Replace(".", string.Empty) + "_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
			string fullDirectoryName = Path.Combine(Path.GetTempPath(), uniqueDirectoryName);

			try
			{ 
				Directory.CreateDirectory(fullDirectoryName);

				ZipFile.ExtractToDirectory(filename, fullDirectoryName);

				string dbFilename = Path.Combine(fullDirectoryName, FinzDatabaseFilename);

				if (!File.Exists(dbFilename))
					return null;

                var db = OpenDatabase(dbFilename, Options.CurrentUserOptions, false);

                // First and only fin
                var fin = db.getItem(0);

                var baseimgfilename = Path.GetFileName(fin.ImageFilename);
                fin.ImageFilename = Path.Combine(fullDirectoryName, baseimgfilename);

				List<ImageMod> imageMods;
				bool thumbOnly;
				string originalFilename;
				float normScale;

				PngHelper.ParsePngText(fin.ImageFilename, out normScale, out imageMods, out thumbOnly, out originalFilename);
				
				fin.ImageMods = imageMods;
				fin.Scale = normScale;

				// TODO: Do something with thumbOnly?

				// We're loading the image this way because Bitmap keeps a lock on the original file, and
				// we want to try to delete the file below.  So we open the file in another object in a using statement
				// then copy it over to our actual working object.
				using (var imageFromFile = (Bitmap)Image.FromFile(fin.ImageFilename))
				{
					fin.ModifiedFinImage = new Bitmap(imageFromFile);
				}

				if (!string.IsNullOrEmpty(originalFilename))
				{
					fin.OriginalImageFilename = Path.Combine(fullDirectoryName, Path.GetFileName(originalFilename));

					using (var originalImageFromFile = (Bitmap)Image.FromFile(fin.OriginalImageFilename))
					{
						fin.mFinImage = new Bitmap(originalImageFromFile);
					}
				}

                return fin;
            }
			catch
            {
				// TODO: Probably should have better handling here
				return null;
            }
			finally
            {
				try
				{
					Trace.WriteLine("Trying to remove temporary files for finz file.");

					SQLiteConnection.ClearAllPools();

					GC.Collect();
					GC.WaitForPendingFinalizers();

					if (Directory.Exists(fullDirectoryName))
						Directory.Delete(fullDirectoryName, true);
				}
				catch (Exception ex)
                {
					Trace.Write("Couldn't remove temporary files:");
					Trace.WriteLine(ex);
                }
			}
		}

		public static void RebuildFolders(string home, string area)
		{
			if (string.IsNullOrEmpty(home))
				throw new ArgumentNullException(nameof(home));

			if (string.IsNullOrEmpty(area))
				throw new ArgumentNullException(nameof(area));

			Trace.WriteLine("Creating folders...");

			var surveyAreasPath = Path.Combine(new string[] { home, SurveyAreasFolderName, area });

			// Note that CreateDirectory won't do anything if the path already exists, so no need
			// to check first.
			Directory.CreateDirectory(surveyAreasPath);
			Directory.CreateDirectory(Path.Combine(surveyAreasPath, CatalogFolderName));
			Directory.CreateDirectory(Path.Combine(surveyAreasPath, TracedFinsFolderName));
			Directory.CreateDirectory(Path.Combine(surveyAreasPath, MatchQueuesFolderName));
			Directory.CreateDirectory(Path.Combine(surveyAreasPath, MatchQResultsFolderName));
			Directory.CreateDirectory(Path.Combine(surveyAreasPath, SightingsFolderName));
		}
	}
}
