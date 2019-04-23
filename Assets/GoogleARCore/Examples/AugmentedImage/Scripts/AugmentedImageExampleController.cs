//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor;
using System.Threading.Tasks;

namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;



    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class AugmentedImageExampleController : MonoBehaviour
    {
        /// <summary>
        /// A prefab for visualizing an AugmentedImage.
        /// </summary>
        public AugmentedImageVisualizer AugmentedImageVisualizerPrefab;


        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;
        public AudioClip MusicClip;
        public AudioClip upload;
        public Text text;
        public Text imageText;

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
        private List<AugmentedImage> m_AllAugmentedImages = new List<AugmentedImage>();

        private DatabaseReference reference;
        private FirebaseStorage storage;
        private StorageReference storage_ref;
        private bool[] updating = new bool[10];
        //private StorageReference audio_ref;
        private List<float[]> positionsList = new List<float[]>();
        private List<GameObject> spheresList = new List<GameObject>();
        private List<AudioFire> fireList = new List<AudioFire>();
        private List<string> keysList = new List<string>();
        private string currKey = "";
       
        private string currImage = "";

        public void Start()
        {
            Debug.Log("=========Script Loaded");
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    //   app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });

            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://reflections-51bdd.firebaseio.com/");
            // Get the root reference location of the database.
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            storage = FirebaseStorage.DefaultInstance;
            storage_ref = storage.GetReferenceFromUrl("gs://reflections-51bdd.appspot.com");
            //audio_ref = storage_ref.Child("audioTest");
            for (int i = 0; i < 10; i++) {
                updating[i] = false;
            }
        }


        /// <summary>
        /// The Unity Update method.
        /// </summary>
        public void Update()
        {

            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            // Get updated augmented images for this frame.
            Session.GetTrackables<AugmentedImage>(
                m_TempAugmentedImages, TrackableQueryFilter.Updated);

            // Create visualizers and anchors for updated augmented images that are tracking and do
            // not previously have a visualizer. Remove visualizers for stopped images.
            foreach (var image in m_TempAugmentedImages)
            {

                AugmentedImageVisualizer visualizer = null;
                //Camera.current;
                m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                if (image.TrackingState == TrackingState.Tracking && currImage != image.Name)
                {
                    currImage = image.Name;
                    imageText.text = image.Name;
                    //imageText.text = "image" + image.Name;
                    visualizer.Destroy();
                    m_Visualizers.Clear();
                    GameObject.Destroy(visualizer.gameObject);

                    float halfWidth = image.ExtentX / 2;
                    float halfHeight = image.ExtentZ / 2;
                    Anchor anchor = image.CreateAnchor(image.CenterPose);

                    visualizer = (AugmentedImageVisualizer)Instantiate(
                        AugmentedImageVisualizerPrefab, anchor.transform);
                    visualizer.Image = image;
                    visualizer.position = new float[] { 0.1f, 0.1f, 0.1f };
                    visualizer.text = text;
                    visualizer.positionsList = positionsList;
                    visualizer.spheresList = spheresList;
                    visualizer.fireList = fireList;
                    visualizer.MusicClip = MusicClip;
                    m_Visualizers.Add(image.DatabaseIndex, visualizer);

                    FirebaseDatabase.DefaultInstance
                      .GetReference("audio")
                      .GetValueAsync().ContinueWith(task => {
                          if (task.IsFaulted)
                          {
                              // Handle the error...
                          }
                          else if (task.IsCompleted)
                          {
                              DataSnapshot snapshot = task.Result;
                              DataSnapshot imageSnapshot = snapshot.Child(currImage);
                              foreach (DataSnapshot c in imageSnapshot.Children)
                              {
                                  double value0 = (double)c.Child("0").Value;
                                  double value1 = (double)c.Child("1").Value;
                                  double value2 = (double)c.Child("2").Value;
                                  positionsList.Add(new float[] { (float)value0, (float)value1, (float)value2 });
                                  keysList.Add(c.Key);
                              }
                          }
                      });

                }
                if (image.TrackingState == TrackingState.Tracking && visualizer == null)
                {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    float halfWidth = image.ExtentX / 2;
                    float halfHeight = image.ExtentZ / 2;
                    Anchor anchor = image.CreateAnchor(image.CenterPose);

                    visualizer = (AugmentedImageVisualizer)Instantiate(
                        AugmentedImageVisualizerPrefab, anchor.transform);
                    visualizer.Image = image;
                    visualizer.position = new float[] { 0.1f, 0.1f, 0.1f };
                    visualizer.text = text;
                    visualizer.positionsList = positionsList;
                    visualizer.spheresList = spheresList;
                    visualizer.fireList = fireList;
                    visualizer.keysList = keysList;
                    visualizer.MusicClip = MusicClip;
                    m_Visualizers.Add(image.DatabaseIndex, visualizer);
                    if (currImage != image.Name) {
                        visualizer.Destroy();
                        m_Visualizers.Clear();
                        GameObject.Destroy(visualizer.gameObject);
                        //GameObject.Destroy(visualizer.gameObject);
                    }
                    currImage = image.Name;
                    imageText.text = image.Name;

                    // You can convert it back to an array if you would like to


                    FirebaseDatabase.DefaultInstance
                      .GetReference("audio")
                      .GetValueAsync().ContinueWith(task => {
                          if (task.IsFaulted)
                          {
                              // Handle the error...
                          }
                          else if (task.IsCompleted)
                          {
                              DataSnapshot snapshot = task.Result;
                              foreach (DataSnapshot c in snapshot.Children)
                              {
                                  double value0 = (double) c.Child("0").Value;
                                  double value1 = (double) c.Child("1").Value;
                                  double value2 = (double) c.Child("2").Value;
                                  positionsList.Add(new float[] { (float) value0, (float) value1, (float) value2 });
                                  keysList.Add(c.Key);
                              }
                          }
                      });



                    //text.text = text.text + " hello";
                    foreach (var device in Microphone.devices)
                    {
                        text.text = text.text + " " + device; 
                    }

                    //}


                } 
                else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                {
                    m_Visualizers.Remove(image.DatabaseIndex);
                    GameObject.Destroy(visualizer.gameObject);
                    //visualizer.Destroy();
                }
            }
            // Show the fit-to-scan overlay if there are no images that are Tracking.
            foreach (var visualizer in m_Visualizers.Values)
            {
                if (visualizer.Image.TrackingState == TrackingState.Tracking)
                {
                    FitToScanOverlay.SetActive(false);
                    return;
                }
            }

            FitToScanOverlay.SetActive(true);
        }
    }
}
