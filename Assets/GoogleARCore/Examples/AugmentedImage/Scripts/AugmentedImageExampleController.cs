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

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

        private bool first = true;
        private DatabaseReference reference;
        private FirebaseStorage storage;

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
        }


        /// <summary>
        /// The Unity Update method.
        /// </summary>
        public void Update()
        {
            //Debug.Log("=========Script Loaded");
            storage = FirebaseStorage.DefaultInstance;
            //Debug.Log("=========Script Loaded");
            StorageReference storage_ref = storage.GetReferenceFromUrl("gs://reflections-51bdd.appspot.com");
            //Debug.Log("=========Script Loaded");

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
                m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                if (image.TrackingState == TrackingState.Tracking && visualizer == null)
                {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    Anchor anchor = image.CreateAnchor(image.CenterPose);
                    visualizer = (AugmentedImageVisualizer)Instantiate(
                        AugmentedImageVisualizerPrefab, anchor.transform);
                    visualizer.Image = image;
                    visualizer.MusicClip = MusicClip;
                    m_Visualizers.Add(image.DatabaseIndex, visualizer);

                    //if (first)
                    //{
                    // Set this before calling into the realtime database.
                    FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://reflections-51bdd.firebaseio.com/");
                    // Get the root reference location of the database.
                    reference = FirebaseDatabase.DefaultInstance.RootReference;
                    storage = FirebaseStorage.DefaultInstance;
                    text.text = "hi";

                    //string json = JsonUtility.ToJson(MusicClip);


                    string key = reference.Child("hi").Push().Key;
                    string key1 = reference.Child("hi").Push().Key;
                    string key2 = reference.Child("hi").Push().Key;
                    reference.Child("hi").Child(key).SetValueAsync(key);
                    reference.Child("hi").Child(key1).SetValueAsync(key1);
                    reference.Child("hi").Child(key2).SetValueAsync(key2);

                    //reference.Child("hi").Child("AudioList").SetValueAsync(AudioList);
                    FirebaseDatabase.DefaultInstance
                      .GetReference("hi")
                      .GetValueAsync().ContinueWith(task => {
                          if (task.IsFaulted)
                          {
                              // Handle the error...
                          }
                          else if (task.IsCompleted)
                          {
                              reference.Child("hi").Child("sup").SetValueAsync("new");
                              DataSnapshot snapshot = task.Result;
                              reference.Child("hi").Child("hey").SetValueAsync("new");
                              DataSnapshot child = snapshot.Child(key);
                              reference.Child("hi").Child("hello").SetValueAsync("new");
                              string s0 = child.GetRawJsonValue();
                              reference.Child("hi").Child("wha").SetValueAsync(s0);
                              reference.Child("hi").Child("why").SetValueAsync(child.Key);
                              string s1;
                              foreach (DataSnapshot c in snapshot.Children) {
                                  s1 = c.Key;
                                  reference.Child("hi").Child(s1).SetValueAsync("updated");
                              }
                          }
                      });

                    text.text = text.text + " hello";
                    foreach (var device in Microphone.devices)
                    {
                        text.text = text.text + " " + device; 
                    }
                    text.text = text.text + " hello";
                    // Get a reference to the storage service, using the default Firebase App
                    storage = FirebaseStorage.DefaultInstance;
                    visualizer.Play();
                    //StorageReference storage_ref = storage.GetReferenceFromUrl("gs://reflections-51bdd");

                    StorageReference new_ref = storage_ref.Child(key);

                    first = false;
                        
                    //}
                }
                else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                {
                    m_Visualizers.Remove(image.DatabaseIndex);
                    GameObject.Destroy(visualizer.gameObject);
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
