// Copyright 2005 Sameer Morar <smorar@gmail.com>, Carl Hultquist <chultquist@gmail.com>
// This code is licensed under the GPLv2 license. Please see the COPYING file
// for more information

using System.Collections;
using System.IO;

namespace bibliographer
{
	class BibtexRecordFieldTypeLibrary {
		private static ArrayList fields;
		
		public static int Count() {
			return fields.Count;
		}
		
		public static bool Contains(string name)
		{
			for (int i = 0; i < fields.Count; i++)
				if (((BibtexRecordFieldType) fields[i]).name == name)
					return true;
			return false;
		}
		
		public static BibtexRecordFieldType Get(string name)
		{
			for (int i = 0; i < fields.Count; i++)
				if (((BibtexRecordFieldType) fields[i]).name == name)
					return (BibtexRecordFieldType) fields[i];
			return null;
		}
			
		public static BibtexRecordFieldType GetWithIndex(int index)
		{
			if (index < 0 || index >= fields.Count)
				return null;
			return (BibtexRecordFieldType) fields[index];
		}
		
		public static void Add(BibtexRecordFieldType field)
		{
			fields.Add(field);
		}
		
		private static string Filename() {
			return System.Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer/bibliographer_fields";
		}
		
		private static string Directory() {
		  return System.Environment.GetEnvironmentVariable("HOME") + "/.config/bibliographer";
		}
		
		public static void Save()
		{
			// TODO: possibly make this safer (in case of crash during write?)

			StreamWriter stream = null;
      try {
		    stream = new StreamWriter(Filename());
		    if (stream == null) {
			   Debug.WriteLine(1, "Argh, couldn't open the file!");
		    }
	    } catch (System.IO.DirectoryNotFoundException e) {
		        Debug.WriteLine(10, e.Message);
				Debug.WriteLine(1, "Directory ~/.config/bibliographer/ not found!");
				System.IO.Directory.CreateDirectory(Directory());
			}
        
			if (stream != null) {
				// good to go
				for (int i = 0; i < fields.Count; i++) {
					BibtexRecordFieldType field = (BibtexRecordFieldType) fields[i];
					stream.WriteLine(field.name);
					stream.WriteLine(field.description);
					stream.WriteLine(field.spec ? 1 : 0);
					stream.WriteLine();
				}
				stream.Close();
			}
		}
		
		public static void Load()
		{
			fields = new ArrayList();

			StreamReader stream = null;
			do {
			  try {
				  stream = new StreamReader(Filename());
				  if (stream == null) {
					  Debug.WriteLine(1, "Argh, couldn't open the file!");
				  }
				  break;
			  } catch (System.IO.DirectoryNotFoundException e) {
  		          Debug.WriteLine(10, e.Message);
				  Debug.WriteLine(1, "Directory ~/.config/bibliographer/ not found! Creating it...");
          System.IO.Directory.CreateDirectory(Directory());          
			  } catch (System.IO.FileNotFoundException e) {
  		          Debug.WriteLine(10, e.Message);
				  Debug.WriteLine(1, "File ~/.config/bibliographer/bibliographer_fields not found! Instantiating it...");
				  System.IO.Stream recStream = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream("bibliographer_fields");
				  System.IO.FileStream outRecStream = new FileStream(Filename(), FileMode.CreateNew);
				  byte[] data = new byte[recStream.Length];
				  recStream.Read(data, 0, (int) recStream.Length);
				  recStream.Close();
				  outRecStream.Write(data, 0, data.Length);
				  outRecStream.Close();				  
			  }
			} while (true);
			
			if (stream != null) {
				do {
					string fieldName = stream.ReadLine();
					if (fieldName == null)
						break;
					string description = stream.ReadLine();
					if (description == null)
						break;
					string spec = stream.ReadLine();
					stream.ReadLine(); // blank line between records
					
					BibtexRecordFieldType field = new BibtexRecordFieldType();
					field.name = fieldName;
					field.description = description;
					field.spec = (System.Convert.ToInt32(spec) == 1);

					fields.Add(field);
					
					Debug.WriteLine(5,"Read in info for field '" + fieldName + "'");
				} while (true);
				stream.Close();
			}	
		}
	}
}
