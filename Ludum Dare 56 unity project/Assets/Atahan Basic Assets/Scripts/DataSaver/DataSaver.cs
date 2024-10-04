using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullSerializer;
using Sirenix.Serialization;

[Serializable]
public class DataSaver {
	public static DataSaver s;

	[SerializeField]
	private SaveFile activeSave;
	const string saveName = "save.data";
	const DataFormat saveFormat = DataFormat.JSON;

	
	public bool loadingComplete = false;
	public ScriptableObjectReferenceCache cache;

	public string GetSaveFilePathAndFileName () {
		return Application.persistentDataPath + "/" + saveName;
	}

	public delegate void SaveYourself ();
	public static event SaveYourself earlyLoadEvent;
	public static event SaveYourself loadEvent;
	public static event SaveYourself earlySaveEvent;
	public static event SaveYourself saveEvent;

	public SaveFile GetCurrentSave() {
		return activeSave;
	}

	public void ClearCurrentSave() {
		Debug.Log("Clearing Save");
		activeSave = MakeNewSaveFile();
	}
	
	public SaveFile MakeNewSaveFile() {
		var file = new SaveFile();
		file.isRealSaveFile = true;
		return file;
	}


	public bool dontSave = false;
	public void SaveActiveGame () {
		if (!dontSave) {
			earlySaveEvent?.Invoke();
			saveEvent?.Invoke();
			Save();
		}
	}

	void Save() {
		var path = GetSaveFilePathAndFileName();

		activeSave.isRealSaveFile = true;
		SaveFile data = activeSave;
		
		WriteFile(path, data);
	}
	
	public void WriteFile(string path, object file) {
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		
		var context = new SerializationContext
		{
			StringReferenceResolver = cache
		};
		
		var bytes = SerializationUtility.SerializeValue(file, saveFormat, context);
		File.WriteAllBytes(path,bytes);
		
		Debug.Log($"IO OP: file \"{file.GetType()}\" saved to \"{path}\"");
	}


	public void Load () {
		if (loadingComplete) {
			return;
		}

		var path = GetSaveFilePathAndFileName();
		try {
			if (File.Exists(path)) {
				activeSave = ReadFile<SaveFile>(path);
			} else {
				Debug.Log($"No Save Data Found");
				activeSave = MakeNewSaveFile();
			}
		} catch {
			File.Delete(path);
			Debug.Log("Corrupt Data Deleted");
			activeSave = MakeNewSaveFile();
		}

		earlyLoadEvent?.Invoke();
		loadEvent?.Invoke();
		loadingComplete = true;
	}

	public T ReadFile<T>(string path) where T : class, new() {

		var context = new DeserializationContext()
		{
			StringReferenceResolver = cache
		};

		var bytes = File.ReadAllBytes(path);
		var file = SerializationUtility.DeserializeValue<T>(bytes, saveFormat, context);
		
		Debug.Log($"IO OP: file \"{file.GetType()}\" read from \"{path}\"");
		return file;
	}


	[Serializable]
	public class SaveFile {
		public bool isRealSaveFile = false;
	}
}
