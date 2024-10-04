using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullSerializer;

[Serializable]
public class DataSaver {

	public static DataSaver s;

	public SaveFile mySave;
	public const string saveName = "mySave.data";
	
	private static readonly fsSerializer _serializer = new fsSerializer();

	public string saveFilePathAndFileName {
		get { return Application.persistentDataPath + "/" + saveName; }
	}
	public delegate void SaveYourself ();
	public static event SaveYourself earlyLoadEvent;
	public static event SaveYourself loadEvent;
	public static event SaveYourself earlySaveEvent;
	public static event SaveYourself saveEvent;


	public SaveFile GetSave() {
		return mySave;
	}

	public void ClearSave() {
		Debug.Log("Clearing Save");
		mySave = new SaveFile();
		OnNewSave();
	}
	
	public void OnNewSave() {
		mySave = new SaveFile();
		mySave.isRealSaveFile = true;
	}


	public bool dontSave = false;
	public void SaveGame () {
		if (!dontSave) {
			earlySaveEvent?.Invoke();
			saveEvent?.Invoke();
			Save();
		}
	}

	void Save () {
		StreamWriter writer = new StreamWriter(saveFilePathAndFileName);
		
		mySave.isRealSaveFile = true;
		SaveFile data = mySave;

		fsData serialized;
		_serializer.TrySerialize(data, out serialized);
		var json = fsJsonPrinter.CompressedJson(serialized);
		
		writer.Write(json);
		writer.Close();
		
		Debug.Log("Data Saved to " + saveFilePathAndFileName);
	}

	public bool Load () {
		if (mySave.isRealSaveFile) {
			return true;
		}
		try {
			if (File.Exists(saveFilePathAndFileName)) {
				
				StreamReader reader = new StreamReader(saveFilePathAndFileName); 
				var json = reader.ReadToEnd();
				reader.Close();
				
				fsData serialized = fsJsonParser.Parse(json);

				_serializer.TryDeserialize(serialized, ref mySave).AssertSuccessWithoutWarnings();

				return true;
			} else {
				Debug.Log("No Data Found");
				OnNewSave();
				return false;
			}
		} catch {
			File.Delete(saveFilePathAndFileName );
			Debug.Log("Corrupt Data Deleted");
			OnNewSave();
			
			return false;
		}
		
		earlyLoadEvent?.Invoke();
		loadEvent?.Invoke();
	}

	public void DeleteSave () {
		Debug.Log("Deleting Save File at " + saveFilePathAndFileName);
		File.Delete(saveFilePathAndFileName);
		mySave = new SaveFile();
		OnNewSave();
	}

	[System.Serializable]
	public class SaveFile {
		public bool isRealSaveFile = false;

	}

	
}
