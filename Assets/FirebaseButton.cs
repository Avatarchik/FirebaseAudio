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
    private DatabaseReference reference;
    private FirebaseStorage storage;
    private StorageReference audio_ref;
    private StorageReference storage_ref;
    private float[] floatFileContents;
    private bool updating = false;
    private string key = "default";



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
    
    }

    public void testFirebase()
    {

        //Using the Buffer solution
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
                floatFileContents = new float[fileContents.Length / 4];
                Debug.Log("1");
                System.Buffer.BlockCopy(fileContents, 0, floatFileContents, 0, fileContents.Length);
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
            audioSource.clip.SetData(floatFileContents, 0);
            updating = false;
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
        audioSource.clip.SetData(floatFileContents, 0);
        Debug.Log("4");
        audioSource.Play();
    } 

    public void PushDatabase() {
        key = reference.Child("image1").Push().Key;
        reference.Child("image1").Child(key).SetValueAsync(new float[] {0, 0, 0});

        key = reference.Child("image1").Push().Key;
        reference.Child("image1").Child(key).SetValueAsync(new float[] { 0.1f, 0, 0 });

        key = reference.Child("image1").Push().Key;
        reference.Child("image1").Child(key).SetValueAsync(new float[] { 0, 0.1f, 0 });

        key = reference.Child("image1").Push().Key;
        reference.Child("image1").Child(key).SetValueAsync(new float[] { 0, 0, 0.1f });
    }

    public void PullDatabase() {
        //reference.Child("hi").Child("AudioList").SetValueAsync(AudioList);
        FirebaseDatabase.DefaultInstance
          .GetReference("image1")
          .GetValueAsync().ContinueWith(task => {
              if (task.IsFaulted)
              {
                  // Handle the error...
              }
              else if (task.IsCompleted)
              {
                  DataSnapshot snapshot = task.Result;
                  DataSnapshot child = snapshot.Child(key);
                  foreach (DataSnapshot c in snapshot.Children)
                  {


                      long value0 = (long) c.Child("0").Value;
                      long value1 = (long) c.Child("1").Value;
                      long value2 = (long) c.Child("2").Value;
                      float float0 = (float) value0;
                      Debug.Log(float0);
                      //text.text += ", " + value0;
                      
                  }
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
