using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using UnityEngine.UI;
using GoogleARCore;

public class FirebaseButton : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip audioClipDefault;
    public AudioClip audioClipFromDB;
    public Text text;
    public Text uploading;

    public Text downloading;
    public Text imageText;
    public Text dataText;

    private DatabaseReference reference;
    private FirebaseStorage storage;
    private StorageReference audio_ref;
    private StorageReference storage_ref;
    private bool updating;

    private bool updatingPosition = false;
    private bool updatingAudio = false;
    private bool updatingAll = false;
    private bool updatingAllFirst = false;
    private bool updatingAllDone = false;

    private List<float[]> positionsList = new List<float[]>();
    private List<float[]> audioList = new List<float[]>();
    private List<string> keysList = new List<string>();
    private string error = "";
    private float[] position;
    private float[] audioData;
    private string imageNameForGetAll;



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("FIrebaseButton Script Loaded");
        audioSource.clip = audioClipDefault;
        //audioSource.Play();

        // Get a reference to the storage service, using the default Firebase App
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://reflections-51bdd.firebaseio.com/");

        // Get the root reference location of the database.
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        storage = FirebaseStorage.DefaultInstance;
        storage_ref = storage.GetReferenceFromUrl("gs://reflections-51bdd.appspot.com");
        audio_ref = storage_ref.Child("audioTest");
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (updating) {
            dataText.text += error;
            for (int i = 0; i < keysList.Count; i++) {
                dataText.text += " " + keysList[i] + ": ";
                dataText.text += positionsList[i][0] + ", " + positionsList[i][1] + ", " + positionsList[i][2];
            }
            updating = false;
        }
        */
        if (keysList.Count == audioList.Count && updatingAll) {
            updatingAllDone = true;
        }

        if (updatingAudio) {


            updatingAudio = false;
        }

        if (updatingPosition) {


            updatingPosition = false;
        }

        if (updatingAllFirst)
        {
            GetAllAudio(imageNameForGetAll);
            updatingAllFirst = false;
            updatingAll = true;
        }

        if (updatingAllDone)
        {
            updatingAllDone = false;
            updatingAll = false;
        }

    }

    public void GetAudioAndPosition(string key, string imageName)
    {
        FirebaseDatabase.DefaultInstance
                        .GetReference(imageName)
          .GetValueAsync().ContinueWith(task => {
              if (task.IsFaulted)
              {
                  error = "Error!";
              }
              else if (task.IsCompleted)
              {
                  DataSnapshot snapshot = task.Result;
                  DataSnapshot c = snapshot.Child(key);
                  double value0 = (double)c.Child("0").Value;
                  double value1 = (double)c.Child("1").Value;
                  double value2 = (double)c.Child("2").Value;
                  position = new float[] { (float)value0, (float)value1, (float)value2 };
                  updatingPosition = true;
              }
          });

        byte[] fileContents = { };
        const long maxAllowedSize = 34008512;

        audio_ref = storage_ref.Child(key);
        audio_ref.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Downloading Failed");
                Debug.Log(task.Exception.ToString());
                // Uh-oh, an error occurred!
            }
            else
            {

                fileContents = task.Result;
                audioData = new float[fileContents.Length / 4];
                System.Buffer.BlockCopy(fileContents, 0, audioData, 0, fileContents.Length);
                updatingAudio = true;
            }
        });
    }

    public void SendAudioAndPosition(string imageName, float[] audio, float[] position) {
        string key = reference.Child(imageText.text).Push().Key;
        reference.Child(imageName).Child(key).SetValueAsync(position);
        SendAudio(key, audio);
    }

    public void SendAudioAndPosition(string key, string imageName, float[] audio, float[] position)
    {
        reference.Child(imageName).Child(key).SetValueAsync(position);
        SendAudio(key, audio);
    }


    private void SendAudio(string key, float[] audio)
    {
        //Using the Buffer solution
        audio_ref = storage_ref.Child(key);
        byte[] byteSamples = new byte[audioClipFromDB.samples * 4];

        System.Buffer.BlockCopy(audio, 0, byteSamples, 0, byteSamples.Length);
        audio_ref.PutBytesAsync(byteSamples)
          .ContinueWith((Task<StorageMetadata> task) => {
              if (task.IsFaulted || task.IsCanceled)
              {
                  //audioSource.Play();
                  Debug.Log("Uploading Failed");
                  Debug.Log(task.Exception.ToString());

                  // Uh-oh, an error occurred!
              }
              else
              {
                  Debug.Log("Upload Success");
                  Firebase.Storage.StorageMetadata metadata = task.Result;
                  string download_url = metadata.Reference.GetDownloadUrlAsync().ToString();

              }
          });
    }

    public void GetAllAudiosAndPositions(string imageName) {
        positionsList.Clear();
        keysList.Clear();
        audioList.Clear();
        FirebaseDatabase.DefaultInstance
                        .GetReference(imageName)
          .GetValueAsync().ContinueWith(task => {
              if (task.IsFaulted)
              {
                  error = "Error!";
              }
              else if (task.IsCompleted)
              {
                  DataSnapshot snapshot = task.Result;
                  foreach (DataSnapshot c in snapshot.Children)
                  {
                      double value0 = (double)c.Child("0").Value;
                      double value1 = (double)c.Child("1").Value;
                      double value2 = (double)c.Child("2").Value;
                      positionsList.Add(new float[] { (float)value0, (float)value1, (float)value2 });
                      keysList.Add(c.Key);
                  }
                  updatingAll = true;
                  imageNameForGetAll = imageName;
              }
          });
    }

    private void GetAllAudio(string imageName) {
        byte[] fileContents = { };
        const long maxAllowedSize = 34008512;
        foreach (string key in keysList) {
            audio_ref = storage_ref.Child(key);
            audio_ref.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("Downloading Failed");
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else
                {

                    fileContents = task.Result;
                    float[] newAudioData = new float[fileContents.Length / 4];
                    System.Buffer.BlockCopy(fileContents, 0, audioData, 0, fileContents.Length);
                    audioList.Add(newAudioData);
                }
            });
        }
    }



    public void testFirebase(string key)
    {
        //Using the Buffer solution
        audio_ref = storage_ref.Child(key);
        uploading.text = "Uploading";
        float[] samples = new float[audioClipFromDB.samples];
        audioClipFromDB.GetData(samples, 0);
        byte[] byteSamples = new byte[audioClipFromDB.samples * 4];

        System.Buffer.BlockCopy(samples, 0, byteSamples, 0, byteSamples.Length);
        audio_ref.PutBytesAsync(byteSamples)
          .ContinueWith((Task<StorageMetadata> task) => {
              if (task.IsFaulted || task.IsCanceled)
              {
                  //audioSource.Play();
                  Debug.Log("Uploading Failed");
                  Debug.Log(task.Exception.ToString());
                  
                  // Uh-oh, an error occurred!
              }
              else
              {
                  // Metadata contains file metadata such as size, content-type, and download URL.
                  Debug.Log("Upload Success");
                  //audioSource.Play();
                  Firebase.Storage.StorageMetadata metadata = task.Result;
                  //audioSource.Play();
                  string download_url = metadata.Reference.GetDownloadUrlAsync().ToString();
                  Debug.Log("Finished uploading...");
                  Debug.Log("download url = " + download_url);
                  uploading.text = "Upload";

              }
          });
        //audioSource.Play();
    }

    public void testDownload()
    {
        downloading.text = "Downloading";
        byte[] fileContents = { };
        const long maxAllowedSize = 34008512;

        audio_ref.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Downloading Failed");
                Debug.Log(task.Exception.ToString());
                // Uh-oh, an error occurred!
            }
            else
            {

                fileContents = task.Result;
                Debug.Log("Finished downloading!");
                audioData = new float[fileContents.Length / 4];
                Debug.Log("1");
                System.Buffer.BlockCopy(fileContents, 0, audioData, 0, fileContents.Length);
                //audioSource.Play();
                Debug.Log("2");
                updating = true;
                Debug.Log("3");
                downloading.text = "Download";
            }
        });

        /*
        float[] floatFileContents = new float[fileContents.Length / 4];

        System.Buffer.BlockCopy(fileContents, 0, floatFileContents, 0, fileContents.Length);
        //audioSource.Play();

        float[] audioSample = new float[audioSource.clip.samples];
        audioSource.Play();
        for (int i = 0; i < audioSample.Length; i ++) {

            audioSample[i] = floatFileContents[i];
        }
        */
        //audioSource.Play();

        //audioSource.Play();
        //AudioClip downloadedAudio = AudioClip.Create("new", floatFileContents.Length, 4, 1, false);
        //audioSource.Play();
        //downloadedAudio.SetData(floatFileContents, 0);

        //audioSource.clip = downloadedAudio;
        //audioSource.Play();
    }

    public void Play()
    {
        //audioSource.Play();
        Debug.Log("4");
        if(updating) {
            audioSource.clip.SetData(audioData, 0);
            updatingAudio = false;
        }
        Debug.Log("5");
        audioSource.Play();
        Debug.Log("6");
    }

    private void SetAudio() {
        /*
        for (int i = 0; i < audioSample.Length; i++)
        {

            audioSample[i] = floatFileContents[i];
        }
        */
        Debug.Log("3");
        audioSource.clip.SetData(audioData, 0);
        Debug.Log("4");
        audioSource.Play();
    } 

    public void PushDatabase() {
        string key = reference.Child(imageText.text).Push().Key;
        reference.Child(imageText.text).Child(key).SetValueAsync(new float[] {0.1f, 0, 0});
        testFirebase(key);

        key = reference.Child(imageText.text).Push().Key;
        reference.Child(imageText.text).Child(key).SetValueAsync(new float[] { 0, 0.1f, 0 });
        testFirebase(key);

        key = reference.Child(imageText.text).Push().Key;
        reference.Child(imageText.text).Child(key).SetValueAsync(new float[] { 0, 0, 0.1f });
        testFirebase(key);

        key = reference.Child(imageText.text).Push().Key;
        reference.Child(imageText.text).Child(key).SetValueAsync(new float[] { 0.1f, 0.1f, 0.1f });
        testFirebase(key);

    }

    public void PullDatabase() {
        //reference.Child("hi").Child("AudioList").SetValueAsync(AudioList);
        //updating = true;
        FirebaseDatabase.DefaultInstance
                        .GetReference(imageText.text)
          .GetValueAsync().ContinueWith(task => {
              if (task.IsFaulted)
              {
                  error = "Error!";
              }
              else if (task.IsCompleted)
              {
                  DataSnapshot snapshot = task.Result;
                  foreach (DataSnapshot c in snapshot.Children)
                  {
                      double value0 = (double) c.Child("0").Value;
                      double value1 = (double) c.Child("1").Value;
                      double value2 = (double) c.Child("2").Value;
                      positionsList.Add(new float[] { (float)value0, (float)value1, (float)value2 });
                      //positionsList.Add(new long[] { value0, value1, value2 });
                      keysList.Add(c.Key);
                      //text.text += ", " + value0;
                  }
                  updating = true;
              }
          });

    }

    public void StartRecord() {
        text.text = "Microphones: ";
        audioSource.clip = Microphone.Start(null, false, 10, 44100);
        foreach (var device in Microphone.devices)
        {
            //Debug.Log("Name: " + device);
            text.text = text.text + device;
        }

    }



}
